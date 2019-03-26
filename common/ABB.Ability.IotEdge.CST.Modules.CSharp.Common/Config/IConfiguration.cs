using System;

namespace ABB.Ability.IotEdge.CST.Modules.CSharp.Common.Config
{
    public interface IConfiguration
    {
        string ClientId { get; }
        Uri ServerUri { get; }
        string Username { get; }
        string Password { get; }
        bool CleanSession { get; }
        string MethodsInTopic { get; }
        string MethodsOutTopic { get; }
        string MessagesOutTopic { get; }
        string ModelInTopic { get; }
        string LocalInTopic { get; }
        string LocalOutTopic { get; }
        string FilesOutTopic { get; }
        string FilesInTopic { get; }
        string ObjectId { get; }
    }
}
