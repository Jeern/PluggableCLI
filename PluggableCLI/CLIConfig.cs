using System;
using System.Configuration;

namespace PluggableCLI
{
    public static class CLIConfig
    {
        public static object ReadAppSetting(string name, Func<string, object> typeConvert)
        {
            return typeConvert(ConfigurationManager.AppSettings[name]);
        }
        public static string ReadConnectionString(string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }
    }
}
