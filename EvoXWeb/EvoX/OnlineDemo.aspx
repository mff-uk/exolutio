<%@ Page Title="Evox > Online Demo" Language="C#" AutoEventWireup="true"
    Inherits="EvoX_OnlineDemo" MasterPageFile="~/Main.master" ValidateRequest="false" Codebehind="OnlineDemo.aspx.cs" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Register TagPrefix="ajaxToolkit" Namespace="AjaxControlToolkit" Assembly="EvoX.EvoXWeb" %>
<%@ Register TagPrefix="CH" Namespace="ActiproSoftware.CodeHighlighter" Assembly="ActiproSoftware.CodeHighlighter.Net20" %>
<%@ Register TagPrefix="evox" Src="~/EvoX/Controls/PIMVisualizer.ascx" TagName="PIMVisualizer"  %>
<%@ Register TagPrefix="evox" Src="~/EvoX/Controls/PSMVisualizer.ascx" TagName="PSMVisualizer"  %>
<%@ Register TagPrefix="evox" Src="~/EvoX/Controls/OperationEditor.ascx" TagName="OperationEditor"  %>



<asp:Content ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="Stylesheet" href="../Styles/OnlineDemo.css" runat="server" />
    <script language="JavaScript" src="../Scripts/OnlineDemo.js" type="text/javascript"></script>
</asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" />
    <p>
        Select one of the sample files:
        <asp:DropDownList ID="ddlSampleFiles" runat="server" Width="206px"></asp:DropDownList>
        <asp:Button ID="bLoadProjectFromFile" Text="Load" OnClick="bLoadProjectFromFile_OnClick" runat="server"/>
        or upload your own: 
        <asp:FileUpload ID="fUpload" runat="server" />
        <asp:Button runat="server" id="bUpload" text="Upload" onclick="UploadButton_Click" />
    </p>
    <div>
        <%--<evox:PIMVisualizer ID="PIMVisualizer1" runat="server"></evox:PIMVisualizer>--%>
        <%--<evox:PSMVisualizer ID="PSMVisualizer1" runat="server"></evox:PSMVisualizer>--%>
        
        <evox:OperationEditor ID="operationEditor" runat="server" OnOperationExecuted="operationEditor_OnOperationExecuted" />    
        
        <asp:Button runat="server" ID="bEdit" Text="Edit" OnClick="bEdit_OnClick"/>
        <asp:Button runat="server" ID="bSave" Text="Save" Visible="false" OnClick="bSave_OnClick"/>
        <asp:Button runat="server" ID="bCancel" Text="Cancel" Visible="false" OnClick="bCancel_OnClick"/>
        <asp:Button runat="server" ID="bDownload" Text="Download project" OnClick="bDownload_OnClick" />
        <ajax:TabContainer ID="tcProject" runat="server" ActiveTabIndex="2" >
            <ajax:TabPanel runat="server" HeaderText="Project editor" ID="tpProjectEditor" Visible="true">
                <ContentTemplate>
                    <asp:TextBox ID="tbEditSerializedProject" runat="server" CssClass="inputArea" TextMode="MultiLine" />
                </ContentTemplate>
            </ajax:TabPanel>
            <ajax:TabPanel runat="server" HeaderText="Project" ID="tpNoProjectLoaded">
                <ContentTemplate>
                    No project loaded. 
                    
                </ContentTemplate>
            </ajax:TabPanel>
        </ajax:TabContainer>
        
    </div>
</asp:Content>
