using System;
using System.Collections.Generic;

namespace EvoX.View.Commands.Project
{
    public class guiOpenWebProjectCommand: guiProjectCommand
    {
        public delegate void GetServerProjectListHandler();

        public delegate void OpenServerWebProjectHandler(string projectName);

        public IEnumerable<string> ServerProjectList { get; set; }

        public GetServerProjectListHandler GetServerProjectList { get; set; }
        
        public OpenServerWebProjectHandler OpenServerWebProject { get; set; }

        public override bool CanExecute(object parameter)
        {
            return true; 
        }

        public override void Execute(object parameter)
        {
            GetServerProjectList();
            ServerProjectsWindow w = new ServerProjectsWindow();
            w.Projects = ServerProjectList;
            w.Closed += DoExecute;
            Current.MainWindow.FloatingWindowHost.Add(w);
            w.ShowModal();
        }

        private void DoExecute(object sender, EventArgs eventArgs)
        {
            ServerProjectsWindow w = (ServerProjectsWindow) sender;
            if (w.DialogResult == true && !string.IsNullOrEmpty(w.SelectedProject))
            {
                OpenServerWebProject(w.SelectedProject);
            }
        }

        public override string Text
        {
            get { return "Open web project"; }
        }

        public override string ScreenTipText
        {
            get { return "Open project from the web repository of examples"; }
        }
    }
}