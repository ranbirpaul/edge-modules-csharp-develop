using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

  //"objectId": "55555cb3-435d-4ef6-8779-9dd34f01a2b6",
  //"model": "abb.ability.device",
  //"fromFile": "backup_2017_08_08.zip",
  //"toFile": "backups/2017/08/08/backup.zip",
  //"removeOnComplete": true,
  //"correlationId": "123"

namespace ABB.Ability.IotEdge.CST.Modules.CSharp.Common.Messages
{
    public class SendFileMessage
    { 
        [JsonProperty("objectId")]
        public string ObjectId { get; private set; }

        [JsonProperty("model")]
        public string Model { get; private set; }

        [JsonProperty("fromFile")]
        public string FromFile { get; private set; }

        [JsonProperty("toFile")]
        public string ToFile { get; private set; }

        [JsonProperty("removeOnComplete")]
        public bool RemoveOnComplete { get; private set; }

        [JsonProperty("correlationId")] 
        public string CorrelationId { get; private set; }

        public SendFileMessage(string objectId, string model, string fromFile, string toFile, bool removeOnComplete, string correlationId)
        {
            ObjectId = objectId;
            Model = model;
            FromFile = fromFile;
            ToFile = toFile;
            RemoveOnComplete = removeOnComplete;
            CorrelationId = correlationId;
        }
    }
}
