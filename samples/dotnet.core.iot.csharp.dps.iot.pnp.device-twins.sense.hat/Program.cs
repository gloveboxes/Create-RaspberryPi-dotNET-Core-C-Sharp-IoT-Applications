using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using Iot.Device.CpuTemperature;
using Newtonsoft.Json;
using Iot.Device.SenseHat;
using System.Drawing;

// Sample based on Azure IoT C# SDK Samples. 
// https://github.com/Azure-Samples/azure-iot-samples-csharp

namespace dotnet.core.iot
{
    class Program
    {
        static SenseHatLedMatrixI2c ledMatrix = new SenseHatLedMatrixI2c();
        private static string idScope = Environment.GetEnvironmentVariable("DPS_IDSCOPE");
        private static string registrationId = Environment.GetEnvironmentVariable("DPS_REGISTRATION_ID");
        private static string primaryKey = Environment.GetEnvironmentVariable("DPS_PRIMARY_KEY");
        private static string secondaryKey = Environment.GetEnvironmentVariable("DPS_SECONDARY_KEY");
        private const string GlobalDeviceEndpoint = "global.azure-devices-provisioning.net";
        static CpuTemperature _temperature = new CpuTemperature();
        static int _msgId = 0;
        static double targetTemperature = 0;
        enum RoomAction { Unknown, Heating, Cooling, Green }
        static RoomAction previousRoomState = RoomAction.Unknown;
        static RoomAction roomState = RoomAction.Unknown;
        private const string ModelId = "dtmi:com:example:Thermostat;1";
        static DeviceClient iotClient = null;
        static double temperature;
        static double maxTemperature = double.MinValue;


        static async Task Main(string[] args)
        {
            using (var security = new SecurityProviderSymmetricKey(registrationId, primaryKey, secondaryKey))
            using (var transport = new ProvisioningTransportHandlerMqtt())
            {
                ProvisioningDeviceClient provClient = ProvisioningDeviceClient.Create(GlobalDeviceEndpoint, idScope, security, transport);
                var pnpPayload = new ProvisioningRegistrationAdditionalData
                {
                    JsonData = $"{{ \"modelId\": \"{ModelId}\" }}",
                };

                DeviceRegistrationResult result = await provClient.RegisterAsync(pnpPayload);
                IAuthenticationMethod auth = new DeviceAuthenticationWithRegistrySymmetricKey(result.DeviceId, (security as SecurityProviderSymmetricKey).GetPrimaryKey());

                using (iotClient = DeviceClient.Create(result.AssignedHub, auth, TransportType.Mqtt))
                {
                    await iotClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertyChangedAsync, null).ConfigureAwait(false);   // callback for Device Twin updates
                    await DeviceTwinGetInitialState(iotClient); // Get current cloud state of the device twin

                    while (true)
                    {
                        if (_temperature.IsAvailable)
                        {
                            try
                            {
                                temperature = Math.Round(_temperature.Temperature.DegreesCelsius, 2);

                                Console.WriteLine($"The CPU temperature is {temperature}");

                                await SendMsgIotHub(iotClient, temperature);

                                roomState = (int)temperature > targetTemperature ? RoomAction.Cooling : (int)temperature < targetTemperature ? RoomAction.Heating : RoomAction.Green;
                                await UpdateRoomAction(roomState);

                                if (temperature > maxTemperature)
                                {
                                    maxTemperature = temperature;
                                    await UpdateDeviceTwin("maxTempSinceLastReboot", maxTemperature);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("exception msg: " + ex.Message);
                            }
                        }
                        Thread.Sleep(2000); // sleep for 2 seconds
                    }
                }
            }
        }

        private static async Task SendMsgIotHub(DeviceClient iotClient, double temperature)
        {
            var telemetry = new Telemetry() { Temperature = temperature, MessageId = _msgId++ };
            string json = JsonConvert.SerializeObject(telemetry);

            Message eventMessage = new Message(Encoding.UTF8.GetBytes(json));

            await iotClient.SendEventAsync(eventMessage).ConfigureAwait(false);
        }

        private static async Task DeviceTwinGetInitialState(DeviceClient iotClient)
        {
            Twin twin = await iotClient.GetTwinAsync().ConfigureAwait(false);
            if (twin.Properties.Desired.Contains("targetTemperature"))
            {
                double.TryParse(Convert.ToString(twin.Properties.Desired["targetTemperature"]), out targetTemperature);
            }
        }

        private static async Task OnDesiredPropertyChangedAsync(TwinCollection desiredProperties, object userContext)
        {
            if (desiredProperties.Contains("targetTemperature"))
            {
                double.TryParse(Convert.ToString(desiredProperties["targetTemperature"]), out targetTemperature);
                await UpdateDeviceTwin("targetTemperature", targetTemperature);

                roomState = (int)temperature > targetTemperature ? RoomAction.Cooling : (int)temperature < targetTemperature ? RoomAction.Heating : RoomAction.Green;
                await UpdateRoomAction(roomState);
            }
        }

        private static async Task UpdateDeviceTwin(string propertyName, object value)
        {
            TwinCollection reportedProperties = new TwinCollection();
            reportedProperties[propertyName] = value;
            await iotClient.UpdateReportedPropertiesAsync(reportedProperties).ConfigureAwait(false);
        }

        private static async Task UpdateRoomAction(RoomAction roomState)
        {
            if (roomState != previousRoomState)
            {
                await UpdateDeviceTwin("RoomAction", roomState.ToString());
                previousRoomState = roomState;

                switch (roomState)
                {
                    case RoomAction.Cooling:
                        ledMatrix.Fill(Color.Blue);
                        break;
                    case RoomAction.Heating:
                        ledMatrix.Fill(Color.Red);
                        break;
                    default:
                        ledMatrix.Fill(Color.Green);
                        break;
                }
            }
        }

        class Telemetry
        {
            [JsonPropertyAttribute(PropertyName = "temperature")]
            public double Temperature { get; set; } = 0;

            [JsonPropertyAttribute(PropertyName = "MsgId")]
            public int MessageId { get; set; } = 0;
        }
    }
}