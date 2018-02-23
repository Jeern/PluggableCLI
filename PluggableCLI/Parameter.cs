using System;

namespace PluggableCLI
{
    public class Parameter
    {
        public string ShortName { get; }
        public string LongName { get; }
        public string MemberName { get; }
        public Type ParameterType { get; }
        public string HelpText { get; }

        public Parameter(string shortName, string longName, Type parameterType, string helpText)
        {
            MemberName = longName.Trim(' ', '-').ToLowerInvariant();
            ShortName = $"-{shortName.Trim(' ', '-')}".ToLowerInvariant();
            LongName = $"-{MemberName}";
            if(parameterType != typeof(bool) && parameterType != typeof(string) && parameterType != typeof(int))
                throw new ArgumentException("parameterType must be int, bool or string");
            ParameterType = parameterType;
            HelpText = helpText;
        }
    }
}
