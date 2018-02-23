using System;
using System.Collections.Generic;
using PluggableCLI;

namespace CLIDemo
{
    public class CheckoutProvider : CLIProvider
    {
        public override string Verb => "Checkout";
        public override bool HasVerbArgument => true;

        public override List<AppSetting> SetupAppSettings => new List<AppSetting>
        {
            new AppSetting("Production", typeof(bool), "Is this production")
        };

        public override List<ConnectionString> SetupConnectionStrings => new List<ConnectionString>
        {
            new ConnectionString("TheDatabase", "Connectionstring to the database")
        };

        public override List<Parameter> SetupParameters => new List<Parameter>
        {
            new Parameter("v", "Verbose", typeof(bool), "Makes the output verbose"),
            new Parameter("tp", "Testprint", typeof(string), "Prints what ever value is provided to testprint")
        };

        protected override void Execute()
        {
            Console.WriteLine(VerbArgument);
            Console.WriteLine(Parameters.Verbose);
            Console.WriteLine(Parameters.TestPrint);
            Console.WriteLine(AppSettings.Production);
            Console.WriteLine(ConnectionStrings.TheDatabase);
        }
    }
}
