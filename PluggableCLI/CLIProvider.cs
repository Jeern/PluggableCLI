using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;

namespace PluggableCLI
{
    public abstract class CLIProvider : ICLIProvider
    {
        public abstract string Name { get; }
        public abstract List<Parameter> Parameters { get; }
        public abstract List<AppSetting> AppSettings { get; }
        public abstract List<ConnectionString> ConnectionStrings { get; }
        public void Handle()
        {
            //Diverse pre og evt. post checks
            Execute();
        }

        protected abstract void Execute();
    }
}
