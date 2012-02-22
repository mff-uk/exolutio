using System;
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

        private Guid propagateSource;

        /// <summary>
        /// Sets the source of propagation to avoid propagating back to the source. (PSM => PIM => PSMs)
        /// </summary>
        public Guid PropagateSource
        {
            get { return propagateSource; }
            set { propagateSource = value; }
        }        
        
        /// <summary>
        /// Creates a macrocommand containing what needs to be done before command execution
        /// </summary>
        /// <returns></returns>
        internal virtual PropagationMacroCommand PrePropagation()
        {
            return null;
        }

        /// <summary>
        /// Creates a macrocommand containing what needs to be done after command execution
        /// </summary>
        /// <returns></returns>
        internal virtual PropagationMacroCommand PostPropagation()
        {
            return null;
        }
    }
}