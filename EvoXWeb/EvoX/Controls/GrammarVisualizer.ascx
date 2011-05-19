<%@ Control Language="C#" AutoEventWireup="true" 
    Inherits="EvoX.Web.Controls.GrammarVisualizer" Codebehind="GrammarVisualizer.ascx.cs" %>

<asp:Label ID="Label1" runat="server" Text="Grammar"></asp:Label><br />
<asp:Repeater ID="repTerminals" runat="server" OnItemDataBound="Terminal_ItemDataBound">
<HeaderTemplate>
    <strong>Terminals: </strong><br />
</HeaderTemplate>
<ItemTemplate>
    <asp:HyperLink runat="server" Text="" ID="lblTerminal"/>
</ItemTemplate>
</asp:Repeater>

<br /><br />

<asp:Repeater ID="repNonTerminals" runat="server" OnItemDataBound="NonTerminal_ItemDataBound">
<HeaderTemplate>
    <strong>Non-terminals: </strong><br />
</HeaderTemplate>
<ItemTemplate>
    <asp:HyperLink runat="server" Text="" ID="lblNonTerminal"/>
</ItemTemplate>
</asp:Repeater>

<br /><br />

<asp:Repeater ID="repInitialNonTerminals" runat="server" OnItemDataBound="NonTerminal_ItemDataBound">
<HeaderTemplate>
    <strong>Initial non-terminals: </strong><br />
</HeaderTemplate>
<ItemTemplate>
    <asp:HyperLink runat="server" Text="" ID="lblNonTerminal"/>
</ItemTemplate>
</asp:Repeater>

<br /><br />

<asp:Repeater ID="repProductionRules" runat="server" OnItemDataBound="ProductionRule_ItemDataBound">
<HeaderTemplate>
    <strong>Production rules: </strong><br />
</HeaderTemplate>
<ItemTemplate>
    <asp:Label runat="server" Text="" ID="lblProductionRule"/>
</ItemTemplate>
<SeparatorTemplate>
    <br />
</SeparatorTemplate>
</asp:Repeater>


