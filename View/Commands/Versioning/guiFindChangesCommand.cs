using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Exolutio.Dialogs;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.Model.Versioning;
using Exolutio.ResourceLibrary;
using Exolutio.Revalidation;

namespace Exolutio.View.Commands.Versioning
{
    public class guiFindChangesCommand : guiActiveDiagramCommand
    {
        public override void Execute(object parameter)
        {
            IEnumerable<ExolutioVersionedObject> openedVersions = Current.MainWindow.DiagramTabManager.AnotherOpenedVersions(Current.ActiveDiagram);
            if (openedVersions!= null && openedVersions.Count() == 1)
            {
                FindAndDisplayChanges(((PSMDiagram)openedVersions.First()).PSMSchema, ((PSMDiagram)Current.ActiveDiagram).PSMSchema);
            }
            else
            {
                IEnumerable<PSMSchema> versionedItems =
                    Current.Project.VersionManager.GetAllVersionsOfItem(Current.ActiveDiagram.Schema).Cast<PSMSchema>();
                SelectItemsDialog dialog = new SelectItemsDialog();
                dialog.UseRadioButtons = true;
                List<PSMSchema> psmSchemata = versionedItems.ToList();
                psmSchemata.Remove((PSMSchema) Current.ActiveDiagram.Schema);
                dialog.SetItems(psmSchemata);
                dialog.ShortMessage = "Select version";
                dialog.LongMessage = "Select version of the schema you wish to compare";
                dialog.ShowDialog();

                if (dialog.DialogResult == true && dialog.selectedObjects.Count == 1)
                {
                    FindAndDisplayChanges((PSMSchema)dialog.selectedObjects.First(), ((PSMDiagram)Current.ActiveDiagram).PSMSchema);
                }
            }
        }

        public static void FindAndDisplayChanges(PSMSchema schemaVersion1, PSMSchema schemaVersion2)
        {
            ChangeDetector changeDetector = new ChangeDetector();
            DetectedChangesSet detectedChangesSet = changeDetector.DetectChanges(schemaVersion1, schemaVersion2);
            EvolutionChangesWindow.Show(detectedChangesSet, Current.MainWindow, schemaVersion1.PSMDiagram, schemaVersion2.PSMDiagram);
        }

        public override string Text
        {
            get { return "Find changes"; }
        }

        public override string ScreenTipText
        {
            get { return "Find changes between two versions of a schema"; }
        }

        public override bool CanExecute(object parameter)
        {
            return Current.ActiveDiagram != null && Current.ActiveDiagram is PSMDiagram && Current.Project.UsesVersioning && 
                Current.Project.VersionManager.GetAllVersionsOfItem(Current.ActiveDiagram).Count > 0;
        }

        public override ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.magnifier); }
        }

    }
}