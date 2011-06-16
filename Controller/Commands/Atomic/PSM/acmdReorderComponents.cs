using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PSM;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    public class acmdReorderComponents<TComponentType> : StackedCommand where TComponentType : ExolutioObject
    {
        public acmdReorderComponents()
        {
            
        }

        public acmdReorderComponents(Controller c)
            : base(c)
        {
            
        }

        public UndirectCollection<TComponentType> OwnerCollection { get; set; }

        public List<Guid> ComponentGuids { get; set; }

        private List<Guid> oldOrder; 

        public override bool CanExecute()
        {
            return true;
        }

        internal override void CommandOperation()
        {
            //PSMClass c = Project.TranslateComponent<PSMAttribute>(ComponentGuids.First()).PSMClass;
            oldOrder = new List<Guid>();

            foreach (TComponentType component in OwnerCollection)
            {
                oldOrder.Add(component.ID);
            }

            OrderAsInList(ComponentGuids);

            Report = new CommandReport(CommandReports.Components_reordered);
        }

        internal override OperationResult UndoOperation()
        {
            OrderAsInList(oldOrder);
            return OperationResult.OK;
        }

        private void OrderAsInList(IEnumerable<Guid> componentGuids)
        {
            Dictionary<Guid, TComponentType> map = new Dictionary<Guid, TComponentType>();
            foreach (Guid componentGuid in componentGuids)
            {
                TComponentType a = Project.TranslateComponent<TComponentType>(componentGuid);
                OwnerCollection.RemoveAsGuidSilent(a);
                map[componentGuid] = a;
            }
            foreach (Guid attributeGuid in componentGuids)
            {
                OwnerCollection.AddAsGuidSilent(map[attributeGuid]);
            }
            NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OwnerCollection.InvokeCollectionChanged(e);
        }
    }
}
