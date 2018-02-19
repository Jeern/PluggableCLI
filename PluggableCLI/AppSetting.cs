using System;

namespace PluggableCLI
{
    public class AppSetting
    {
        public string Name { get; }
        public Type AppSettingType { get; }
        public string HelpText { get; }

        public AppSetting(string name, Type appSettingType, string helpText)
        {
            Name = name;
            AppSettingType = appSettingType;
            HelpText = helpText;
        }
    }
}
