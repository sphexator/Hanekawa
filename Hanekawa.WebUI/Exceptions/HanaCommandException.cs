using System;

namespace Hanekawa.WebUI.Exceptions
{
    public class HanaCommandException : Exception
    {
        public string CommandErrorMessage { get; set; }
        public HanaCommandException(string message) => CommandErrorMessage = message;

        public override string ToString() => $"Hana Command Exception: {Message}";
    }
}