using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PluggableCLI
{
    public abstract class CLIProvider : ICLIProvider
    {
        public abstract string Verb { get; }
        public abstract bool HasVerbArgument { get; }
        public string VerbArgument { get; internal set; }
        public virtual string ProviderHelpText { get; set; }
        public abstract List<Parameter> SetupParameters { get; }
        public abstract List<AppSetting> SetupAppSettings { get; }
        public abstract List<ConnectionString> SetupConnectionStrings { get; }

        protected readonly dynamic Parameters = new CLIValue();
        protected readonly dynamic AppSettings = new CLIValue();
        protected readonly dynamic ConnectionStrings = new CLIValue();

        public void Handle(string executableName, List<string> arguments)
        {
            //Step 1 - Read all AppSettings fail if any one missing 
            ReadAllAppSettings();

            //Step 2 - Read all ConnectionString fail if any one missing
            ReadAllConnectionStrings();

            //Step 3 - Evalute Show Help
            CheckIfHelpTextShouldBeDisplayed(executableName, arguments);

            //Step 4 - Setup all Parameters 
            ReadAllParameters(arguments);

            //Step 5 - Call execute
            Execute();
        }

        private void CheckIfHelpTextShouldBeDisplayed(string executableName, List<string> arguments)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Usage:");
            sb.AppendLine(Formatting.Columns($"  {executableName} {Verb} help", "- displays this text"));
            string providerHelpText = ProviderHelpText ?? $"- Activates the {Verb} provider";
            if (HasVerbArgument)
            {
                sb.AppendLine(Formatting.Columns($"  {executableName} {Verb} <verb argument>", providerHelpText));
                sb.AppendLine($"    the special <verb argument> cannot start with -");
            }
            else
            {
                sb.AppendLine(Formatting.Columns($"  {executableName} {Verb}", providerHelpText));
            }

            if (SetupParameters != null && SetupParameters.Count > 0)
            {
                sb.AppendLine("");
                sb.AppendLine("Parameters:");
                foreach (var parameter in SetupParameters)
                {
                    sb.AppendLine(Formatting.Columns($"    {parameter.ShortName} {parameter.LongName}",
                        parameter.HelpText));
                    if (parameter.ParameterType == typeof(bool))
                    {
                        sb.AppendLine($"      Usage: {parameter.LongName}");
                    }
                    else if(parameter.ParameterType == typeof(int))
                    {
                        sb.AppendLine($"      Usage: {parameter.LongName}=<number>");
                    }
                    else if (parameter.ParameterType == typeof(string))
                    {
                        sb.AppendLine($"      Usage: {parameter.LongName}=<somevalue>");
                    }
                }
            }

            string helpText = sb.ToString();

            //Main help should be displayed 
            //1. if the second argument is help or 
            //2. if HasVerbArgument is true and the 2. argument is either not there or starts with -
            //3. If one of the rest of the arguments is not in the allowed arguments of the provider (they all have to start with -)

            //1
            if(arguments.Count > 1 && arguments[1] == "help")
                throw new CLIInfoException(helpText);

            //2
            if(HasVerbArgument && arguments.Count <= 1)
                throw new CLIInfoException(helpText);

            //2
            if (HasVerbArgument && arguments.Count > 1 && arguments[1].StartsWith("-"))
                throw new CLIInfoException(helpText);

            //3
            int firstArgumentIdx = HasVerbArgument ? 2 : 1;
            if (arguments.Count > firstArgumentIdx)
            {
                for (int idx = firstArgumentIdx; idx < arguments.Count; idx++)
                {
                    if(SetupParameters == null || 
                       (SetupParameters.All(a => a.ShortName != ArgumentNameTrueToType(a, arguments[idx], helpText)) && 
                        SetupParameters.All(a => a.LongName != ArgumentNameTrueToType(a, arguments[idx], helpText))))
                        throw new CLIInfoException(helpText);
                }
            }
        }

        private string ArgumentNameTrueToType(Parameter parameter, string argument, string helpText = null)
        {
            if (parameter.ParameterType == typeof(bool))
                return argument;

            int equalPos = argument.IndexOf("=", StringComparison.InvariantCulture);
            if (equalPos < 0)
            {
                if(string.IsNullOrWhiteSpace(helpText))
                    throw new ArgumentException("Something wrong with parameters in ArgumentNameTrueToType");
                throw new CLIInfoException(helpText);
            }
            return argument.Substring(0, equalPos);
        }

        private object ArgumentValue(Parameter parameter, string argument)
        {
            if (parameter.ParameterType == typeof(bool))
                return ArgumentValueBool(argument);
            if (parameter.ParameterType == typeof(int))
                return ArgumentValueInt(argument);
            if (parameter.ParameterType == typeof(string))
                return ArgumentValueString(argument);
            throw new ArgumentException("Something wrong with parameters in ArgumentValue");
        }

        private string ArgumentValueString(string argument)
        {
            int equalPos = argument.IndexOf("=", StringComparison.InvariantCulture);
            if (equalPos < 0)
                throw new ArgumentException("Something wrong with parameters in ArgumentValueString");
            return argument.Substring(equalPos+1);
        }

        private int ArgumentValueInt(string argument)
        {
            int equalPos = argument.IndexOf("=", StringComparison.InvariantCulture);
            if (equalPos < 0)
                throw new ArgumentException("Something wrong with parameters in ArgumentNameValueInt");
            return Convert.ToInt32(argument.Substring(equalPos + 1));
        }

        private bool ArgumentValueBool(string argument)
        {
            return true;
        }

        private void ReadAllAppSettings()
        {
            if(SetupAppSettings == null)
                return;

            foreach (var setupAppSetting in SetupAppSettings) 
            {
                string key = $"{Verb}:{setupAppSetting.Name}";
                try
                {
                    var value = CLIConfig.ReadAppSetting(key, setupAppSetting.TypeConvert);
                    if(value == null)
                        throw new ArgumentException("value cannot be null");
                    AppSettings.SetMember(setupAppSetting.Name, value);
                }
                catch
                {
                    throw new CLIInfoException($"The provider {Verb} must contain an AppSetting called {key}");
                }

            }
        }

        private void ReadAllConnectionStrings()
        {
            if (SetupConnectionStrings == null)
                return;

            foreach (var setupConnectionString in SetupConnectionStrings)
            {
                string key = $"{Verb}:{setupConnectionString.Name}";
                try
                {
                    var value = CLIConfig.ReadConnectionString(key);
                    if (value == null)
                        throw new ArgumentException("value cannot be null");
                    AppSettings.SetMember(setupConnectionString.Name, value);
                }
                catch
                {
                    throw new CLIInfoException($"The provider {Verb} must contain a ConnectionString called {key}");
                }
            }
        }

        private void ReadAllParameters(List<string> arguments)
        {
            if(SetupParameters == null)
                return;

            if (HasVerbArgument)
            {
                VerbArgument = arguments[1];
            }

            //First set all boolean values to false
            foreach (var setupParameter in SetupParameters)
            {
                if (setupParameter.ParameterType == typeof(bool))
                {
                    Parameters.SetMember(setupParameter.MemberName, false);
                }
            }

            int firstArgumentIdx = HasVerbArgument ? 2 : 1;
            if (arguments.Count > firstArgumentIdx)
            {
                for (int idx = firstArgumentIdx; idx < arguments.Count; idx++)
                {
                    var setupParameter =
                        SetupParameters.FirstOrDefault(a => a.ShortName == ArgumentNameTrueToType(a, arguments[idx])) ??
                        SetupParameters.FirstOrDefault(a => a.LongName == ArgumentNameTrueToType(a, arguments[idx]));

                    Parameters.SetMember(setupParameter.MemberName, ArgumentValue(setupParameter, arguments[idx]));
                }
            }
        }


        protected abstract void Execute();
    }
}
