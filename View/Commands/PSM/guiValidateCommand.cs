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
    public class guiValidateCommand : guiActiveDiagramCommand
    {
        public IReportDisplay ReportDisplay { get; set; }

        public override void Execute(object parameter)
        {
            if (Current.ActiveDiagram != null && Current.ActiveDiagram.Schema is PSMSchema)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Multiselect = false;

                String fileToCheck = null;

                if (openFileDialog.ShowDialog() == DialogResult.OK) {
                    fileToCheck = openFileDialog.FileName;              

                    PushDownAutomat pda = new PushDownAutomat();
                    ValidationResult validationResult = null;
                    try
                    {
                        pda.inicialize((PSMSchema)Current.ActiveDiagram.Schema, fileToCheck);
                    }
                    catch (System.Xml.XmlException e)
                    {
                        validationResult = new ValidationResult(false, e.Message);
                    }                    
                    if (validationResult == null)
                    {
                        validationResult = pda.processTree();
                    }
                    ExolutioMessageBox.Show("Validation", "Validation Result", validationResult.Message);
                }
            }
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