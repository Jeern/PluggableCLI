using System.ComponentModel.Composition;
using System.Collections.Generic;

namespace PluggableCLI
{
    [InheritedExport]
    public interface ICLIProvider
    {
        string Verb { get; }
        bool HasVerbArgument { get; }
        string VerbArgument { get; }
        List<Parameter> SetupParameters { get; }
        List<AppSetting> SetupAppSettings { get; }
        List<ConnectionString> SetupConnectionStrings { get; }

        void Handle();
    }
}
