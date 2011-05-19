using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using EvoX.Model;
using EvoX.Model.PSM;

using EvoX.Web.IO;
using EvoX.Web.Controls.TabTemplates;
using EvoX.Web.ModelHelper;

public partial class EvoX_OnlineDemo : System.Web.UI.Page, ISerializedProjectHolder
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack && !IsCallback)
        {
            LoadSampleFiles();
        }

        if (IsPostBack && !String.IsNullOrEmpty(SerializedProject))
        {
            DisplayProject(SerializedProject);
        }

        if (String.IsNullOrEmpty(SerializedProject))
        {
            NoProjectMode();
        }
    }

    public string SerializedProject
    {
        get { return (string)ViewState["SerializedProject"]; }
        set { ViewState["SerializedProject"] = value; }
    }

    public void NoProjectMode()
    {
        tcProject.Tabs[0].Visible = false;
        tcProject.Tabs[1].Visible = true;
                
        tcProject.ActiveTabIndex = 1;
        bEdit.Enabled = false;
        bDownload.Enabled = false;
        ClearSchemaTabs(tcProject);
    }

    public void LoadedProjectMode()
    {
        if (tcProject.Tabs.Count > 0)
        {
            tcProject.ActiveTabIndex = 0;
        }

        bEdit.Enabled = true;
        bDownload.Enabled = true;
        tcProject.Tabs[0].Visible = true;
        tcProject.Tabs[1].Visible = false;        
        operationEditor.OnProjectLoaded();
    }


    private void LoadSampleFiles()
    {
        DirectoryInfo sampleFilesDir = new DirectoryInfo(Server.MapPath("~/Evox/SampleFiles"));
        IEnumerable<FileInfo> sampleFiles = IOHelper.GetAvailableProjectFiles(sampleFilesDir);

        ListItem empty = new ListItem(null, null);
        ddlSampleFiles.Items.Add(empty);

        foreach (FileInfo sampleFile in sampleFiles)
        {
            ListItem item = new ListItem(sampleFile.Name, sampleFile.FullName);
            ddlSampleFiles.Items.Add(item);
        }

    }

    private void LoadInputFile(string selectedValue)
    {
        SerializedProject = IOHelper.GetFileText(selectedValue);
        DisplayProject(SerializedProject, true);
    }

    private void ClearSchemaTabs(TabContainer tabContainer)
    {
        if (tabContainer.Tabs.Count > 2)
        {
            for (int i = tabContainer.Tabs.Count - 1; i >= 2; i--)
            {
                tabContainer.Tabs.RemoveAt(i);
            }   
        }
    }

    public void DisplayProject(string serializedText, bool updateSession = false)
    {
        if (string.IsNullOrEmpty(serializedText))
        {
            NoProjectMode();
            ModelHelper.SetSessionProject(Session, null);
            return;
        }

        Project project = IOHelper.LoadProjectFromString(serializedText);
        
        if (updateSession)
        {
            ModelHelper.SetSessionProject(Session, project);
            operationEditor.ClearResult();
        }

        ProjectVersion projectVersion = project.UsesVersioning ? project.ProjectVersions.Last() : project.SingleVersion;

        //PIMVisualizer1.Display(projectVersion.PIMSchema);
        //PSMVisualizer1.Display(projectVersion.PSMSchemas[2]);

        ClearSchemaTabs(tcProject);

        {
            TabPanel xmlTabPanel = new TabPanel();
            xmlTabPanel.HeaderTemplate = new TabPanelHeaderTemplate("Serialized project");
            xmlTabPanel.ContentTemplate = new DisplayXmlTemplate(serializedText);
            tcProject.Tabs.Add(xmlTabPanel);
        }

        {
            // pim schema
            TabPanel pimTabPanel = new TabPanel();
            pimTabPanel.HeaderTemplate = new TabPanelHeaderTemplate("PIM");
            pimTabPanel.ContentTemplate = new TabPanelPIMTemplate(this, projectVersion.PIMSchema);
            tcProject.Tabs.Add(pimTabPanel);
        }

        // psm schemas
        foreach (PSMSchema psmSchema in projectVersion.PSMSchemas)
        {
            TabPanel psmTabPanel = new TabPanel();
            psmTabPanel.HeaderTemplate = new TabPanelHeaderTemplate(psmSchema.Caption);
            psmTabPanel.ContentTemplate = new TabPanelPSMTemplate(this, psmSchema);
            tcProject.Tabs.Add(psmTabPanel);
        }

        LoadedProjectMode();
        GoToDisplayMode();
        operationEditor.UpdateForNewProject(project);
    }

    protected void bLoadProjectFromFile_OnClick(object sender, EventArgs e)
    {
        if (ddlSampleFiles.SelectedValue != null)
        {
            LoadInputFile(ddlSampleFiles.SelectedValue);
        }
    }

    protected void bEdit_OnClick(object sender, EventArgs e)
    {
        GoToEditMode();
    }

    private void GoToEditMode()
    {
        if (tcProject.Tabs.Count > 0)
        {
            tcProject.Tabs[0].Visible = true;
            tcProject.ActiveTabIndex = 0;
        }

        ClearSchemaTabs(tcProject);
        tbEditSerializedProject.Text = SerializedProject;

        bEdit.Visible = false;
        bSave.Visible = true;
        bCancel.Visible = true;
    }

    private void GoToDisplayMode()
    {
        bEdit.Visible = true;
        bSave.Visible = false;
        bCancel.Visible = false;

        tcProject.Tabs[0].Visible = false;
        tcProject.ActiveTabIndex = 3;
    }

    protected void bSave_OnClick(object sender, EventArgs e)
    {
        SerializedProject = tbEditSerializedProject.Text;
        DisplayProject(SerializedProject, true);
        tbEditSerializedProject.Text = null;
        GoToDisplayMode();
    }

    protected void bCancel_OnClick(object sender, EventArgs e)
    {
        tbEditSerializedProject.Text = null;
        GoToDisplayMode();
    }

    protected void UploadButton_Click(object sender, EventArgs e)
    {   
        if (fUpload.HasFile)
        {
            try
            {
                string filename = Path.GetFileName(fUpload.FileName);
                StreamReader sr;
                if (filename.ToUpper().EndsWith("XCASE"))
                {
                    this.SerializedProject = IOHelper.LoadXCaseProjectFromStream(fUpload.FileContent);
                }
                else
                {
                    sr = new StreamReader(fUpload.FileContent);
                    this.SerializedProject = sr.ReadToEnd();
                    sr.Close();
                    sr.Dispose();
                }
                
                DisplayProject(SerializedProject);
                //StatusLabel.Text = "Upload status: File uploaded!";
            }
            catch (Exception)
            {
                //StatusLabel.Text = "Upload status: The file could not be uploaded. The following error occured: " + ex.Message;
            }
        }

    }

    protected void bDownload_OnClick(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(SerializedProject))
        {
            Response.Clear();
            Response.AppendHeader("Content-Disposition", "attachment; filename=web-project.evox");
            Response.ContentEncoding = Encoding.UTF8;
            Response.ContentType = "application/download";
            Response.Charset = "utf-8";
            Response.BufferOutput = true;

            StreamWriter sw = new StreamWriter(Response.OutputStream, Encoding.UTF8);
            sw.Write(SerializedProject);
            sw.Flush();
            sw.Close();
            sw.Dispose();
            // Send the output to the client.
            Response.Flush();
            Response.Close();
            Response.End();
        }
    }

    protected void operationEditor_OnOperationExecuted()
    {
        
    }
}