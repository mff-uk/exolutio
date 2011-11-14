using System;
using System.Xml.Linq;
using Exolutio.Model.PSM;
using Exolutio.Model.PSM.Grammar.SchematronTranslation;
using Exolutio.ResourceLibrary;
using Exolutio.SupportingClasses;

namespace Exolutio.View.Commands.Grammar
{
    public class guiGenerateSchematronSchemaCommand : guiActiveDiagramCommand
    {
        public override void Execute(object parameter = null)
        {
            if (Current.ActiveDiagram != null && Current.ActiveDiagram is PSMDiagram)
            {
                SchematronSchemaGenerator schemaGenerator = new SchematronSchemaGenerator();
                schemaGenerator.Initialize((PSMSchema)Current.ActiveDiagram.Schema);
                
                XDocument schematronSchemaDocument = schemaGenerator.GetSchematronSchema();

                if (Environment.MachineName.Contains("TRUPIK"))
                {
                    schematronSchemaDocument.Save(@"D:\Programování\EVOXSVN\SchematronTest\LastSchSchema.sch");
                }

                Current.MainWindow.FilePresenter.DisplayFile(schematronSchemaDocument, EDisplayedFileType.SCH, Current.ActiveDiagram.Caption + ".sch", schemaGenerator.Log);
            }
        }

        public override string Text
        {
            get { return "Generate Schematron schema"; }
        }

        public override string ScreenTipText
        {
            get { return "Generate Schematron schema from the OCL scripts defined for this PSM schema"; }
        }

        public override bool CanExecute(object parameter)
        {
            return Current.ActiveDiagram != null && Current.ActiveDiagram is PSMDiagram;
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.bilby); }
        }
    }
}