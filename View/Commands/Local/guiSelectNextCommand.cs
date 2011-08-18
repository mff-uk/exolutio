using System.Collections.Generic;
using System.Linq;
using Exolutio.Controller.Commands;
using Exolutio.Model;
using Exolutio.Model.PIM;

namespace Exolutio.View.Commands
{
    public abstract class guiSelectCommandBase : guiScopeCommand
    {
        public override void Execute(object parameter = null)
        {
            Component component = Current.ActiveDiagramView.GetSingleSelectedComponentOrNull();
            if (component != null)
            {
                IList<Component> collection;
                int index;
                GetCollectionAndIndex(component, out collection, out index);

                if (index != -1 && collection != null && collection.Count > 0)
                {
                    index = (IndexOperation(index))%collection.Count;
                    Current.ActiveDiagramView.SetSelection(collection[index], true);
                }
            }
        }

        protected abstract int IndexOperation(int index);

        private void GetCollectionAndIndex(Component component, out IList<Component> collection, out int index)
        {
            index = -1;
            collection = null; 
            if (component is PIMClass)
            {
                collection = ((PIMClass)component).PIMSchema.PIMClasses.Cast<Component>().ToList();
            }
            if (component is PIMAttribute)
            {
                collection = ((PIMAttribute)component).PIMClass.PIMAttributes.Cast<Component>().ToList();
            }
            if (collection != null)
            {
                index = collection.IndexOf(component);
            }
        }

        

        public override bool CanExecute(object parameter = null)
        {
            return true; 
        }
    }

    [Scope(ScopeAttribute.EScope.PIMClass | ScopeAttribute.EScope.PIMAttribute)]
    public class guiSelectNextCommand : guiSelectCommandBase
    {
        public override System.Windows.Input.KeyGesture Gesture
        {
            get
            {
                return KeyGestures.Tab;
            }
        }

        public override string ScreenTipText
        {
            get { return "Next construct"; }
        }

        protected override int IndexOperation(int index)
        {
            return index + 1;
        }
    }

    [Scope(ScopeAttribute.EScope.PIMClass | ScopeAttribute.EScope.PIMAttribute)]
    public class guiSelectPrevCommand : guiSelectCommandBase
    {
        public override System.Windows.Input.KeyGesture Gesture
        {
            get
            {
                return KeyGestures.ShiftTab;
            }
        }

        public override string ScreenTipText
        {
            get { return "Previous construct"; }
        }

        protected override int IndexOperation(int index)
        {
            return index - 1;
        }
    }
}