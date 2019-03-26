namespace ABB.Ability.IotEdge.CST.Modules.CSharp.ListenForConfigChanges
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            var module = new ConfigurableModule();
            module.StartModuleAsync(args).Wait();
        }
    }
}