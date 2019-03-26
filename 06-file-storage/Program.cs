using System;

namespace ABB.Ability.IotEdge.CST.Modules.CSharp.FileStorage
{
    class Program
    {
        static void Main(string[] args)
        {
            var module = new UploadFileModule();
            module.StartModuleAsync(args).Wait();
        }
    }
}
