<%@ Control Language="C#" AutoEventWireup="true" Inherits="EvoX.Web.Controls.OperationEditor" Codebehind="OperationEditor.ascx.cs" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<%@ Register TagPrefix="evox" Namespace="EvoX.Web.Controls" Assembly="EvoX.EvoXWeb" %>
<%@ Register TagPrefix="evox" Src="~/EvoX/Controls/GrammarVisualizer.ascx" TagName="GrammarVisualizer"  %>
<%@ Register TagPrefix="evoxx" Namespace="EvoX.Web.OperationParameters" Assembly="EvoX.EvoXWeb" %>


<asp:UpdatePanel ID="UpdatePanel1" runat="server" >
    <ContentTemplate>                 
            <%--<asp:ComboBox ID="ddlAvailableOperations" runat="server" 
                    AutoPostBack="True" 
                    DropDownStyle="Simple" 
                    AutoCompleteMode="SuggestAppend" 
                    CaseSensitive="False" 
                    ItemInsertLocation="Append"
                    Width="245px"
                    OnSelectedIndexChanged="ddlAvailableOperations_OnSelectedIndexChanged" />--%>
            <asp:DropDownList ID="ddlAvailableOperations" runat="server" Width="245px"  AutoPostBack="true" OnSelectedIndexChanged="ddlAvailableOperations_OnSelectedIndexChanged" >
                <asp:ListItem Text="Choose operation..." Value="nothing" />
            </asp:DropDownList>

            <asp:ListSearchExtender id="LSE" runat="server"
                TargetControlID="ddlAvailableOperations"
                PromptText="Type to search"
                PromptCssClass="ListSearchExtenderPrompt"
                QueryPattern="Contains"

                PromptPosition="Top"                                
                QueryTimeout="0"
                IsSorted="False"/> 

            <%--<asp:TextBox ID="tbAvailableOperations" runat="server" />--%>
            <%--<asp:AutoCompleteExtender ID="AutoCompleteExtender1" runat="server" 
                TargetControlID="tbAvailableOperations" ServiceMethod="GetOperationsList" ServicePath="~/EvoX/EvoXWS.asmx"
                MinimumPrefixLength="2" 
                CompletionInterval="500"
                EnableCaching="true"
                CompletionSetCount="20" >
            </asp:AutoCompleteExtender>--%>


            <evox:EvoXClearablePanel ID="panelParams" runat="server" />
            <asp:Button ID="bExecute" runat="server" Text="Execute" OnClick="bExecute_OnClick" />
            <asp:Button ID="bUndo" runat="server" Text="Undo" OnClick="bUndo_OnClick" />
            <asp:Button ID="bRedo" runat="server" Text="Redo" OnClick="bRedo_OnClick" />
            <br />
            <br />
            Or select a schema: <evoxx:PSMSchemaLookup runat="server" ID="ddlPSMSchemaSelector"></evoxx:PSMSchemaLookup> and
            <asp:Button ID="bTestNormalization" runat="server" 
                Text="Test whether schema is normalized" onclick="bTestNormalization_Click" />
            <asp:Button ID="bNormalizeSchemas" runat="server" 
                Text="Normalize schema" onclick="bNormalizeSchema_Click" />
            <asp:Button ID="bGenerateRTG" runat="server" 
                Text="Generate regular tree grammar" onclick="bGenerateRTG_Click" />
            <br />
            <br />
            <asp:Label ID="lCommandResult" runat="server" Text="" Visible="false"></asp:Label>
            <evox:CommandReportDisplay ID="reportDisplay" runat="server" />
            <evox:GrammarVisualizer ID="grammarVisualizer" runat="server" Visible="false" />



    </ContentTemplate>        
    <Triggers>
        <asp:PostBackTrigger ControlID="bExecute" />
        <asp:PostBackTrigger ControlID="bUndo" />
        <asp:PostBackTrigger ControlID="bRedo" />
    </Triggers>
</asp:UpdatePanel>
        
