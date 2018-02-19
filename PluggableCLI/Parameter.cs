using System;

namespace PluggableCLI
{
    public class Parameter
    {
        public string ShortName { get; }
        public string LongName { get; }
        public Type ParameterType { get; }
        public string HelpText { get; }

        public Parameter(string shortName, string longName, Type parameterType, string helpText)
        {
            ShortName = shortName;
            LongName = longName;
            ParameterType = parameterType;
            HelpText = helpText;
        }
    }
}
