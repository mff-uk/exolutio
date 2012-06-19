using System;
using System.Collections.Generic;
using System.Linq;
using Exolutio.Model.OCL;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.ConstraintConversion;
using Exolutio.Model.PSM;
using Exolutio.ResourceLibrary;

namespace Exolutio.View.Commands.OCL
{
    public class guiCreateOCLScriptCommand : guiActiveDiagramCommand
    {
        public OCLScript.EOclScriptType CreatedScriptType { get; set; }

        public override void Execute(object parameter = null)
        {
            if (Current.ActiveDiagram != null)
            {
                if (Current.ActiveDiagram.Schema != null)
                {
                    OCLScript oclScript = new OCLScript(Current.Project, Guid.NewGuid(), Current.ActiveDiagram.Schema);
                    oclScript.Type = CreatedScriptType;
                    if (CreatedScriptType == OCLScript.EOclScriptType.Validation)
                        oclScript.Contents = string.Format("-- new empty validation script, created {0}. ", DateTime.Now);
                    else if (CreatedScriptType == OCLScript.EOclScriptType.Evolution)
                        oclScript.Contents = string.Format("-- new empty evolution script, created {0}. ", DateTime.Now);
                    Current.ActiveOCLScript = oclScript;
                }
            }
        }

        public override string Text
        {
            get
            {
                if (CreatedScriptType == OCLScript.EOclScriptType.Validation)
                    return "New OCL Script";
                else
                    return "New OCL Evolution Script";
            }
        }

        public override string ScreenTipText
        {
            get { return "Create OCL Script for current schema"; }
        }

        public override bool CanExecute(object parameter = null)
        {
            if (CreatedScriptType == OCLScript.EOclScriptType.Evolution)
                return Current.ActiveDiagram != null && Current.Project.UsesVersioning
                    && Current.Project.VersionManager.GetAllVersionsOfItem(Current.ActiveDiagram.Schema).Count() > 1;
            else
                return Current.ActiveDiagram != null;
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.document_add); }

        }
    }
}