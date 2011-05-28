using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    public class acmdUpdatePSMAttributeXForm : StackedCommand
    {
        Guid attributeGuid;
        bool newForm, oldForm;

        public acmdUpdatePSMAttributeXForm(Controller c, Guid psmAttributeGuid, bool element)
            : base(c)
        {
            attributeGuid = psmAttributeGuid;
            newForm = element;
        }

        public override bool CanExecute()
        {
            return attributeGuid != Guid.Empty
                && Project.VerifyComponentType<PSMAttribute>(attributeGuid);
        }
        
        internal override void CommandOperation()
        {
            PSMAttribute psmAttribute = Project.TranslateComponent<PSMAttribute>(attributeGuid);
            oldForm = psmAttribute.Element;
            psmAttribute.Element = newForm;

            string newFormString = newForm ? "element" : "attribute";
            string oldFormString = oldForm ? "element" : "attribute";

            Report = new CommandReport("XML form of {0} changed from {1} to {2}.", psmAttribute, oldFormString, newFormString);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMAttribute psmAttribute = Project.TranslateComponent<PSMAttribute>(attributeGuid);
            psmAttribute.Element = oldForm;
            return OperationResult.OK;
        }
    }
}
