/**************************************************************************************************** 

Author: Dave Glover
Date: Aug 2020

References:

Azure IoT C# SDK Samples    https://github.com/Azure-Samples/azure-iot-samples-csharp
Protobuf Serialiser         https://developers.google.com/protocol-buffers/docs/csharptutorial
System Drawing Libraries    https://github.com/dotnet/runtime/issues/27200
Google Protobuf types       https://developers.google.com/protocol-buffers/docs/proto3#scalar


Generate protobuf class:

https://docs.microsoft.com/en-us/azure/iot-accelerators/iot-accelerators-device-simulation-protobuf#generate-the-protobuf-class

protoc -I . --csharp_out=. TelemetryProtobuf.proto

Libraries:

This sample was tested on Windows, macOS, Ubuntu 20.04 on x64, and Ubuntu 20.04 on Raspberry Pi

On Linux, the following library must be installed for System.Drawing

sudo apt install libgdiplus

****************************************************************************************************/

using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using DotNet.Core.IotHub.Models.Protobuf;
using Google.Protobuf;
using Iot.Device.CpuTemperature;
using Newtonsoft.Json;


namespace DotNet.Core.IotHub.Protobuf
{
    class Program
    {
        private static string idScope = Environment.GetEnvironmentVariable("DPS_IDSCOPE");
        private static string registrationId = Environment.GetEnvironmentVariable("DPS_REGISTRATION_ID");
        private static string primaryKey = Environment.GetEnvironmentVariable("DPS_PRIMARY_KEY");
        private static string secondaryKey = Environment.GetEnvironmentVariable("DPS_SECONDARY_KEY");
        private const string GlobalDeviceEndpoint = "global.azure-devices-provisioning.net";
        static CpuTemperature _temperature = new CpuTemperature();
        static int msgId = 0;
        static string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        static string filename = appDirectory + "images/orange.jpg";

        static async Task Main(string[] args)
        {
            double temperature;
            var rand = new Random();

            using (var security = new SecurityProviderSymmetricKey(registrationId, primaryKey, secondaryKey))
            using (var transport = new ProvisioningTransportHandlerMqtt())
            {
                ProvisioningDeviceClient provClient = ProvisioningDeviceClient.Create(GlobalDeviceEndpoint, idScope, security, transport);
                DeviceRegistrationResult result = await provClient.RegisterAsync();
                IAuthenticationMethod auth = new DeviceAuthenticationWithRegistrySymmetricKey(result.DeviceId, (security as SecurityProviderSymmetricKey).GetPrimaryKey());

                using (DeviceClient iotClient = DeviceClient.Create(result.AssignedHub, auth, TransportType.Mqtt))
                {
                    while (true)
                    {
                        if (_temperature.IsAvailable) { temperature = Math.Round(_temperature.Temperature.Celsius, 2); }
                        else { temperature = rand.Next(15, 35); }

                        try
                        {
                            msgId++;

                            Console.WriteLine($"The CPU temperature is {temperature}");

                            Image img = Image.FromFile(filename);   // simulate camera captured image

                            using (MemoryStream ms = new MemoryStream())
                            {
                                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                                TelemetryProtobuf tpb = new TelemetryProtobuf()
                                {
                                    Temperature = temperature,
                                    Humidity = 50,
                                    Pressure = 1100,
                                    MsgId = msgId,
                                    Label = "Orange",
                                    Probability = 1.0,
                                    Image = ByteString.CopyFrom(ms.ToArray()),
                                    DeviceId = string.Empty     // this is set in the azure function when writing to cosmosdb
                                };

                                using (MemoryStream memStream = new MemoryStream())
                                {
                                    tpb.WriteTo(memStream);
                                    await SendMsgIotHub(iotClient, memStream.ToArray());

                                    // Deserialise Protobuf
                                    // var newTpb = TelemetryProtobuf.Parser.ParseFrom(memStream.ToArray());
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("exception msg: " + ex.Message);
                        }
                        Thread.Sleep(4000); // sleep for 10 seconds
                    }
                }
            }
        }

        private static async Task SendMsgIotHub(DeviceClient iotClient, double temperature)
        {
            var telemetry = new Telemetry() { Temperature = temperature, MessageId = msgId };
            string json = JsonConvert.SerializeObject(telemetry);

            Message eventMessage = new Message(Encoding.UTF8.GetBytes(json));
            eventMessage.Properties.Add("appid", "hvac");
            eventMessage.Properties.Add("type", "telemetry");
            eventMessage.Properties.Add("format", "json");
            eventMessage.Properties.Add("msgid", msgId.ToString());

            await iotClient.SendEventAsync(eventMessage).ConfigureAwait(false);
        }

        private static async Task SendMsgIotHub(DeviceClient iotClient, byte[] telemetry)
        {
            Message eventMessage = new Message(telemetry);
            eventMessage.Properties.Add("appid", "qa-score");
            eventMessage.Properties.Add("type", "telemetry");
            eventMessage.Properties.Add("format", "protobuf");
            eventMessage.Properties.Add("msgid", msgId.ToString());

            await iotClient.SendEventAsync(eventMessage).ConfigureAwait(false);
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