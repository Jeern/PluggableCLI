namespace PluggableCLI
{
    public class ConnectionString
    {
        public string Name { get; }
        public string HelpText { get; }

        public ConnectionString(string name, string helpText)
        {
            Name = name;
            HelpText = helpText;
        }
    }
}
