using Newtonsoft.Json;
using System;

namespace ABB.Ability.IotEdge.CST.Modules.CSharp.Common.Messages
{
    public class TelemetryMessage<T>
    {
        public TelemetryMessage(Guid deviceId, string variable, Func<T> valueGenerator)
        {
            ObjectId = deviceId.ToString();
            Model = "abb.ability.device";
            Timestamp = DateTime.UtcNow.ToString("o");
            Value = valueGenerator();
            Variable = variable;
            Quality = 1;
        }

        [JsonProperty("objectId")]
        public string ObjectId { get; private set; }

        [JsonProperty("model")]
        public string Model { get; private set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; private set; }

        [JsonProperty("value")]
        public T Value { get; private set; }

        [JsonProperty("variable")]
        public string Variable { get; private set; }

        [JsonProperty("quality")]
        public int Quality { get; private set; }
    }
}
