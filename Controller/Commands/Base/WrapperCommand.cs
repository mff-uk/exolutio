namespace Exolutio.Controller.Commands
{
    public abstract class WrapperCommand: MacroCommand
    {
        protected WrapperCommand()
        {
        }

        protected WrapperCommand(Controller controller)
            : base(controller)
        {
        }
    }
}