using System;

namespace PluggableCLI
{
    public class AppSetting
    {
        public string Name { get; }
        public Func<string, object> TypeConvert { get; }
        public string HelpText { get; }

        public AppSetting(string name, string helpText, Func<string, object> typeConvert = null)
        {
            Name = name;
            TypeConvert = typeConvert ?? (setting => setting);
            HelpText = helpText;
        }
        public AppSetting(string name, Type appSettingType, string helpText)
        {
            Name = name;
            HelpText = helpText;
            if (appSettingType == typeof(string))
            {
                TypeConvert = setting => setting;
            }
            else if (appSettingType == typeof(int))
            {
                TypeConvert = setting => Convert.ToInt32(setting);
            }
            else if (appSettingType == typeof(bool))
            {
                TypeConvert = setting => Convert.ToBoolean(setting);
            }
            else
            {
                throw new ArgumentException($"AppSetting Type {appSettingType.FullName} not supported out of the box. Use the constructor overload that takes a TypeConvert instead");
            }
        }
    }
}
