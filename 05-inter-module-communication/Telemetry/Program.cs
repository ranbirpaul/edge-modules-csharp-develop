namespace ABB.Ability.IotEdge.CST.Modules.CSharp.InterModuleCommunication.Telemetry
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            var module = new TelemetryModule();
            module.StartModuleAsync(args).Wait();
        }
    }
}