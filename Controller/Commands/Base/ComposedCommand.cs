namespace Exolutio.Controller.Commands
{
    public abstract class ComposedCommand: MacroCommand
    {
        protected ComposedCommand()
        {
        }

        protected ComposedCommand(Controller controller) : base(controller)
        {
        }
    }
}