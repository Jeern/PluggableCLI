using System;
using System.Collections.Generic;
using System.Dynamic;

namespace PluggableCLI
{
    public class CLIValue : DynamicObject
    {
        private readonly Dictionary<string, object> _content = new Dictionary<string, object>();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string name = binder.Name;
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name), "cannot be null");
            name = name.ToLowerInvariant();
            if (_content.ContainsKey(name))
            {
                result = _content[name];
                return true;
            }
            result = null;
            return false;
        }

        internal void SetMember(string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name), "cannot be null");
            name = name.ToLowerInvariant();
            if (_content.ContainsKey(name))
            {
                _content[name] = value;
            }
            else
            {
                _content.Add(name.ToLowerInvariant(), value);
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            throw new CLIInfoException($"Sorry you cannot change Parameters, AppSettings or ConnectionStrings programmatically. You tried with {binder.Name}.");
        }
    }
}
