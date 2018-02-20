using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PluggableCLI
{
    public class CLIProviders
    {
        public static void Run(string[] args, Assembly executingAssembly = null)
        {
            try
            {
                var assemblyName = (executingAssembly ?? Assembly.GetExecutingAssembly()).GetName().Name;

                var loader = new ProviderLoader();
                var providers = loader.LoadAllProviders().ToList();
                ValidateProviders(providers);

                foreach (var provider in providers)
                {
                    provider.Handle();
                }

                Console.WriteLine(assemblyName);

            }
            catch (CLIInfoException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static void ValidateProviders(List<ICLIProvider> providers)
        {
            ValidateUniqueNames(providers);
            ValidateUniqueParameterNames(providers);
            ValidateUniqueAppSettingNames(providers);
            ValidateConnectionStringNames(providers);
        }

        private static void ValidateConnectionStringNames(List<ICLIProvider> providers)
        {
            foreach (var provider in providers)
            {
                ValidateConnectionStringNames(provider);
            }
        }

        private static void ValidateConnectionStringNames(ICLIProvider provider)
        {
            if (provider.SetupConnectionStrings == null || provider.SetupConnectionStrings.Count == 0)
                return;

            ValidateUniqueNess(provider.SetupConnectionStrings.Select(p => p.Name.ToLowerInvariant()),
                $"A connectionString in a  provider has to have a unique name. There is at least two called {{0}} in the {provider.Verb} provider");
        }
        private static void ValidateUniqueAppSettingNames(List<ICLIProvider> providers)
        {
            foreach (var provider in providers)
            {
                ValidateUniqueAppSettingNames(provider);
            }
        }

        private static void ValidateUniqueAppSettingNames(ICLIProvider provider)
        {
            if (provider.SetupAppSettings == null || provider.SetupAppSettings.Count == 0)
                return;

            ValidateUniqueNess(provider.SetupAppSettings.Select(p => p.Name.ToLowerInvariant()),
                $"An appSetting in a  provider has to have a unique name. There is at least two called {{0}} in the {provider.Verb} provider");
        }

        private static void ValidateUniqueParameterNames(List<ICLIProvider> providers)
        {
            foreach (var provider in providers)
            {
                ValidateUniqueParameterNames(provider);
            }
        }

        private static void ValidateUniqueParameterNames(ICLIProvider provider)
        {
            if (provider.SetupParameters == null || provider.SetupParameters.Count == 0)
                return;

            ValidateUniqueNess(provider.SetupParameters.Select(p => p.ShortName.ToLowerInvariant()),
                $"A parameter in a  provider has to have a unique name. There is at least two called {{0}} in the {provider.Verb} provider");

            ValidateUniqueNess(provider.SetupParameters.Select(p => p.LongName.ToLowerInvariant()),
                $"A parameter in a  provider has to have a unique name. There is at least two called {{0}} in the {provider.Verb} provider");
        }

        private static void ValidateUniqueNess(IEnumerable<string> names, string errorMessageFormatTemplate)
        {
            //I Could just use a Distinct() and compare the count to the original count to find if there is a name
            //that is not unique. But then I would not know which name it is. Therefore I use this hashSet technique
            var hashSet = new HashSet<string>();
            foreach (var name in names)
            {
                if (hashSet.Contains(name))
                    throw new CLIInfoException(string.Format(errorMessageFormatTemplate, name));
                hashSet.Add(name);
            }
        }

        private static void ValidateUniqueNames(List<ICLIProvider> providers)
        {
            if(providers == null || providers.Count == 0)
                throw new CLIInfoException("No providers found");

            ValidateUniqueNess(providers.Select(p => p.Verb.ToLowerInvariant()),
                "A CLI provider has to have a unique name. There is at least two called {0}");
        }
    }
}
