using System.ComponentModel.Composition;
using System.Collections.Generic;

namespace PluggableCLI
{
    [InheritedExport]
    public interface ICLIProvider
    {
        string Name { get; }
        List<Parameter> Parameters { get; }
        List<AppSetting> AppSettings { get; }
        List<ConnectionString> ConnectionStrings { get; }

        void Handle();
    }
}
