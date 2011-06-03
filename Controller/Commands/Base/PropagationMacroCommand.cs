namespace Exolutio.Controller.Commands
{
    /// <summary>
    /// This type of MacroCommand should be used to distinguish propagation macrocommands
    /// </summary>
    public class PropagationMacroCommand : MacroCommand
    {
        public PropagationMacroCommand(Controller controller)
    		: base(controller)
    	{	}
    }
}