using System;
using System.Reflection;

namespace PluggableCLI
{
    public class CLIProviders
    {
        public static void Run(string[] args, Assembly executingAssembly = null)
        {
            var assemblyName = (executingAssembly ?? Assembly.GetExecutingAssembly()).GetName().Name;

            var loader = new ProviderLoader();
            var providers = loader.LoadAllProviders();

            Console.WriteLine(assemblyName);
        }
    }
}
