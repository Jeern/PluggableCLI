using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PluggableCLI
{
    public class CLIProviders
    {
        public static void Run(string[] args, Assembly executingAssembly = null)
        {
            try
            {
                var executeableName = GetExeName(executingAssembly);

                var loader = new ProviderLoader();
                var providers = loader.LoadAllProviders().ToList();
                ValidateProviders(providers);

                //Format = AssemblyName Verb Argument -extra=... -extra2=...
                var arguments = CleanArguments(args);

                CheckIfMainHelpTextShouldBeDisplayed(executeableName, arguments, providers);

                //Find specific provider
                var relevantProvider = GetRelevantProvider(arguments[0], providers);
                relevantProvider.Handle(executeableName, arguments);
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
#if DEBUG
            finally
            {
                Console.ReadLine();
            }
#endif
        }

        private static ICLIProvider GetRelevantProvider(string verb, List<ICLIProvider> providers)
        {
            var relevantProvider = providers.FirstOrDefault(p => p.Verb.ToLowerInvariant() == verb);
            if (relevantProvider == null)
                throw new CLIInfoException($"Something went wrong. {verb} provider not found");
            return relevantProvider;
        }

        private static string GetExeName(Assembly executingAssembly)
        {
            if (executingAssembly != null)
                return Path.GetFileNameWithoutExtension(executingAssembly.GetName().Name);

            return Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
        }

        private static List<string> CleanArguments(string[] args)
        {
            return args.Select(a => a.ToLowerInvariant().Trim()).ToList();
        }

        private static void CheckIfMainHelpTextShouldBeDisplayed(string assemblyName, List<string> arguments, List<ICLIProvider> providers)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Usage:");
            sb.AppendLine(Columns($"  {assemblyName} help", "- displays this text"));
            providers.Where(p => p.HasVerbArgument).ToList().ForEach(p => sb.AppendLine(Columns($"  {assemblyName} {p.Verb} <argument>", $"- Activates the {p.Verb} provider")));
            providers.Where(p => !p.HasVerbArgument).ToList().ForEach(p => sb.AppendLine(Columns($"  {assemblyName} {p.Verb}", $"- Activates the {p.Verb} provider")));

            //Main help should be displayed if the first verb is help or if none of the providers verb is chosen, or if 
            //no arguments belong to a provider.
            if (arguments == null || arguments.Count == 0 || arguments[0] == "help" ||
                providers.All(p => p.Verb.ToLowerInvariant() != arguments[0]))
                throw new CLIInfoException(sb.ToString());
        }

        private static string Columns(string col1, string col2)
        {
            const int posCol2 = 40;
            const string spaces = "                                                                                          ";
            return $"{col1}{spaces.Substring(0, Math.Max(1, posCol2 - col1.Length))}{col2}";
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
