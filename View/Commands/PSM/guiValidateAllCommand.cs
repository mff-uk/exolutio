using System;
using Exolutio.Controller.Commands;
using Exolutio.Dialogs;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.ResourceLibrary;
using Exolutio.Model.PSM.XMLValidation;
using System.Windows.Forms;

namespace Exolutio.View.Commands.PSM
{
    public class guiValidateAllCommand : guiActiveDiagramCommand
    {
        public IReportDisplay ReportDisplay { get; set; }

        public override void Execute(object parameter)
        {
            String validationResults = "";

            System.Collections.Generic.IList<DiagramView> diagrams = Current.MainWindow.DiagramTabManager.GetOpenedDiagramViews();
            foreach (DiagramView diagram in diagrams) {
                if (diagram.Diagram.Schema is PSMSchema) {
                    string coreFileName = ((PSMSchema)diagram.Diagram.Schema).PSMSchemaClass.Name;

                    System.IO.DirectoryInfo dirInfo = diagram.Diagram.Project.ProjectFile.Directory;
                    System.IO.FileInfo[] fileNames = dirInfo.GetFiles("*.xml");
                    foreach (System.IO.FileInfo file in fileNames) {
                        bool startsWith = file.Name.StartsWith(coreFileName);
                        bool followedByO = coreFileName.Length < file.Name.Length && file.Name.Substring(coreFileName.Length).StartsWith("O");
                        bool followedByF = coreFileName.Length < file.Name.Length && file.Name.Substring(coreFileName.Length).StartsWith("F");
                        if (startsWith &&(followedByO || followedByF))
                        {
                            PushDownAutomat pda = new PushDownAutomat();
                            ValidationResult validationResult = null;
                            bool validate = true;
                            try
                            {
                                pda.inicialize((PSMSchema)diagram.Diagram.Schema, file.FullName);
                            }
                            catch (System.Xml.XmlException e)
                            {
                                validationResult = new ValidationResult(false, e.Message);
                            }
                            catch (Exception e) {
                                validationResults += "Soubor " + file.Name + " neodpovida modelu " + coreFileName + ". Nastala chyba: " + e.Message + System.Environment.NewLine;
                                validate = false;
                            }
                            if (validate)
                            {
                                if (validationResult == null)
                                {
                                    validationResult = pda.processTree();
                                }
                                if ((!validationResult.Successful && file.Name.Substring(coreFileName.Length).StartsWith("O")) || validationResult.Successful && file.Name.Substring(coreFileName.Length).StartsWith("F"))
                                {
                                    validationResults += "Soubor " + file.Name + " neodpovida modelu " + coreFileName + "." + System.Environment.NewLine;
                                }
                            }
                        }

                    }
                }
            }

            ExolutioMessageBox.Show("Validation", "Validation Result", validationResults);                
            
        }

        public override string Text
        {
            get { return "Validation"; }
        }

        public override string ScreenTipText
        {
            get { return "Validate XML file against current PSM diagram"; }
        }

        public override bool CanExecute(object parameter)
        {
            // TODO
            return Current.ActiveDiagram != null && Current.ActiveDiagram is PSMDiagram;
        }

        public override System.Windows.Media.ImageSource Icon
        {
            // TODO
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.component_preferences); }
        }

    }
}