using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Reflection;
using Exolutio.Dialogs;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;

#if SILVERLIGHT
using SilverFlow.Controls;
#endif

namespace Exolutio.View.Commands
{
    public class guiControllerCommand : guiCommandBase
    {
        public guiControllerCommand()
        {
            Current.ActiveDiagramChanged += Current_ActiveDiagramChanged;
            Current.SelectionChanged += delegate { OnCanExecuteChanged(null); };
        }

        private void Current_ActiveDiagramChanged()
        {
            Diagram = Current.ActiveDiagram;
            ScopeObject = null;
            OnCanExecuteChanged(null);
        }

        public CommandBase ControllerCommand { get; set; }

        public Diagram Diagram { get; set; }

        private Type controllerCommandType;
        public Type ControllerCommandType
        {
            private get { return controllerCommandType; }
            set
            {
                controllerCommandType = value;
                if (string.IsNullOrEmpty(Text) && value != null)
                {
                    CommandDescriptor commandDescriptor = PublicCommandsHelper.GetCommandDescriptor(value);
                    Text = commandDescriptor.CommandDescription;
                }
            }
        }

        public delegate CommandBase ControllerCommandFactoryMethodDelegate();

        internal ControllerCommandFactoryMethodDelegate ControllerCommandFactoryMethod { get; set; }

        private List<Control> controls;

        private object scopeObject;
        public object ScopeObject
        {
            get { return scopeObject; }
            set
            {
                if (ScopeObjectConvertor != null)
                    scopeObject = ScopeObjectConvertor(value);
                else
                    scopeObject = value;
                OnCanExecuteChanged(null);
            }
        }

        public delegate object ScopeObjectConvertorDelegate(object value);

        public ScopeObjectConvertorDelegate ScopeObjectConvertor;

        public bool NoScope { get; set; }

        /// <summary>
        /// Use the currently selected item in the schema as scope object 
        /// </summary>
        public bool ScopeIsSelectedComponent { get; set; }

        public Type AcceptedSelectedComponentType { get; set; }

        public override void Execute(object parameter)
        {
            if (ScopeIsSelectedComponent && AcceptedSelectedComponentType != null && Current.ActiveDiagramView.IsSelectedComponentOfType(AcceptedSelectedComponentType))
            {
                ScopeObject = Current.ActiveDiagramView.GetSelectedComponents().First();
            }

            CommandDialogWindow w = null;
            controls = new List<Control>();

            #region substitute scope with diagram
            if (ScopeObject == null && Diagram != null)
            {
                CommandDescriptor commandDescriptor = PublicCommandsHelper.GetCommandDescriptor(ControllerCommandType);
                if (string.IsNullOrEmpty(ControllerCommandDescription))
                {
                    ControllerCommandDescription = commandDescriptor.CommandDescription;
                }
                commandDescriptor.ClearParameterValues();
                OperationParametersControlCreator.ReadParameterValues(commandDescriptor, controls);
                foreach (ParameterDescriptor parameterDescriptor in commandDescriptor.Parameters)
                {
                    if (parameterDescriptor.ParameterPropertyInfo == commandDescriptor.ScopeProperty)
                    {
                        if (parameterDescriptor.ComponentType != null &&
                            typeof(Diagram).IsAssignableFrom(parameterDescriptor.ComponentType))
                        {
                            ScopeObject = Diagram;
                        }

                        if (parameterDescriptor.ComponentType == typeof(PSMSchema) && Diagram is PSMDiagram)
                        {
                            ScopeObject = Diagram.Schema;
                        }
                    }
                }
            }
            #endregion

            if (OpenDialog)
            {
                w = new CommandDialogWindow();

                MenuHelper.CreateDialogControlsForCommand(ControllerCommandType, (ExolutioObject)ScopeObject, ProjectVersion, w.spParameters,
                                                          out controls);
                w.lTitle.Content = ControllerCommandDescription;
                if (NoScope)
                {
                    w.lTarget.Content = " (global command) ";
                }
                else
                {
                    w.lTarget.Content = ScopeObject.ToString();
                }
            }

            bool dialogOk = !OpenDialog;

            if (!dialogOk)
            {
#if SILVERLIGHT
                Current.MainWindow.FloatingWindowHost.Add(w);
                w.Closed += new EventHandler(w_Closed);
                w.ShowModal();
#else
                w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                //w.ShowDialog();
                w.WindowStyle = WindowStyle.ToolWindow;
                w.Topmost = true;
                w.ShowInTaskbar = false;
                w.ShowActivated = true;
                Current.MainWindow.DisableCommands();
                w.Show();
                w.Closed += w_Closed;
#endif
            }
            else
            {
                DoExecute();
            }

            //if (!OpenDialog || dialogOk)
            //{
            //    DoExecute();
            //}
        }

        void w_Closed(object sender, EventArgs e)
        {
            bool? dialogResult = ((CommandDialogWindow)sender).DialogResult;
            if (dialogResult == true)
            {
                DoExecute();
            }
        }


        private void DoExecute()
        {
            CommandDescriptor commandDescriptor =
                PublicCommandsHelper.GetCommandDescriptor(ControllerCommandType);
            commandDescriptor.ClearParameterValues();
            OperationParametersControlCreator.ReadParameterValues(commandDescriptor, controls);
            foreach (ParameterDescriptor parameterDescriptor in commandDescriptor.Parameters)
            {
                if (parameterDescriptor.ParameterPropertyInfo == commandDescriptor.ScopeProperty)
                {
                    parameterDescriptor.ParameterValue = ((ExolutioObject)this.ScopeObject).ID;
                }
            }
            CreateControllerCommand();
            CommandSerializer.FillParameters(ControllerCommand, commandDescriptor);
            ControllerCommand.CanExecuteChanged -= OnControllerCommandCanExecuteChanged;
            if (!ControllerCommand.CanExecute())
            {
#if SILVERLIGHT
                ExolutioErrorMsgBox.Show("Command can not be executed", ControllerCommand.ErrorDescription,
                                 Current.MainWindow.FloatingWindowHost);
#else
                ExolutioErrorMsgBox.Show("Command can not be executed", ControllerCommand.ErrorDescription);
#endif
            }
            else
            {
                if (ControllerCommand is StackedCommand)
                {
                    if (((StackedCommand)ControllerCommand).Controller == null)
                    {
                        ((StackedCommand)ControllerCommand).Controller = Current.Controller;
                    }
                }
                ControllerCommand.Execute();
            }
            ControllerCommand = null;
        }

        public string ControllerCommandDescription { get; set; }

        private string text;

        public override string Text
        {
            get { return !string.IsNullOrEmpty(text) ? text : ControllerCommandDescription; }
            set { text = value; }
        }

        public override string ScreenTipText
        {
            get { return ControllerCommandDescription; }
        }

        public bool OpenDialog { get; set; }

        private ProjectVersion projectVersion;

        public ProjectVersion ProjectVersion
        {
            get
            {

                if (projectVersion == null && Diagram != null)
                {
                    return Diagram.ProjectVersion;
                }
                else
                {
                    return projectVersion;
                }
            }
            set { projectVersion = value; }
        }

        public bool PIMOnly { get; set; }

        public bool PSMOnly { get; set; }

        public override bool CanExecute(object parameter)
        {
            if (PIMOnly)
            {
                if (Current.ActiveDiagram == null || !(Current.ActiveDiagram.Schema is PIMSchema))
                {
                    return false;
                }
            }

            if (PSMOnly)
            {
                if (Current.ActiveDiagram == null || !(Current.ActiveDiagram.Schema is PSMSchema))
                {
                    return false;
                }
            }

            if (ScopeIsSelectedComponent && AcceptedSelectedComponentType != null &&
                Current.ActiveDiagramView != null &&
                Current.ActiveDiagramView.IsSelectedComponentOfType(AcceptedSelectedComponentType))
            {
                if (Diagram == null)
                    return false;
                // this silent assignment does not trigger CanExecuteChanged
                scopeObject = Current.ActiveDiagramView.GetSelectedComponents().First();
                return true;
            }
            else
            {
                if (ScopeIsSelectedComponent)
                {
                    // this silent assignment does not trigger CanExecuteChanged
                    scopeObject = null;
                }
            }

            return NoScope || ScopeObject != null;
        }

        public void CreateControllerCommand()
        {
            CommandBase controllerCommand = ControllerCommandFactoryMethod();
            if (controllerCommand is ICommandWithDiagramParameter)
            {
                if (Diagram == null)
                {
                    Diagram = Current.ActiveDiagram;
                }
                if (Diagram == null)
                {
                    return;
                }
                ((ICommandWithDiagramParameter)controllerCommand).SchemaGuid = Diagram.Schema.ID;
                ((ICommandWithDiagramParameter)controllerCommand).DiagramGuid = Diagram.ID;
            }
            if (controllerCommand is StackedCommand)
            {
                ((StackedCommand)controllerCommand).Controller = Current.Controller;
            }
            ControllerCommand = controllerCommand;
        }

        private void OnControllerCommandCanExecuteChanged(object sender, EventArgs args)
        {
            OnCanExecuteChanged(args);
        }
    }

    public static class guiControllerCommandScopeConvertors
    {
        public static object DiagramToSchema(object diagram)
        {
            if (diagram is Diagram)
            {
                return ((Diagram)diagram).Schema;
            }
            else return diagram;
        }
    }
}