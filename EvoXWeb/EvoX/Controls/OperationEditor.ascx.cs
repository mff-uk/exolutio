using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EvoX.Controller.Commands;
using EvoX.Controller.Commands.Reflection;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Model.PSM.Grammar;
using EvoX.Model.PSM.Normalization;
using EvoX.Web.ModelHelper;
using EvoX.Web.OperationParameters;
using EvoX.Web.IO;

namespace EvoX.Web.Controls
{
    public partial class OperationEditor : System.Web.UI.UserControl
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            EnableUndoRedo();

            UpdateForNewProject(null);

            if (this.Parent != null && this.Parent.Parent != null && this.Parent.Parent is UpdatePanel)
            {
                UpdatePanel panel = (UpdatePanel) this.Parent.Parent;
                if (panel.Triggers.Count == 0)
                {
                    PostBackTrigger trigger = new PostBackTrigger();
                    trigger.ControlID = this.ID;
                    panel.Triggers.Add(trigger);
                }
            }
        }

        public void UpdateForNewProject(Project project)
        {
            if (project == null)
                project = ModelHelper.ModelHelper.GetSessionProject(Session);
            if (project != null)
            {
                ddlPSMSchemaSelector.ProjectVersion = project.SingleVersion;
                ddlPSMSchemaSelector.InitControl();
                ddlPSMSchemaSelector.DataBind();
            }
        }

        public event Action OperationExecuted;

        private void InvokeOperationExecuted()
        {
            if (OperationExecuted != null)
            {
                OperationExecuted();
            }
        }

        private void EnableUndoRedo()
        {
            Controller.Controller controller = ModelHelper.ModelHelper.GetOrCreateSessionController(Session);
            RedoCommand redo = new RedoCommand(controller);
            UndoCommand undo = new UndoCommand(controller);

            bUndo.Enabled = undo.CanExecute();
            bRedo.Enabled = redo.CanExecute();
        }

        private void ExecuteSelectedOperation()
        {
            int step = 0;
            grammarVisualizer.Visible = false;
            try
            {
                string selectedOperationStr = ddlAvailableOperations.SelectedValue;
                string operationHumanFriendlyName = ddlAvailableOperations.SelectedItem.Text;
                CommandDescriptor commandDescriptor =
                    PublicCommandsHelper.GetCommandDescriptor(selectedOperationStr);
                OperationParametersControlCreator.ReadParameterValues(commandDescriptor, panelParams.Controls);
                Project project = ModelHelper.ModelHelper.GetSessionProject(Session);
                CommandSerializer commandSerializer = new CommandSerializer();
                Controller.Controller controller = ModelHelper.ModelHelper.GetOrCreateSessionController(Session);
                CommandBase command = commandSerializer.CreateCommand(controller, commandDescriptor);
                step = 1;
                if (command.CanExecute())
                {
                    step = 2;
                    command.Execute();
                    step = 3;
                    Tests.ModelIntegrity.ModelConsistency.CheckProject(project);
                    step = 4;
                    SaveModifiedProject(project);
                    step = 5;
                    ddlAvailableOperations.SelectedIndex = 0;
                    ClearParamsPanel(true);
                    lCommandResult.Text = string.Format("Operation '{0}' executed succesfully. ", operationHumanFriendlyName);
                    lCommandResult.Visible = true;
                    if (command is MacroCommand)
                    {
                        NestedCommandReport commandReports = ((MacroCommand) command).GetReport();
                        //repResult.DataSource = commandReports;
                        //repResult.Visible = true; 
                        //repResult.DataBind();
                        reportDisplay.DisplayedReport = commandReports;
                        reportDisplay.Visible = true;
                        reportDisplay.DataBind();
                    }
                    else
                    {
                        reportDisplay.Visible = false;
                        //repResult.Visible = false;
                    }
                }
                else
                {
                    lCommandResult.Text = "Command can not execute. " + command.ErrorDescription;
                    lCommandResult.Visible = true;
                    lCommandResult.ForeColor = Color.Red;
                    lCommandResult.Font.Bold = true;
                }
            }
            catch (Exception ex)
            {
                switch (step)
                {
                    case 0:
                        lCommandResult.Text = "Command can not execute. Error occured during command inicialization: " + ex.Message;
                        break;
                    case 1:
                        lCommandResult.Text = "Command can not execute. Exception thrown by CanExecute method: " + ex.Message;
                        break;
                    case 2:
                        lCommandResult.Text = "Command can not execute. Exception thrown by Execute method: " + ex.Message;
                        break;
                    case 3:
                        lCommandResult.Text = "Command executed, but left the model inconsistent. Error: " + ex.Message;
                        break;
                    case 4:
                        lCommandResult.Text = "Command was executed, but model became inconsistent. Error: " + ex.Message;
                        break;
                    case 5:
                        lCommandResult.Text = "Command was executed, but saving modified project failed, possibly due to model inconsistence. Error: " + ex.Message;
                        break;
                    default:
                        lCommandResult.Text = "Command executed, but error occured during page cleanup: " + ex.Message;
                        break;
                }
                    
                lCommandResult.Visible = true;
                lCommandResult.ForeColor = Color.Red;
                lCommandResult.Font.Bold = true;
            }
            EnableUndoRedo();
            InvokeOperationExecuted();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadOperations();
            }

            bool clear = false;
            if (IsPostBack && LastOperationIndex.HasValue && LastOperationIndex.Value != ddlAvailableOperations.SelectedIndex)
            {
                clear = true;
                LastOperationIndex = ddlAvailableOperations.SelectedIndex;
            }

            if (IsPostBack && ddlAvailableOperations.SelectedIndex > 0)
            {
                LoadParameterDefinition(clear);
            }
            else
            {
                ClearParamsPanel(true);
            }

            lCommandResult.Visible = false;
            lCommandResult.Text = String.Empty; 
            lCommandResult.ForeColor = Color.Black;
            lCommandResult.Font.Bold = false; 
        }

        public int ? LastOperationIndex
        {
            get { return (int?) ViewState["LastOperationIndex"]; }
            set { ViewState["LastOperationIndex"] = value; }
        }

        protected void ddlAvailableOperations_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            LastOperationIndex = ddlAvailableOperations.SelectedIndex;
            if (ddlAvailableOperations.SelectedIndex > 0)
            {
                LoadParameterDefinition(true);
            }
            else
            {
                ClearParamsPanel(true); 
            }
            //repResult.Visible = false;
            reportDisplay.Visible = false;
            grammarVisualizer.Visible = false;
        }

        private void LoadParameterDefinition(bool clearState)
        {
            string selectedOperationStr = ddlAvailableOperations.SelectedValue;
            CommandDescriptor parametersDescriptors = PublicCommandsHelper.GetCommandDescriptor(selectedOperationStr);
            CreateControlsForParameters(parametersDescriptors, clearState);
        }

        private void CreateControlsForParameters(CommandDescriptor parametersDescriptors, bool clearState)
        {
            ClearParamsPanel(clearState);

            if (ModelHelper.ModelHelper.GetSessionProject(Session) != null)
            {
                List<Control> controls = OperationParametersControlCreator.CreateControls(parametersDescriptors, ModelHelper.ModelHelper.GetSessionProject(Session).LatestVersion);
                foreach (Control control in controls)
                {
                    panelParams.Controls.Add(control);
                }
            }
        }

        private void ClearParamsPanel(bool clearState)
        {
            if (clearState)
                panelParams.ClearState();
            List<Control> c = new List<Control>();
            foreach (Control control in panelParams.Controls)
            {
                c.Add(control);
            }
            //panelParams.Controls.Clear();
            foreach (Control control in c)
            {
                panelParams.Controls.Remove(control);
            }
            reportDisplay.Visible = false;
        }

        protected void bExecute_OnClick(object sender, EventArgs e)
        {
            if (ddlAvailableOperations.SelectedIndex > 0)
            {
                ExecuteSelectedOperation();
            }
        }

        private void SaveModifiedProject(Project project)
        {
            //ModelHelper.ModelHelper.SetSessionProject(Session, project);

            //ModelHelper.ModelHelper.SetSessionProject(Session, project);

            if (Page is ISerializedProjectHolder)
            {
                ISerializedProjectHolder parentPage = (ISerializedProjectHolder) Page;
                parentPage.SerializedProject = IOHelper.SerializeProjectToString(project);
                parentPage.DisplayProject(parentPage.SerializedProject);
            }
        }

        Dictionary<string, Tuple<string, PublicCommandAttribute.EPulicCommandCategory>> availableOperations;

        public void LoadOperations()
        {
            ddlAvailableOperations.Items.Clear();
            ddlAvailableOperations.Items.Add(new ListItem("Choose operation...", String.Empty));
            
            availableOperations = ModelHelper.ModelHelper.GetAvailableOperations();
            PublicCommandAttribute.EPulicCommandCategory last = PublicCommandAttribute.EPulicCommandCategory.None;

            foreach (KeyValuePair<string, Tuple<string, PublicCommandAttribute.EPulicCommandCategory>> kvp in availableOperations)
            {
                PublicCommandAttribute.EPulicCommandCategory commandCategory = kvp.Value.Item2;
                if (commandCategory != last)
                {
                    string description = DescriptionAttribute.GetDescription(commandCategory);
                    ListItem separatorItem = new ListItem(string.Format(" --- {0} operations ---", description), String.Empty, true);
                    ddlAvailableOperations.Items.Add(separatorItem);
                    last = commandCategory;
                }
                ddlAvailableOperations.Items.Add(new ListItem(kvp.Value.Item1, kvp.Key));
            }
        }

        protected void bUndo_OnClick(object sender, EventArgs e)
        {
            Controller.Controller controller = ModelHelper.ModelHelper.GetOrCreateSessionController(Session);

            string description = null; 

            try
            {
                UndoCommand undo = new UndoCommand(controller);
                //description = undo.UnderlyingCommandDescription;
                undo.Execute();                
            }
            catch (Exception ex)
            {
                lCommandResult.Text = "Undo failed: " + ex.Message;
                lCommandResult.Visible = true;
                lCommandResult.ForeColor = Color.Red;
                lCommandResult.Font.Bold = true;
            }

            if (!String.IsNullOrEmpty(description))
            {
                lCommandResult.Text = string.Format("Operation '{0}' undone succesfully. ", description);
                lCommandResult.Visible = true;
            }
            else
            {
                lCommandResult.Text = string.Format("Operation undone succesfully. ");
                lCommandResult.Visible = true;
            }            

            EnableUndoRedo();
            reportDisplay.Visible = false;
            grammarVisualizer.Visible = false;
            SaveModifiedProject(controller.Project);
            InvokeOperationExecuted();
        }

        protected void bRedo_OnClick(object sender, EventArgs e)
        {
            Controller.Controller controller = ModelHelper.ModelHelper.GetOrCreateSessionController(Session);

            string description = null; 

            try
            {
                RedoCommand redo = new RedoCommand(controller);
                redo.Execute();
            }
            catch (Exception ex)
            {
                lCommandResult.Text = "Redo failed: " + ex.Message;
                lCommandResult.Visible = true;
                lCommandResult.ForeColor = Color.Red;
                lCommandResult.Font.Bold = true;
            }

            if (!String.IsNullOrEmpty(description))
            {
                lCommandResult.Text = string.Format("Operation '{0}' redone succesfully. ", description);
                lCommandResult.Visible = true;
            }
            else
            {
                lCommandResult.Text = string.Format("Operation redone succesfully. ");
                lCommandResult.Visible = true;
            }   

            EnableUndoRedo();
            reportDisplay.Visible = false;
            grammarVisualizer.Visible = false;
            SaveModifiedProject(controller.Project);
            InvokeOperationExecuted();
        }

        public void ClearResult()
        {
            reportDisplay.Visible = false;
            grammarVisualizer.Visible = false; 
        }

        public PSMSchema SelectedSchema
        {
            get { return ddlPSMSchemaSelector.Value; }
        }

        protected void bTestNormalization_Click(object sender, EventArgs e)
        {
            if (SelectedSchema != null)
            {
                ModelVerifier verifier = new ModelVerifier();
                if (!verifier.TestSchemaNormalized(SelectedSchema))
                {
                    reportDisplay.DisplayedLog = verifier.Log;
                    reportDisplay.Visible = true;
                    reportDisplay.DataBind();
                }
                else
                {
                    reportDisplay.DisplayedReport = new CommandReport("Schema is normalized. ");
                    reportDisplay.Visible = true;
                    reportDisplay.DataBind();                    
                }
            }
        }

        protected void bNormalizeSchema_Click(object sender, EventArgs e)
        {
            if (SelectedSchema != null)
            {
                Normalizer normalizer = new Normalizer();
                normalizer.Controller = ModelHelper.ModelHelper.GetOrCreateSessionController(Session);
                Project project = ModelHelper.ModelHelper.GetSessionProject(Session);
                normalizer.NormalizeSchema(SelectedSchema);
                SaveModifiedProject(project);
            }
        }

        protected void bGenerateRTG_Click(object sender, EventArgs e)
        {
            if (SelectedSchema != null)
            {
                GrammarGenerator generator = new GrammarGenerator();
                Grammar grammar = generator.GenerateGrammar(SelectedSchema);
                grammarVisualizer.Visible = true;
                reportDisplay.Visible = false;
                grammarVisualizer.Display(grammar);
            }
        }

        public void OnProjectLoaded()
        {
            Project sessionProject = ModelHelper.ModelHelper.GetSessionProject(Session);
            if (sessionProject != null)
            {
                ddlPSMSchemaSelector.ProjectVersion = sessionProject.SingleVersion;
                ddlPSMSchemaSelector.InitControl();
                ddlPSMSchemaSelector.DataBind();
            }
        }
    }
}