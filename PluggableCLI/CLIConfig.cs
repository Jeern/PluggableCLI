using System;
using System.Configuration;

namespace PluggableCLI
{
    public static class CLIConfig
    {
        public static object ReadAppSetting(string name, Func<string, object> typeConvert)
        {
            var value = ConfigurationManager.AppSettings[name];
            if(value == null)
                throw new ArgumentNullException("value cannot be null");
            return typeConvert(value);
        }
        public static string ReadConnectionString(string name)
        {
            return ConfigurationManager.ConnectionStrings[name]?.ConnectionString;
        }
    }
}
