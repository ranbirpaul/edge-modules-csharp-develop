using System;

namespace ABB.Ability.IotEdge.CST.Modules.CSharp.Common.Config
{
    /// <summary>
    /// To be used if running the module as a standalone console application or if unit tests are created around module logic
    /// In these cases, configuration values will need to be hard-coded below
    /// </summary>
    public class ManualConfiguration : IConfiguration
    {
        public ManualConfiguration()
        {
            ClientId = Guid.NewGuid().ToString();
            ServerUri = new Uri("mqtt://localhost:1883");
            Username = string.Empty;
            Password = string.Empty;
            CleanSession = true;
            MethodsInTopic = string.Empty;
            MethodsOutTopic = string.Empty;
            MessagesOutTopic = string.Empty;
            ModelInTopic = string.Empty;
            ObjectId = Guid.NewGuid().ToString();
            LocalOutTopic = string.Empty;
            LocalInTopic = string.Empty;
            FilesOutTopic = string.Empty;
            FilesInTopic = string.Empty;
        }

        public string ClientId { get; private set; }
        public Uri ServerUri { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public bool CleanSession { get; private set; }
        public string MethodsInTopic { get; private set; }
        public string MethodsOutTopic { get; private set; }
        public string MessagesOutTopic { get; private set; }
        public string ModelInTopic { get; private set; }
        public string ObjectId { get; private set; }
        public string LocalOutTopic { get; private set; }
        public string LocalInTopic { get; private set; }
        public string FilesOutTopic { get; private set; }
        public string FilesInTopic { get; private set; }
    }
}
