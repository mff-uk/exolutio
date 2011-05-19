using System;

namespace EvoX.Controller.Commands
{
	/// <summary>
	/// Denotes a property of a command as a result of the command's <see cref="CommandBase.Execute"/> method. 
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class CommandResultAttribute:Attribute
	{
		
	}
}