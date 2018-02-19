using System;
using System.Runtime.Serialization;

namespace PluggableCLI
{
    [Serializable]
    public class CLIInfoException : Exception
    {
        public CLIInfoException() { }
        public CLIInfoException(string message) : base(message) { }
        public CLIInfoException(string message, Exception inner) : base(message, inner) { }
        protected CLIInfoException(SerializationInfo info,StreamingContext context) : base(info, context) { }
    }
}
