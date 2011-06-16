using System;
using System.Collections.Generic;
using System.Windows.Documents;
using Exolutio.Model;
using Exolutio.View;
using Exolutio.View.Commands;

namespace Exolutio.WPFClient.Commands
{
    public class guiShowVersionLinksAdorner : guiActiveDiagramCommand
    {
        MainWindowAdornerManager Manager = new MainWindowAdornerManager();

        public override void Execute(object parameter)
        {
            Manager.ShowLayer();
        }

        public override string ScreenTipText
        {
            get { return Text; }
        }

        public override bool CanExecute(object parameter)
        {
            return true;
        }
    }

    public class MainWindowAdornerManager
    {
        public MainWindow MainWindow
        {
            get { return (MainWindow)Current.MainWindow; }
        }

        public AdornerLayer GetAdorner ()
        {
            AdornerLayer adorner = AdornerLayer.GetAdornerLayer(MainWindow);
            
            return adorner;
        }

        private AdornerLayer adornerLayer;

        public void ShowLayer()
        {
            
            IList<DiagramView> topDiagramViews = MainWindow.DiagramTabManager.GetTopDiagramViews();
            if (topDiagramViews.Count == 2 && Current.Project.UsesVersioning)
            {
                Diagram d1 = topDiagramViews[0].Diagram; 
                Diagram d2 = topDiagramViews[1].Diagram;

                adornerLayer = GetAdorner();

                if (Current.Project.VersionManager.AreItemsLinked(d1, d2))
                {
                    foreach (DiagramView topDiagramView in topDiagramViews)
                    {

                    }
                }
            }
        }
    }
}