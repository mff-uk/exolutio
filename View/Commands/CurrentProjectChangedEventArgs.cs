using System;
using Exolutio.Model;

namespace Exolutio.View
{
    public class CurrentProjectChangedEventArgs:EventArgs
    {
        public Project OldProject { get; private set; }

        public Project NewProject { get; private set; }

        public Controller.Controller OldController { get; set; }

        public Controller.Controller NewController { get; set; }

        public CurrentProjectChangedEventArgs(Project oldProject, Project newProject)
        {
            OldProject = oldProject;
            NewProject = newProject;
        }
    }
}