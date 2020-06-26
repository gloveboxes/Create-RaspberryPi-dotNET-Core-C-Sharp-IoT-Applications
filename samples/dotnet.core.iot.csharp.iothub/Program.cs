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

        // Replace with the device id you used when you created the device in Azure IoT Hub
        const string DeviceId = "<Your Device Id>";
        static DeviceClient _deviceClient = DeviceClient.CreateFromConnectionString(DeviceConnectionString, TransportType.Mqtt);
        static CpuTemperature _temperature = new CpuTemperature();
        static int _msgId = 0;
        const double TemperatureThreshold = 42.0;

        static async Task Main(string[] args)
        {
            while (true)
            {
                if (_temperature.IsAvailable)
                {
                    Console.WriteLine($"The CPU temperature is {Math.Round(_temperature.Temperature.Celsius, 2)}");
                    await SendMsgIotHub(_temperature.Temperature.Celsius);
                }
                Thread.Sleep(2000); // sleep for 2000 milliseconds
            }
        }

        private static async Task SendMsgIotHub(double temperature)
        {
            var telemetry = new Telemetry() { Temperature = Math.Round(temperature, 2), MessageId = _msgId++ };
            string json = JsonConvert.SerializeObject(telemetry);

            Message eventMessage = new Message(Encoding.UTF8.GetBytes(json));
            eventMessage.Properties.Add("temperatureAlert", (temperature > TemperatureThreshold) ? "true" : "false");
            await _deviceClient.SendEventAsync(eventMessage).ConfigureAwait(false);
        }

        class Telemetry
        {
            [JsonPropertyAttribute (PropertyName="temperature")] 
            public double Temperature { get; set; } = 0;

            [JsonPropertyAttribute (PropertyName="messageId")] 
            public int MessageId { get; set; } = 0;

            [JsonPropertyAttribute (PropertyName="deviceId")] 
            public string DeviceId {get; set;} = Program.DeviceId;
        }
    }
}