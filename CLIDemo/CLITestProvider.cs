using System;
using System.Collections.Generic;
using PluggableCLI;

namespace CLIDemo
{
    public class CLITestProvider : CLIProvider
    {
        public override string Verb => "Test";
        public override bool HasVerbArgument => false;

        public override List<Parameter> SetupParameters => new List<Parameter>
        {
            new Parameter("p", "Print", typeof(string), "Prints"),
            new Parameter("c", "count", typeof(bool), "count")
        };

        public override List<AppSetting> SetupAppSettings => new List<AppSetting>
        {
            new AppSetting("Production", typeof(bool), "Is this production")
        };

        public override List<ConnectionString> SetupConnectionStrings => new List<ConnectionString>
        {
            new ConnectionString("TheDatabase", "Connectionstring to the database")
        };

        protected override void Execute()
        {
            if (!AppSettings.Production)
            {
                Console.WriteLine($"Print: {Parameters.Print}");
                Console.WriteLine(Parameters.Count);
            }
            else
            {
                Console.WriteLine("Print text is secret in production");
            }
        }
    }
}
