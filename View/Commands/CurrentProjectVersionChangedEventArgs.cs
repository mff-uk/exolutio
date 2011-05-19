using System;
using EvoX.Model;

namespace EvoX.View
{
    public class CurrentProjectVersionChangedEventArgs: EventArgs
    {
        public ProjectVersion OldProjectVersion { get; private set; }

        public ProjectVersion NewProjectVersion { get; private set; }

        public CurrentProjectVersionChangedEventArgs(ProjectVersion oldProjectVersion, ProjectVersion newProjectVersion)
        {
            OldProjectVersion = oldProjectVersion;
            NewProjectVersion = newProjectVersion;
        }
    }
}