using System;
using System.Windows;
using System.Xml.Linq;
using Exolutio.Controller;
using Exolutio.Controller.Commands;
using Exolutio.Dialogs;
using Exolutio.Model;
using Exolutio.ResourceLibrary;
using Exolutio.View;
using Exolutio.View.Commands;
using Fluent;
using Microsoft.Win32;

namespace Exolutio.WPFClient.Commands
{
    public abstract class guiLogCommandBase : guiCommandBase
    {
        private CommandLogger commandLogger;
        public CommandLogger CommandLogger
        {
            get { return commandLogger; }
            set
            {
                if (commandLogger != null)
                {
                    commandLogger.StateChanged -= CommandLoggerStateChaged;
                }
                commandLogger = value;
                if (commandLogger != null)
                {
                    commandLogger.StateChanged += CommandLoggerStateChaged;
                }
            }
        }

        public ToggleButton ToggleButton { get; set; }

        protected virtual void CommandLoggerStateChaged()
        {
            InvokeCanExecuteChanged();
        }
    }

    public class guiStartLogging: guiLogCommandBase
    {
        
        public override void Execute(object parameter = null)
        {
            if (parameter is Fluent.ToggleButton)
            {
                ToggleButton = (ToggleButton) parameter;
            }
            if (!CommandLogger.LoggingStarted)
            {
                CommandLogger.StartLogging();
            }
            else
            {
                CommandLogger.ContinueLogging();
            }
        }

        public override bool CanExecute(object parameter = null)
        {
            return !CommandLogger.LoggingStarted || CommandLogger.LoggingPaused;
        }

        public override string Text
        {
            get
            {
                return "Start logging";
            }
            set
            {
                base.Text = value;
            }
        }

        public override string ScreenTipText
        {
            get { return "Start new log session."; }
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.bullet_ball_red); }
        }

        protected override void CommandLoggerStateChaged()
        {
            base.CommandLoggerStateChaged();
            if (ToggleButton != null)
            {
                ToggleButton.IsChecked = CommandLogger.LoggingStarted && !CommandLogger.LoggingPaused;
            }
        }
    }

    public class guiPauseLogging : guiLogCommandBase
    {
        public override void Execute(object parameter = null)
        {
            if (parameter is Fluent.ToggleButton)
            {
                ToggleButton = (ToggleButton)parameter;
            }
            if (CommandLogger.LoggingStarted)
            {
                CommandLogger.PauseLogging();
                if (parameter is Fluent.ToggleButton)
                {
                    ((Fluent.ToggleButton)parameter).IsChecked = true;
                }
            }
            else
            {
                CommandLogger.ContinueLogging();
                if (parameter is Fluent.ToggleButton)
                {
                    ((Fluent.ToggleButton)parameter).IsChecked = false;
                }
            }
        }

        public override bool CanExecute(object parameter = null)
        {
            return !CommandLogger.LoggingPaused && CommandLogger.LoggingStarted;
        }

        public override string Text
        {
            get
            {
                return "Pause logging";
            }
            set
            {
                base.Text = value;
            }
        }

        public override string ScreenTipText
        {
            get { return "Pause current log session."; }
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.media_pause); }
        }

        protected override void CommandLoggerStateChaged()
        {
            base.CommandLoggerStateChaged();
            if (ToggleButton != null)
            {
                ToggleButton.IsChecked = CommandLogger.LoggingStarted && CommandLogger.LoggingPaused;
            }
        }
    }

    public class guiStopLogging : guiLogCommandBase
    {
        public override void Execute(object parameter = null)
        {
            if (parameter is Fluent.ToggleButton)
            {
                ToggleButton = (ToggleButton)parameter;
            }
            if (CommandLogger.LoggingStarted)
            {
                MessageBoxResult result = ExolutioYesNoBox.ShowYesNoCancel("Save log", "Do you wish to save recorded log file?");
                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
                if (result == MessageBoxResult.Yes)
                {
                    SaveFileDialog dlg = new SaveFileDialog
                    {
                        DefaultExt = ".eXoL",
                        Filter = "eXolutio log files (*.eXoL)|*.eXoL|XML files (*.xml)|*.xml|All files (*.*)|*.*||"
                    };

                    bool? sresult = dlg.ShowDialog();
                    if (sresult == true)
                    {
                        XDocument doc = CommandLogger.SerializationDocument;
                        doc.Save(dlg.FileName);
                        CommandLogger.StopLogging();
                    }
                }
                else
                {
                    CommandLogger.StopLogging();
                }
            }
        }

        public override bool CanExecute(object parameter = null)
        {
            return CommandLogger.LoggingStarted;
        }

        public override string Text
        {
            get
            {
                return "Stop logging";
            }
            set
            {
                base.Text = value;
            }
        }

        public override string ScreenTipText
        {
            get { return "Stop current log session."; }
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.media_stop); }
        }
    }
    
    public class guiLoadLog : guiLogCommandBase
    {
        public override void Execute(object parameter = null)
        {
            if (parameter is Fluent.ToggleButton)
            {
                ToggleButton = (ToggleButton)parameter;
            }

            OpenFileDialog ofd = new OpenFileDialog 
                    {
                        DefaultExt = ".eXoL",
                        Filter = "eXolutio log files (*.eXoL)|*.eXoL|XML files (*.xml)|*.xml|All files (*.*)|*.*||"
                    };

            if (ofd.ShowDialog() == true)
            {
                CommandLogger.LoadLogFile(ofd.FileName);
            }
        }

        public override bool CanExecute(object parameter = null)
        {
            return !CommandLogger.LoggingStarted;
        }

        public override string Text
        {
            get
            {
                return "Load log";
            }
            set
            {
                base.Text = value;
            }
        }

        public override string ScreenTipText
        {
            get { return "Load log session from a previously stored log file."; }
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.PasteBig); }
        }
    }

    public class guiExecuteNextInLog : guiLogCommandBase
    {
        public override void Execute(object parameter = null)
        {
            if (parameter is Fluent.ToggleButton)
            {
                ToggleButton = (ToggleButton) parameter;
            }

            CommandBase command = CommandLogger.GetNextCommand();
            if (command is StackedCommand)
            {
                ((StackedCommand) command).Controller = Current.Controller;

                ICommandWithDiagramParameter commandWithDiagramParameter = command as ICommandWithDiagramParameter;
                if (commandWithDiagramParameter != null && commandWithDiagramParameter.DiagramGuid != Guid.Empty)
                {
                    Diagram diagram = Current.Controller.Project.TranslateComponent<Diagram>(commandWithDiagramParameter.DiagramGuid);
                    if (diagram.Schema != null)
                        commandWithDiagramParameter.SchemaGuid = diagram.Schema;
                }
            }
            command.Execute();
        }

        public override bool CanExecute(object parameter = null)
        {
            return CommandLogger.CanContinueReplay;
        }

        public override string Text
        {
            get
            {
                return "Execute next";
            }
            set
            {
                base.Text = value;
            }
        }

        public override string ScreenTipText
        {
            get { return "Execute next command in the loaded log session."; }
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.media_play_green); }
        }
    }

    public class guiEndReplay : guiLogCommandBase
    {
        public override void Execute(object parameter = null)
        {
            if (parameter is Fluent.ToggleButton)
            {
                ToggleButton = (ToggleButton) parameter;
            }
            
            CommandLogger.EndReplay();
        }

        public override bool CanExecute(object parameter = null)
        {
            return CommandLogger.CanContinueReplay;
        }

        public override string Text
        {
            get
            {
                return "End replay";
            }
            set
            {
                base.Text = value;
            }
        }

        public override string ScreenTipText
        {
            get { return "End replay and disard the rest of the logged session. "; }
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.media_stop); }
        }
    }
}