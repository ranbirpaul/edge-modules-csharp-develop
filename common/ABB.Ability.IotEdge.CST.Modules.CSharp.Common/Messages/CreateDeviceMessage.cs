using Newtonsoft.Json;
using System.Collections.Generic;

namespace ABB.Ability.IotEdge.CST.Modules.CSharp.Common.Messages
{
    public class CreateDeviceMessage
    {
        public CreateDeviceMessage(string deviceType, string serialNumber, string parentObjectId, string parentReference)
        {
            Type = deviceType;
            Properties = new Dictionary<string, PropertyValue<string>> { { "serialNumber", new PropertyValue<string>(serialNumber) } };
            Parent = new ParentReference(parentObjectId, parentReference);
        }

        [JsonProperty("type")]
        public string Type { get; private set; }

        [JsonProperty("parent")]
        public ParentReference Parent { get; private set; }

        [JsonProperty("properties")]
        public Dictionary<string, PropertyValue<string>> Properties { get; private set; }

        public class ParentReference
        {
            public ParentReference(string objectId, string reference)
            {
                ObjectId = objectId;
                Reference = reference;
            }

            [JsonProperty("objectId")]
            public string ObjectId { get; private set; }

            [JsonProperty("reference")]
            public string Reference { get; private set; }

        }

        public class PropertyValue<T>
        {
            public PropertyValue(T value)
            {
                Value = value;
            }

            [JsonProperty("value")]
            public T Value { get; private set; }
        }
    }
}
