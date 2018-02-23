using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PluggableCLI
{
    public class CLIProviders
    {
        public static void Run(Assembly executingAssembly = null)
        {
            try
            {
                string argumentString = Environment.CommandLine;

                var executeableName = GetExeName(executingAssembly);

                var loader = new ProviderLoader();
                var providers = loader.LoadAllProviders().ToList();
                ValidateProviders(providers);

                //Format = AssemblyName Verb Argument -extra=... -extra2=...
                var arguments = CleanArguments(argumentString);
                
                //First argument should be removed because it is just the filename of the console app
                arguments.RemoveAt(0);

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
#if !DEBUG
                throw;
#endif
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

        private static List<string> CleanArguments(string arguments)
        {
            //First we join all arguments this is mitigate things like Funky = "Hello" 3 arguments - instead of Funky="Hello" one argument.
            return SplitOnSpaceRespectingPlings(
                RemoveExcessSpaces(arguments)
                    .Trim())
                .ToList();
        }

        private static IEnumerable<string> SplitOnSpaceRespectingPlings(string arguments)
        {
            var sb = new StringBuilder();
            bool inSingleQuotes = false;
            bool inDoubleQuotes = false;
            foreach (var cc in arguments.ToCharArray())
            {
                if (cc == ' ' && !inSingleQuotes && !inDoubleQuotes)
                {
                    string retVal = sb.ToString();
                    sb = new StringBuilder();
                    yield return retVal;
                }
                else if(cc == '\'')
                {
                    inSingleQuotes = !inSingleQuotes;
                }
                else if (cc == '"')
                {
                    inDoubleQuotes = !inDoubleQuotes;
                }
                else
                {
                    sb.Append(cc);
                }
            }

            yield return sb.ToString();
        }

        private static string RemoveExcessSpaces(string arguments)
        {
            return new string(RemoveExcessSpaces(arguments.ToCharArray()).ToArray());
        }

        private static IEnumerable<char> RemoveExcessSpaces(char[] arguments)
        {
            bool inSingleQuotes = false;
            bool inDoubleQuotes = false;
            int prevIdx = -1;
            for (int idx = 0; idx < arguments.Length; idx++)
            {
                //cc = current char
                char cc = arguments[idx];
                //nc = next char
                char nc = idx < arguments.Length - 1
                    ? arguments[idx + 1]
                    : '%'; //% is just a random char other than '-', '=' and ' '  does not matter
                //pc = previous char
                char pc = idx > 0
                    ? arguments[prevIdx]
                    : '%'; //% is just a random char other than '-', '=' and ' '  does not matter
                if (inSingleQuotes && cc == '\'')
                {
                    inSingleQuotes = false;
                    prevIdx = idx;
                    yield return cc;
                }
                else if (inSingleQuotes)
                {
                    prevIdx = idx;
                    yield return cc;
                }
                else if (cc == '\'')
                {
                    inSingleQuotes = true;
                    prevIdx = idx;
                    yield return cc;
                }
                else if (inDoubleQuotes && cc == '"')
                {
                    inDoubleQuotes = false;
                    prevIdx = idx;
                    yield return cc;
                }
                else if (inDoubleQuotes)
                {
                    prevIdx = idx;
                    yield return cc;
                }
                else if (cc == '"')
                {
                    inDoubleQuotes = true;
                    prevIdx = idx;
                    yield return cc;
                }
                else if (cc == ' ' && nc == ' ')
                    continue;
                else if (cc == ' ' && nc == '=')
                    continue;
                else if (pc == '=' && cc == ' ')
                    continue;
                else if (pc == '-' && cc == ' ')
                    continue;
                else
                {
                    prevIdx = idx;
                    yield return cc;
                }
            }
        }
        private static void CheckIfMainHelpTextShouldBeDisplayed(string assemblyName, List<string> arguments, List<ICLIProvider> providers)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Usage:");
            sb.AppendLine(Formatting.Columns($"  {assemblyName} help", "- displays this text"));
            providers.Where(p => p.HasVerbArgument).ToList().ForEach(p => sb.AppendLine(Formatting.Columns($"  {assemblyName} {p.Verb} <argument>", $"- Activates the {p.Verb} provider")));
            providers.Where(p => !p.HasVerbArgument).ToList().ForEach(p => sb.AppendLine(Formatting.Columns($"  {assemblyName} {p.Verb}", $"- Activates the {p.Verb} provider")));

            //Main help should be displayed if the first verb is help or if none of the providers verb is chosen, or if 
            //no arguments belong to a provider.
            if (arguments == null || arguments.Count == 0 || arguments[0] == "help" ||
                providers.All(p => p.Verb.ToLowerInvariant() != arguments[0]))
                throw new CLIInfoException(sb.ToString());
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
