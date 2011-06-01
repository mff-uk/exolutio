using System.Windows;
using Exolutio.View;
using Exolutio.View.Commands;
using SilverFlow.Controls;
using SilverlightClient.ExolutioService;
using SilverlightClient.W;

namespace SilverlightClient
{
    public partial class MainPage
    {
        void ServerCommunication_ServerProjectListLoaded(object sender, GetProjectFilesCompletedEventArgs e)
        {
            try
            {
                GuiCommands.OpenWebProjectCommand.ServerProjectList = e.Result;
            }
            catch
            {

            }
        }

        private void InitializeRibbon()
        {
            ExolutioRibbon.bOpen.Command = GuiCommands.OpenProjectCommand;
            ExolutioRibbon.bNew.Command = GuiCommands.NewProjectCommand;
            ExolutioRibbon.bOpenWeb.Command = GuiCommands.OpenWebProjectCommand;
            ExolutioRibbon.bSaveToClient.Command = GuiCommands.SaveAsProjectCommand;
            ExolutioRibbon.bUndo.Command = GuiCommands.UndoCommand;
            ExolutioRibbon.bRedo.Command = GuiCommands.RedoCommand;
            ExolutioRibbon.bNormalize.Command = GuiCommands.NormalizeSchemaCommandCommand;
            ExolutioRibbon.bVerifyNormalized.Command = GuiCommands.TestNormalizationCommand;
            ExolutioRibbon.bGenerateGrammar.Command = GuiCommands.GenerateGrammarCommand;
            ExolutioRibbon.bHelp.Command = GuiCommands.HelpCommand;
            ExolutioRibbon.bPIMClass.Command = GuiCommands.AddPIMClassCommand;
            ExolutioRibbon.bPIMAssociation.Command = GuiCommands.AddPIMAssociationCommand;
            ExolutioRibbon.bPIMAttribute.Command = GuiCommands.AddPIMAttributeCommand;
            ExolutioRibbon.bPSMClass.Command = GuiCommands.AddPSMClassCommand;
            ExolutioRibbon.bPSMAssociation.Command = GuiCommands.AddPSMAssociationCommand;
            ExolutioRibbon.bPSMAttribute.Command = GuiCommands.AddPSMAttributeCommand;
            ExolutioRibbon.bPSMContentModel.Command = GuiCommands.AddPSMContentModel;

            GuiCommands.TestNormalizationCommand.ReportDisplay = this.ReportDisplay;
            GuiCommands.OpenWebProjectCommand.OpenServerWebProject = ServerCommunication.LoadWebFile;
            GuiCommands.OpenWebProjectCommand.GetServerProjectList = ServerCommunication.GetServerProjects;
        }
        
        private void OpenStartupProject()
        {
            ServerCommunication.LoadWebFile("web-project.evox");
        }

        FloatingWindowHost IMainWindow.FloatingWindowHost
        {
            get { return floatingWindowHost; }
        }

        private void floatingWindowHost_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DockManager.Width = floatingWindowHost.ActualWidth - 0;
            DockManager.Height = floatingWindowHost.ActualHeight - 0;
        }

        public void Close()
        {
            // called from close command, but does not do anything
        }

        //private void LoadProject(Project newProject)
        //{
        //    if (newProject.LatestVersion.PIMDiagrams.Count == 0)
        //    {
        //        PIMDiagram pimDiagram = new PIMDiagram(newProject);
        //        newProject.SingleVersion.PIMDiagrams.Add(pimDiagram);
        //        pimDiagram.LoadSchemaToDiagram(newProject.SingleVersion.PIMSchema);
        //    }

        //    if (newProject.LatestVersion.PSMDiagrams.Count == 0)
        //    {
        //        foreach (PSMSchema psmSchema in newProject.LatestVersion.PSMSchemas)
        //        {
        //            PSMDiagram psmDiagram = new PSMDiagram(newProject);
        //            newProject.SingleVersion.PSMDiagrams.Add(psmDiagram);
        //            psmDiagram.LoadSchemaToDiagram(psmSchema);
        //        }
        //    }

        //    DiagramTabManager.BindToCurrentProject();
        //    DiagramTabManager.OpenTabsForCurrentProject();

        //}
    }
}