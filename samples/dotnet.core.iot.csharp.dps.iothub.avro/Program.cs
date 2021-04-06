/**************************************************************************************************** 

Author: Dave Glover
Date: Aug 2020

References:

Azure IoT C# SDK Samples    https://github.com/Azure-Samples/azure-iot-samples-csharp
AVRO Serialiser             https://github.com/AdrianStrugala/AvroConvert/blob/master/docs/Documentation.md
System Drawing Libraries    https://github.com/dotnet/runtime/issues/27200

// gets AVRO Schema
static string avroTelemetrySchema = AvroConvert.GenerateSchema(typeof(TelemetryAvro));


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
using Iot.Device.CpuTemperature;
using Newtonsoft.Json;
using SolTechnology.Avro;


namespace DotNet.Core.IotHub.Avro
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

        // https://github.com/AdrianStrugala/AvroConvert/blob/master/docs/Documentation.md
        static string avroTelemetrySchema = AvroConvert.GenerateSchema(typeof(TelemetryAvro));
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
                        if (_temperature.IsAvailable) { temperature = Math.Round(_temperature.Temperature.DegreesCelsius, 2); }
                        else { temperature = rand.Next(15, 35); }
                        
                        try
                        {
                            msgId++;

                            Console.WriteLine($"The CPU temperature is {temperature}");

                            Image img = Image.FromFile(filename);   // simulate camera captured image

                            using (MemoryStream ms = new MemoryStream())
                            {
                                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                                TelemetryAvro telemetryAvro = new TelemetryAvro()
                                {
                                    Temperature = temperature,
                                    Humidity = 50,
                                    Pressure = 1100,
                                    MsgId = msgId,
                                    Label = "Orange",
                                    Probability = 1.0,
                                    Image = ms.ToArray(),
                                    DeviceId = string.Empty     // this is set in the azure function when writing to cosmosdb
                                };

                                var serialized = AvroConvert.SerializeHeadless(telemetryAvro, avroTelemetrySchema);

                                // Deserialise AVRO
                                // var deserialized = AvroConvert.DeserializeHeadless<TelemetryAvro>(serialized, avroTelemetrySchema);
                                // using (MemoryStream mStream = new MemoryStream(deserialized.Image))
                                // {
                                //     Image.FromStream(mStream).Save(appDirectory + "images/orangev2.jpg");
                                // }

                                await SendMsgIotHub(iotClient, serialized);

                                await SendMsgIotHub(iotClient, temperature);

                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("exception msg: " + ex.Message);
                        }
                        Thread.Sleep(4000);
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
            eventMessage.Properties.Add("version", "1");
            eventMessage.Properties.Add("format", "json");
            eventMessage.Properties.Add("msgid", msgId.ToString());

            await iotClient.SendEventAsync(eventMessage).ConfigureAwait(false);
        }

        private static async Task SendMsgIotHub(DeviceClient iotClient, byte[] telemetry)
        {
            Message eventMessage = new Message(telemetry);
            eventMessage.Properties.Add("appid", "qa-score");
            eventMessage.Properties.Add("type", "telemetry");
            eventMessage.Properties.Add("version", "1");
            eventMessage.Properties.Add("format", "avro");            
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