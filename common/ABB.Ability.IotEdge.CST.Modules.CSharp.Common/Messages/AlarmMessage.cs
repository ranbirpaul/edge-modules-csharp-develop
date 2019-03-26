using Newtonsoft.Json;
using System;

namespace ABB.Ability.IotEdge.CST.Modules.CSharp.Common.Messages
{
    public class AlarmMessage<T>
    {
        public class AlarmMessageValue
        {
            public AlarmMessageValue(Func<T> valueGenerator, bool active)
            {
                Value = valueGenerator != null ? valueGenerator() : default(T);
                Active = active;
            }

            [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
            public T Value { private set; get; }

            [JsonProperty("active")]
            public bool Active { private set; get; }
        }

        public AlarmMessage(Guid deviceId, string alarmName, bool active, Func<T> valueGenerator = null)
        {
            ObjectId = deviceId.ToString();
            Model = "abb.ability.device";
            Timestamp = DateTime.UtcNow.ToString("o");
            Value = new AlarmMessageValue(valueGenerator, active);
            Alarm = alarmName;
        }

        [JsonProperty("objectId")]
        public string ObjectId { get; private set; }

        [JsonProperty("model")]
        public string Model { get; private set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; private set; }

        [JsonProperty("value")]
        public AlarmMessageValue Value { get; private set; }

        [JsonProperty("alarm")]
        public string Alarm { get; private set; }

        [JsonProperty("active")]
        public bool Active { get; private set; }
    }
}
