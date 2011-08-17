namespace Exolutio.Controller.Commands
{
    internal abstract class AtomicCommand: StackedCommand
    {
        protected AtomicCommand()
        {
        }

        protected AtomicCommand(Controller controller)
            : base(controller)
        {
            
        }
    }
}