using System;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Iot.Device.CpuTemperature;
using Iot.Device.SenseHat;
using Iot.Device.Common;
using Newtonsoft.Json;

// Sample based on Azure IoT C# SDK Samples. 
// https://github.com/Azure-Samples/azure-iot-samples-csharp

namespace dotnet.core.iot
{
    class Program
    {
        const string DeviceConnectionString = "<Your Azure IoT Hub Connection String>";

        // https://github.com/Azure/opendigitaltwins-dtdl
        // https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md#telemetry

        const string plugAndPlayModelId = "dtmi:Glovebox:Hvac;1";
        static ClientOptions options = new ClientOptions() { ModelId = plugAndPlayModelId };
        static DeviceClient iotClient = DeviceClient.CreateFromConnectionString(DeviceConnectionString, TransportType.Mqtt, options);
        static CpuTemperature _temperature = new CpuTemperature();
        static SenseHatLedMatrixI2c ledMatrix = new SenseHatLedMatrixI2c();
        static SenseHatTemperatureAndHumidity th = new SenseHatTemperatureAndHumidity();
        static int _msgId = 0;
        static int thermostat = 0;
        enum RoomAction { Unknown, Heating, Cooling, Green }
        static RoomAction previousRoomState = RoomAction.Unknown;
        static int previousRoomTemperature = 0;
        static RoomAction roomState = RoomAction.Unknown;

        static async Task Main(string[] args)
        {
            double temperature, humidity, pressure;
            var rand = new Random();

            await iotClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertyChangedAsync, null).ConfigureAwait(false);   // callback for Device Twin updates
            await DeviceTwinGetInitialState(iotClient); // Get current cloud state of the device twin

            while (true)
            {
                temperature = Math.Round(th.Temperature.DegreesCelsius, 1);
                humidity = Math.Round(th.Humidity, 1);
                pressure = 1000 + rand.Next(0, 200);

                try
                {
                    Console.WriteLine($"Ambient temperature {temperature} and humidity {humidity}");
                    await SendMsgIotHub(temperature, humidity, pressure);

                    roomState = (int)temperature > thermostat ? RoomAction.Cooling : (int)temperature < thermostat ? RoomAction.Heating : RoomAction.Green;
                    await UpdateRoomAction(roomState);
                    await UpdateRoomTemperature(temperature);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("exception msg: " + ex.Message);
                }

                Thread.Sleep(4000); // sleep for 4 seconds
            }
        }

        private static async Task SendMsgIotHub(double temperature, double humidity, double pressure)
        {
            var telemetry = new Telemetry() { Temperature = temperature, Humidity = humidity, Pressure = pressure, MessageId = _msgId++ };
            string json = JsonConvert.SerializeObject(telemetry);

            Message eventMessage = new Message(Encoding.UTF8.GetBytes(json));

            // add application property message metadata
            eventMessage.Properties.Add("appid", "hvac");
            eventMessage.Properties.Add("type", "telemetry");
            eventMessage.Properties.Add("version", "1");
            eventMessage.Properties.Add("format", "json");
            eventMessage.Properties.Add("msgid", _msgId.ToString());
            eventMessage.Properties.Add("temperatureAlert", (temperature > thermostat) ? "true" : "false");

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
                if (int.TryParse(Convert.ToString(desiredProperties["Thermostat"]["value"]), out thermostat))
                {
                    await UpdateDeviceTwin("ThermostatSetting", thermostat);
                }
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

            [JsonPropertyAttribute(PropertyName = "Humidity")]
            public double Humidity { get; set; } = 0;

            [JsonPropertyAttribute(PropertyName = "Pressure")]
            public double Pressure { get; set; } = 0;

            [JsonPropertyAttribute(PropertyName = "MsgId")]
            public int MessageId { get; set; } = 0;
        }
    }
}