using System;
using System.IO;
using Env = System.Environment;

namespace ABB.Ability.IotEdge.CST.Modules.CSharp.Common.Config
{
    /// <summary>
    /// When a module is running as a Docker container in the context of Ability Edge, 
    /// the Edge runtime is responsible for providing the necessary values as environment variables
    /// </summary>
    public class EdgeConfiguration : IConfiguration
    {
        public EdgeConfiguration()
        {
            ClientId = Env.GetEnvironmentVariable("mqtt_client_id");
            ServerUri = new Uri(Env.GetEnvironmentVariable("mqtt_url"));
            Username = Env.GetEnvironmentVariable("module_id");
            Password = File.ReadAllText(
                Env.GetEnvironmentVariable("mqtt_password_file"));
            MethodsInTopic = Env.GetEnvironmentVariable("topics_methods_in");
            MethodsOutTopic = Env.GetEnvironmentVariable("topics_methods_out");
            MessagesOutTopic = Env.GetEnvironmentVariable("topics_messages_out");
            ModelInTopic = Env.GetEnvironmentVariable("topics_model_in");
            CleanSession = true;
            ObjectId = Env.GetEnvironmentVariable("object_id");
            LocalInTopic = Env.GetEnvironmentVariable("topics_local_in");
            LocalOutTopic = Env.GetEnvironmentVariable("topics_local_out");
            FilesOutTopic = Env.GetEnvironmentVariable("topics_files_out");
            FilesInTopic = Env.GetEnvironmentVariable("topics_files_in");
        }

        public string ClientId { get; private set; }
        public Uri ServerUri { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string MethodsInTopic { get; private set; }
        public string MethodsOutTopic { get; private set; }
        public string MessagesOutTopic { get; private set; }
        public string ModelInTopic { get; private set; }
        public bool CleanSession { get; private set; }
        public string ObjectId { get; private set; }
        public string LocalInTopic { get; private set; }
        public string LocalOutTopic { get; private set; }
        public string FilesOutTopic { get; private set; }
        public string FilesInTopic { get; private set; }
    }
}
