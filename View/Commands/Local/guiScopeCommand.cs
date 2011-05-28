using System;

namespace Exolutio.View.Commands
{
    
    public abstract class guiScopeCommand: guiCommandBase
    {
        private object scopeObject;
        public object ScopeObject
        {
            get { return scopeObject; }
            set 
            { 
                scopeObject = value; 
                OnCanExecuteChanged(null);
            }
        }
    }
}