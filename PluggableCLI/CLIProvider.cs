using System.Collections.Generic;

namespace PluggableCLI
{
    public abstract class CLIProvider : ICLIProvider
    {
        public abstract string Verb { get; }
        public abstract bool HasVerbArgument { get; }
        public string VerbArgument { get; internal set; }
        public abstract List<Parameter> SetupParameters { get; }
        public abstract List<AppSetting> SetupAppSettings { get; }
        public abstract List<ConnectionString> SetupConnectionStrings { get; }

        protected readonly dynamic Parameters = new CLIValue();
        protected readonly dynamic AppSettings = new CLIValue();
        protected readonly dynamic ConnectionStrings = new CLIValue();

        public void Handle(string executableName, List<string> arguments)
        {


            Parameters.Test = "Egon";
            Parameters.LockForUpdates();

            //Diverse pre og evt. post checks
            Execute();
        }

        protected abstract void Execute();
    }
}
