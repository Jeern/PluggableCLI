﻿using System;
using System.Collections.Generic;

namespace PluggableCLI.TestConsole
{
    public class CLITestProvider : CLIProvider
    {
        public override string Verb => "Test";

        public override List<Parameter> SetupParameters => new List<Parameter>
        {
            new Parameter("p", "Print", typeof(string), "Prints")
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
            Console.WriteLine(Parameters.Test);
            Parameters.Test2 = "Fede";
            
        }
    }
}
