using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Reflection;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    public class acmdDeletePSMAttribute : StackedCommand
    {
        private Guid schemaGuid;

        private Guid classGuid;

        private int index;

        private Guid attributeGuid;

        public acmdDeletePSMAttribute(Controller c, Guid psmAttributeGuid)
            : base(c)
        {
            attributeGuid = psmAttributeGuid;
            schemaGuid = Project.TranslateComponent<PSMAttribute>(attributeGuid).PSMSchema;
        }

        public override bool CanExecute()
        {
            return attributeGuid != Guid.Empty && Project.VerifyComponentType<PSMAttribute>(attributeGuid)
                && schemaGuid != Guid.Empty && Project.VerifyComponentType<PSMSchema>(schemaGuid);
        }
        
        internal override void CommandOperation()
        {
            PSMAttribute a = Project.TranslateComponent<PSMAttribute>(attributeGuid);
            Report = new CommandReport(CommandReports.PSM_component_deleted, a);
            Project.TranslateComponent<PSMSchema>(schemaGuid).PSMAttributes.Remove(a);
            if (a.PSMClass != null)
            {
                classGuid = a.PSMClass;
                index = a.PSMClass.PSMAttributes.Remove(a);
            }
            else throw new ExolutioCommandException("Deleted attribute " + a.ToString() + " had null PSMClass", this);
            Project.mappingDictionary.Remove(attributeGuid);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            new PSMAttribute(Project, attributeGuid, Project.TranslateComponent<PSMClass>(classGuid), Project.TranslateComponent<PSMSchema>(schemaGuid), index);
            return OperationResult.OK;
        }
    }
}
