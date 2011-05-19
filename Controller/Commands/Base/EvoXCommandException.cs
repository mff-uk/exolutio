using System;
using EvoX.SupportingClasses;

namespace EvoX.Controller.Commands
{
    public class EvoXCommandException: EvoXException 
    {
        public CommandBase Command { get; set; }

        public EvoXCommandException()
        {
        }

        public EvoXCommandException(CommandBase command)
        {
            Command = command;
        }

        public EvoXCommandException(string message) : base(message)
        {
        }

        public EvoXCommandException(string message, CommandBase command) : base(message)
        {
            Command = command;
        }

        public EvoXCommandException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public EvoXCommandException(string message, Exception innerException, CommandBase command) : base(message, innerException)
        {
            Command = command;
        }
    }
}