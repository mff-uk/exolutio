using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Exolutio.Model.Serialization;
using Microsoft.Win32;
using Exolutio.ResourceLibrary;

namespace Exolutio.View.Commands.Project
{
    public class guiSaveAsProjectCommand : guiProjectCommand
    {
        #region Overrides of ExolutioGuiCommandBase

        public override KeyGesture Gesture
        {
            get { return KeyGestures.ControlShiftS; }
        }

        public override string Text
        {
            get { return CommandsResources.guiSaveAsProjectCommand_Text_Save_as; }
        }

        public override string ScreenTipText
        {
            get { return CommandsResources.guiSaveAsProjectCommand_ScreenTipText_Saves_Exolutio_project_under_a_given_file_name; }
        }

        public override ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.Save); }
        }

        #endregion

        public Model.Project SavedProject { get; set; }

        public override void Execute(object parameter = null)
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                DefaultExt = CommandsResources.guiOpenProjectCommand_Execute_Exolutio_Default_Extension,
                Filter = CommandsResources.guiOpenProjectCommand_Execute_Exolutio_files____Exolutio____Exolutio_XML_files____xml____xml_All_files____________
            };

            bool? result = dlg.ShowDialog();

            Model.Project p = SavedProject ?? Current.Project;

            if (result != true)
                return;
            
            {
                ProjectSerializationManager projectSerializationManager = new ProjectSerializationManager();
#if SILVERLIGHT
                projectSerializationManager.SaveProject(p, dlg.OpenFile());
#else
                projectSerializationManager.SaveProject(p, dlg.FileName);
                // save layout of project
                if (p.ProjectFile.Exists)
                {
                    Current.MainWindow.SaveProjectLayout(Current.MainWindow.UserFileForProjectFile(p.ProjectFile.FullName));
                }
#endif
            }

            #if SILVERLIGHT
            if (SavedProject == null)
            {
                p.ProjectFile = new FileInfo(Path.GetFullPath(dlg.SafeFileName));
            }
            #else 
            if (p != null)
            {
                p.ProjectFile = new FileInfo(Path.GetFullPath(dlg.FileName));
            }
            #endif
            Current.InvokeRecentFile(p.ProjectFile);
            Current.MainWindow.CloseRibbonBackstage();
        }

        public override bool CanExecute(object parameter = null)
        {
            return Current.Project != null;

        }
    }
}