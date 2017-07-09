using System;

namespace ManyConsole.CommandLineUtils
{
    public class ConsoleHelpAsException : Exception
    {
        public ConsoleHelpAsException()
        {
        }

        public ConsoleHelpAsException(string message) : base(message)
        {
        }

        public ConsoleHelpAsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}