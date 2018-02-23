using System;

namespace PluggableCLI
{
    public static class Formatting
    {
        public static string Columns(string col1, string col2)
        {
            const int posCol2 = 40;
            const string spaces = "                                                                                          ";
            return $"{col1}{spaces.Substring(0, Math.Max(1, posCol2 - col1.Length))}{col2}";
        }
    }
}
