namespace ABB.Ability.IotEdge.CST.Modules.CSharp.C2DMessages
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            var module = new RemoteCommandsModule();
            module.StartModuleAsync(args).Wait();
        }
    }
}
