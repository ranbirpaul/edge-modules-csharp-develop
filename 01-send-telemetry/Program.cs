namespace ABB.Ability.IotEdge.CST.Modules.CSharp.SendTelemetry
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            var module = new SendTelemetryModule();
            module.StartModuleAsync(args).Wait();
        }
    }
}
