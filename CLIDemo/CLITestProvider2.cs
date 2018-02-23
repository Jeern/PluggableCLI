using System;
using System.Collections.Generic;
using PluggableCLI;

namespace CLIDemo
{
    public class CLITestProvider2 : CLIProvider
    {
        public override string Verb => "Test2";
        public override bool HasVerbArgument => true;

        public override List<Parameter> SetupParameters => new List<Parameter>
        {
            new Parameter("p", "Print", typeof(string), "Prints")
        };

        public override List<AppSetting> SetupAppSettings => new List<AppSetting>
        {
            new AppSetting("Production", typeof(bool), "Is this production")
        };

        protected override void Execute()
        {
            Console.WriteLine(VerbArgument);
        }
    }
}
