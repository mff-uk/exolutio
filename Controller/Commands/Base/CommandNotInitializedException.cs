using System;

namespace EvoX.Controller.Commands
{
	/// <summary>
	/// Thrown when some field of a command is not properly initialized
	/// </summary>
	public class CommandNotInitializedException: EvoXCommandException
	{
	    public CommandNotInitializedException(CommandBase command, string argument) : base(command)
	    {
	        Argument = argument;
	    }

	    public CommandNotInitializedException(string message, string argument) : base(message)
	    {
	        Argument = argument;
	    }

	    public CommandNotInitializedException(string message, CommandBase command, string argument) : base(message, command)
	    {
	        Argument = argument;
	    }

	    public CommandNotInitializedException(string message, Exception innerException, string argument) : base(message, innerException)
	    {
	        Argument = argument;
	    }

	    public CommandNotInitializedException(string message, Exception innerException, CommandBase command, string argument) : base(message, innerException, command)
	    {
	        Argument = argument;
	    }

	    public string Argument { get; private set; }

	    public CommandNotInitializedException(string argument)
	    {
	        Argument = argument;
	    }
	}
}