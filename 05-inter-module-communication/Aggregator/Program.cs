namespace ABB.Ability.IotEdge.CST.Modules.CSharp.InterModuleCommunication.Aggregator
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            var module = new AggregatorModule();
            module.StartModuleAsync(args).Wait();
        }
    }
}
