using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Xml.Linq;
using Exolutio.Dialogs;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.ResourceLibrary;
using Exolutio.Revalidation;
using Exolutio.Revalidation.XSLT;

namespace Exolutio.View.Commands.Versioning
{
    public class guiRevalidation: guiActiveDiagramCommand
    {
        public override string Text
        {
            get { return "Revalidation"; }
        }

        public override string ScreenTipText
        {
            get { return "Generate revalidation script."; }
        }

        public override bool CanExecute(object parameter)
        {
            return Current.ActiveDiagram != null && Current.ActiveDiagram is PSMDiagram && Current.Project.UsesVersioning &&
                Current.Project.VersionManager.GetAllVersionsOfItem(Current.ActiveDiagram).Count() > 0;
        }

        public override ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.Validate); }
        }

        public override void Execute(object parameter)
        {
            IEnumerable<ExolutioVersionedObject> openedVersions = Current.MainWindow.DiagramTabManager.AnotherOpenedVersions(Current.ActiveDiagram);
            if (openedVersions != null && openedVersions.Count() == 1)
            {
                Revalidate(((PSMDiagram)openedVersions.First()).PSMSchema, ((PSMDiagram)Current.ActiveDiagram).PSMSchema);
            }
            else
            {
                IEnumerable<PSMSchema> versionedItems =
                    Current.Project.VersionManager.GetAllVersionsOfItem(Current.ActiveDiagram.Schema).Cast<PSMSchema>();
                SelectItemsDialog dialog = new SelectItemsDialog();
                dialog.UseRadioButtons = true;
                List<PSMSchema> psmSchemata = versionedItems.ToList();
                psmSchemata.Remove((PSMSchema)Current.ActiveDiagram.Schema);
                dialog.SetItems(psmSchemata);
                dialog.ShortMessage = "Select version";
                dialog.LongMessage = "Select version of the schema you wish to compare";
                dialog.ShowDialog();

                if (dialog.DialogResult == true && dialog.selectedObjects.Count == 1)
                {
                    Revalidate((PSMSchema)dialog.selectedObjects.First(), ((PSMDiagram)Current.ActiveDiagram).PSMSchema);
                }
            }
        }

        public static void Revalidate(PSMSchema schemaVersion1, PSMSchema schemaVersion2)
        {
            ChangeDetector changeDetector = new ChangeDetector();
            DetectedChangeInstancesSet detectedChangeInstancesSet = changeDetector.DetectChanges(schemaVersion1, schemaVersion2);
            Exolutio.Revalidation.XSLT.XsltRevalidationScriptGenerator g = new XsltRevalidationScriptGenerator();
            g.Initialize(schemaVersion1, schemaVersion2, detectedChangeInstancesSet);
            g.GenerateTemplateStructure();
            XDocument revalidationStylesheet = g.GetRevalidationStylesheet();
            revalidationStylesheet.Elements().First().AddFirst(new XComment(string.Format(" Template generated by eXolutio on {0} {1} \r\n       from {2}. ", System.DateTime.Now.ToShortDateString(), System.DateTime.Now.ToShortTimeString(), Current.Project.ProjectFile)));

            if (Environment.MachineName.Contains("TRUPIK"))
            {
                revalidationStylesheet.Save(XsltTestWindow.SAVE_STYLESHEET);
                if (schemaVersion1.Project.ProjectFile != null)
                {
                    string ls = string.Format("{0}\\{1}", schemaVersion1.Project.ProjectFile.Directory.FullName, "LastStylesheet.xslt");
                    revalidationStylesheet.Save(ls);
                }
            }
            //else
            {
                XsltTestWindow.ShowDialog(detectedChangeInstancesSet, schemaVersion1, schemaVersion2);    
            }
        }

    }
}