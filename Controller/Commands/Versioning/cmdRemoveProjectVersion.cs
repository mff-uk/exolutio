using System;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Versioning
{
    public class cmdRemoveProjectVersion : StackedCommand
    {
        public Guid ProjectVersionGuid { get; set; }

        public ProjectVersion ProjectVersion { get { return Project.TranslateComponent<ProjectVersion>(ProjectVersionGuid); } }

        protected override bool Undoable
        {
            //lot of work to make this undoable... 
            get { return false; }
        }

        public override bool CanExecute()
        {
            return Project.UsesVersioning && Project.ProjectVersions.Count > 1;
        }

        internal override void CommandOperation()
        {
            Project.VersionManager.DeleteVersion(ProjectVersion.Version);
            Report = new CommandReport(CommandReports.VERSION_REMOVED, ProjectVersion.Version);
        }

        internal override OperationResult UndoOperation()
        {
            throw new NotImplementedException();
        }
    }
}