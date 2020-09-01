using System;
using Iot.Device.CpuTemperature;
using Newtonsoft.Json;
using Microsoft.Azure.Devices.Client;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dotnet.core.iot
{
    class Program
    {
        const string DeviceConnectionString = "<Your Azure IoT Hub Connection String>";

        // https://github.com/Azure/opendigitaltwins-dtdl
        // https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md#telemetry


        const string plugAndPlayModelId = "dtmi:Glovebox:Hvac;1";
        static ClientOptions options = new ClientOptions() { ModelId = plugAndPlayModelId };
        static DeviceClient _deviceClient = DeviceClient.CreateFromConnectionString(DeviceConnectionString, TransportType.Mqtt, options);
        static CpuTemperature _temperature = new CpuTemperature();
        static int _msgId = 0;
        const double TemperatureThreshold = 42.0;

        static async Task Main(string[] args)
        {
            double temperature, humidity, pressure;
            var rand = new Random();

            while (true)
            {
                if (_temperature.IsAvailable) { temperature = Math.Round(_temperature.Temperature.DegreesCelsius, 2); }
                else { temperature = rand.Next(15, 35); }

                humidity = 40 + rand.Next(0, 60);
                pressure = 1000 + rand.Next(0, 200);

                Console.WriteLine($"The CPU temperature is {temperature}");

                await SendMsgIotHub(temperature, humidity, pressure);

                Thread.Sleep(4000); // sleep for 4 seconds
            }
        }

        private static async Task SendMsgIotHub(double temperature, double humidity, double pressure)
        {
            var telemetry = new Telemetry() { Temperature = temperature, Humidity = humidity, Pressure = pressure, MessageId = _msgId++ };
            string json = JsonConvert.SerializeObject(telemetry);

            Message eventMessage = new Message(Encoding.UTF8.GetBytes(json));
            eventMessage.Properties.Add("temperatureAlert", (temperature > TemperatureThreshold) ? "true" : "false");

            await _deviceClient.SendEventAsync(eventMessage).ConfigureAwait(false);
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