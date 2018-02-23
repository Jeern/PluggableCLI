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
            //Step 1 - Read all AppSettings fail if any one missing + LockForUpdates
            //Step 2 - Read all ConnectionString fail if any one missing + LockForUpdates
            //Step 3 - Evalute Show Help
            //Step 4 - Setup all Parameters (fail if any parameter not matching) + LockForUpdates
            //Step 5 - Call execute

            Parameters.Test = "Egon";
            Parameters.LockForUpdates();

            //Diverse pre og evt. post checks
            Execute();
        }

        protected abstract void Execute();
    }
}
