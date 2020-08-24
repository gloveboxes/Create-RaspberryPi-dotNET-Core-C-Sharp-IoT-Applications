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
        public int MsgId { get; set; }

        [DataMember(Name = "label")]
        public string Label { get; set; }

        [DataMember(Name = "probability")]
        public double Probability { get; set; }

        [DataMember(Name = "image")]
        public byte[] Image { get; set; }
    }
}