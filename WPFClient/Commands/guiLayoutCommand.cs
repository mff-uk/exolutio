using System;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.View;
using Exolutio.View.Commands;

namespace Exolutio.WPFClient.Commands
{
    public class guiLayoutCommand : guiCommandBase
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
        }
            
        public enum ELayoutType
        {
            PIMLeftPSMRight
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
                if (LayoutType == ELayoutType.PIMLeftPSMRight)
                {
                    return "PIM left PSM right";
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
            return true;
        }
    }
}