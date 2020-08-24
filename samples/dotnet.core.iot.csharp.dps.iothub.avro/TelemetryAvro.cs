using System.Runtime.Serialization;

namespace DotNet.Core.IotHub.Avro
{
    [DataContract(Name = "Telemetry", Namespace = "Glovebox.Hvac")]
    public class TelemetryAvro
    {
        [DataMember(Name = "temperature")]
        public double Temperature { get; set; }

        [DataMember(Name = "humidity")]
        public double Humidity { get; set; }

        [DataMember(Name = "pressure")]
        public double Pressure { get; set; }

        [DataMember(Name = "msgId")]
        public double MsgId { get; set; }

        [DataMember(Name = "image")]
        public byte[] Image { get; set; }
    }
}