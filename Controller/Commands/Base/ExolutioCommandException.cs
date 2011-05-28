using System;
using Exolutio.SupportingClasses;

namespace Exolutio.Controller.Commands
{
    public class ExolutioCommandException: ExolutioException 
    {
        public CommandBase Command { get; set; }

        public ExolutioCommandException()
        {
        }

        public ExolutioCommandException(CommandBase command)
        {
            Command = command;
        }

        public ExolutioCommandException(string message) : base(message)
        {
        }

        public ExolutioCommandException(string message, CommandBase command) : base(message)
        {
            Command = command;
        }

        public ExolutioCommandException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ExolutioCommandException(string message, Exception innerException, CommandBase command) : base(message, innerException)
        {
            Command = command;
        }
    }
}