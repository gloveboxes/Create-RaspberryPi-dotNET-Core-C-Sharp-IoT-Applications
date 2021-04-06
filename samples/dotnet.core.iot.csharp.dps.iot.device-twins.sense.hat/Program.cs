using System;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using Iot.Device.CpuTemperature;
using Iot.Device.SenseHat;
using Newtonsoft.Json;

// Sample based on Azure IoT C# SDK Samples. 
// https://github.com/Azure-Samples/azure-iot-samples-csharp

namespace dotnet.core.iot
{
    class Program
    {
        private static string idScope = Environment.GetEnvironmentVariable("DPS_IDSCOPE");
        private static string registrationId = Environment.GetEnvironmentVariable("DPS_REGISTRATION_ID");
        private static string primaryKey = Environment.GetEnvironmentVariable("DPS_PRIMARY_KEY");
        private static string secondaryKey = Environment.GetEnvironmentVariable("DPS_SECONDARY_KEY");
        private const string GlobalDeviceEndpoint = "global.azure-devices-provisioning.net";
        static CpuTemperature _temperature = new CpuTemperature();
        static SenseHatLedMatrixI2c ledMatrix = new SenseHatLedMatrixI2c();
        static int _msgId = 0;
        static int thermostat = 0;
        enum RoomAction { Unknown, Heating, Cooling, Green }
        static RoomAction previousRoomState = RoomAction.Unknown;
        static int previousRoomTemperature = 0;
        static RoomAction roomState = RoomAction.Unknown;
        static DeviceClient iotClient = null;


        static async Task Main(string[] args)
        {
            double temperature;

            using (var security = new SecurityProviderSymmetricKey(registrationId, primaryKey, secondaryKey))
            using (var transport = new ProvisioningTransportHandlerMqtt())
            {
                ProvisioningDeviceClient provClient = ProvisioningDeviceClient.Create(GlobalDeviceEndpoint, idScope, security, transport);
                DeviceRegistrationResult result = await provClient.RegisterAsync();
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
                                await SendMsgIotHub(temperature);

                                roomState = (int)temperature > thermostat ? RoomAction.Cooling : (int)temperature < thermostat ? RoomAction.Heating : RoomAction.Green;
                                await UpdateRoomAction(roomState);
                                await UpdateRoomTemperature(temperature);

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("exception msg: " + ex.Message);
                            }
                        }
                        Thread.Sleep(4000); // sleep for 10 seconds
                    }
                }
            }
        }

        private static async Task SendMsgIotHub(double temperature)
        {
            var telemetry = new Telemetry() { Temperature = Math.Round(temperature, 2), MessageId = _msgId++ };
            string json = JsonConvert.SerializeObject(telemetry);

            Message eventMessage = new Message(Encoding.UTF8.GetBytes(json));

            await iotClient.SendEventAsync(eventMessage).ConfigureAwait(false);
        }

        private static async Task DeviceTwinGetInitialState(DeviceClient iotClient)
        {
            Twin twin = await iotClient.GetTwinAsync().ConfigureAwait(false);
            if (twin.Properties.Desired.Contains("Thermostat"))
            {
                int.TryParse(Convert.ToString(twin.Properties.Desired["Thermostat"]["value"]), out thermostat);
            }
        }

        private static async Task OnDesiredPropertyChangedAsync(TwinCollection desiredProperties, object userContext)
        {
            if (desiredProperties.Contains("Thermostat"))
            {
                int.TryParse(Convert.ToString(desiredProperties["Thermostat"]["value"]), out thermostat);
                await UpdateDeviceTwin("Thermostat", thermostat);
            }
        }

        private static async Task UpdateRoomTemperature(double temperature)
        {
            if (previousRoomTemperature != (int)temperature)
            {
                await UpdateDeviceTwin("RoomTemperature", temperature);
                previousRoomTemperature = (int)temperature;
            }
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

        private static async Task UpdateDeviceTwin(string propertyName, object value)
        {
            TwinCollection reportedProperties = new TwinCollection();
            reportedProperties[propertyName] = value;
            await iotClient.UpdateReportedPropertiesAsync(reportedProperties).ConfigureAwait(false);
        }

        class Telemetry
        {
            [JsonPropertyAttribute(PropertyName = "Temperature")]
            public double Temperature { get; set; } = 0;

            [JsonPropertyAttribute(PropertyName = "MsgId")]
            public int MessageId { get; set; } = 0;
        }
    }
}