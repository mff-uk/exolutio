using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EvoX.Dialogs;
using EvoX.Model.Serialization;
using EvoX.ResourceLibrary;

namespace EvoX.View.Commands.Project
{
    public class guiNewProjectCommand : guiProjectCommand
	{
		#region Overrides of EvoXGuiCommandBase

	    public override KeyGesture Gesture
		{
			get { return KeyGestures.ControlN; }
		}

		public override string Text
		{
			get { return CommandsResources.guiNewProjectCommand_Text_New_project; }
		}

		public override string ScreenTipText
		{
			get { return CommandsResources.guiNewProjectCommand_ScreenTipText_Creates_new_empty_EvoX_project; }
		}
		
		public override ImageSource Icon
		{
            get { return EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.GenericDocument); }
		}

		#endregion

		public override void Execute(object parameter = null)
		{
			// Close existing project
            if (Current.Project != null)
            {
                EvoXGuiCommands.CloseProjectCommand.Execute();
            }

            ProjectSerializationManager serializationManager = new ProjectSerializationManager();
		    Current.Project = serializationManager.CreateEmptyProject();
            Current.MainWindow.CloseRibbonBackstage();
		}

		public override bool CanExecute(object parameter = null)
		{
			return true;
		}

    }
}