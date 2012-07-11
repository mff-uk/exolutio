using System;
using Exolutio.Model.Versioning;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Versioning
{
    public class cmdCreateVersionLink: StackedCommand
    {
        public cmdCreateVersionLink()
        {
        }

        public cmdCreateVersionLink(Controller controller) : base(controller)
        {
        }

        public override bool CanExecute()
        {
            if (Project.UsesVersioning && Item1ID != Guid.Empty && Item2ID != Guid.Empty)
            {
                ExolutioVersionedObject item1 = (ExolutioVersionedObject) Project.TranslateComponent(Item1ID);
                ExolutioVersionedObject item2 = (ExolutioVersionedObject) Project.TranslateComponent(Item2ID);

                if (item1.Version != item2.Version && 
                    item1.GetType() == item2.GetType() && 
                    !item1.ExistsInVersion(item2.Version) &&
                    !item2.ExistsInVersion(item1.Version))
                {
                    return true; 
                }
            }
            return false; 
        }

        public Guid Item1ID { get; set; }

        public Guid Item2ID { get; set; }

        internal override void CommandOperation()
        {
            ExolutioVersionedObject item1 = (ExolutioVersionedObject)Project.TranslateComponent(Item1ID);
            ExolutioVersionedObject item2 = (ExolutioVersionedObject)Project.TranslateComponent(Item2ID);

            Project.VersionManager.RegisterVersionLink(item1.Version, item2.Version, item1, item2);
            Report = new CommandReport("Version link created between {0}#{1} and {2}#{3}", item1.Version, item1, item2.Version, item2);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            ExolutioVersionedObject item1 = (ExolutioVersionedObject)Project.TranslateComponent(Item1ID);
            ExolutioVersionedObject item2 = (ExolutioVersionedObject)Project.TranslateComponent(Item2ID);
            Project.VersionManager.UnregisterVersionLink(item1, item2);
            return OperationResult.OK;
        }

        public void Set(IVersionedItem item1, IVersionedItem item2)
        {
            Item1ID = item1.ID;
            Item2ID = item2.ID;
        }
    }
}