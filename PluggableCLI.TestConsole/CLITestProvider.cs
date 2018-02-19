﻿using System;
using System.Collections.Generic;

namespace PluggableCLI.TestConsole
{
    public class CLITestProvider : CLIProvider
    {
        public override string Name => "Test";

        public override List<Parameter> Parameters => new List<Parameter>
        {
            new Parameter("p", "Print", typeof(string), "Prints")
        };

        public override List<AppSetting> AppSettings => new List<AppSetting>
        {
            new AppSetting("Production", typeof(bool), "Is this production")
        };

        public override List<ConnectionString> ConnectionStrings => new List<ConnectionString>
        {
            new ConnectionString("TheDatabase", "Connectionstring to the database")
        };

        protected override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}