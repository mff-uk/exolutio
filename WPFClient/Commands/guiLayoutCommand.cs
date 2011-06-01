using System;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.View;
using Exolutio.View.Commands;

namespace Exolutio.WPFClient.Commands
{
    public class guiLayoutCommand : guiActiveDiagramCommand
    {
        public override void Execute(object parameter)
        {

            MainWindow mainWindow = ((MainWindow) Current.MainWindow);
            mainWindow.DiagramTabManager.CloseAllTabs();

            if (LayoutType == ELayoutType.PIMLeftPSMRight)
            {
                foreach (PIMDiagram pimDiagram in Current.ProjectVersion.PIMDiagrams)
                {
                    mainWindow.DiagramTabManager.ActivateDiagram(pimDiagram);
                }

                bool psmGroup = false;

                foreach (PSMDiagram psmDiagram in Current.ProjectVersion.PSMDiagrams)
                {
                    DiagramTab t = mainWindow.DiagramTabManager.ActivateDiagram(psmDiagram);
                    if (!psmGroup)
                    {
                        mainWindow.dockManager.MainDocumentPane.CreateNewVerticalTabGroup();
                        psmGroup = true;
                    }
                }
                mainWindow.DiagramTabManager.ActivateDiagram(Current.ProjectVersion.PIMDiagrams[0]);
            }

            if (LayoutType == ELayoutType.ByVersions)
            {
                int i = 0;
                bool tabForVersion = false;
                foreach (ProjectVersion projectVersion in Current.Project.ProjectVersions)
                {
                    foreach (PIMDiagram pimDiagram in projectVersion.PIMDiagrams)
                    {
                        mainWindow.DiagramTabManager.ActivateDiagram(pimDiagram);
                        if (i > 0 && !tabForVersion)
                        {
                            mainWindow.dockManager.MainDocumentPane.CreateNewVerticalTabGroup();
                            tabForVersion = true;
                        }
                    }

                    foreach (PSMDiagram psmDiagram in projectVersion.PSMDiagrams)
                    {
                        mainWindow.DiagramTabManager.ActivateDiagram(psmDiagram);
                        if (i > 0 && !tabForVersion)
                        {
                            mainWindow.dockManager.MainDocumentPane.CreateNewVerticalTabGroup();
                            tabForVersion = true;
                        }
                    }

                    i++;
                }
            }
        }
            
        public enum ELayoutType
        {
            PIMLeftPSMRight,
            ByVersions
        }

        public ELayoutType LayoutType { get; set; }

        public override string ScreenTipText
        {
            get { return string.Empty; }
        }

        public override string Text
        {
            get
            {
                switch (LayoutType)
                {
                    case ELayoutType.PIMLeftPSMRight:
                        return "PIM left PSM right";
                        break;
                    case ELayoutType.ByVersions:
                        return "Layout by versions";
                        break;
                }
                return String.Empty;
            }
            set
            {
                base.Text = value;
            }
        }

        public override bool CanExecute(object parameter)
        {
            if (LayoutType == ELayoutType.ByVersions)
            {
                return Current.Project != null && Current.Project.UsesVersioning;
            }
            return Current.Project != null;
        }
    }
}