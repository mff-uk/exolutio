<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="EvoX.Web.Controls.PIMVisualizer" Codebehind="PIMVisualizer.ascx.cs" %>
<%@ Register TagPrefix="evox" Src="~/EvoX/Controls/PIMClassVisualizer.ascx" TagName="PIMClassVisualizer" %>
<%@ Register TagPrefix="evox" Src="~/EvoX/Controls/PIMAssociationVisualizer.ascx" TagName="PIMAssociationVisualizer" %>

<h2>PIM Classes: </h2>

<asp:Repeater ID="repeaterPIMClasses" runat="server" OnItemDataBound="repeaterPIMClasses_OnItemDataBound">
    <ItemTemplate>
        <evox:PIMClassVisualizer ID="pimClassVisualizer" runat="server" />
    </ItemTemplate>
</asp:Repeater>

<br style="clear: both" />

<h2>PIM Associations: </h2>

<asp:Repeater ID="repeaterPIMAssociations" runat="server" OnItemDataBound="repeaterPIMAssociations_OnItemDataBound">
    <ItemTemplate>
        <evox:PIMAssociationVisualizer ID="pimAssociationVisualizer" runat="server" />
    </ItemTemplate>
</asp:Repeater>

<br style="clear: both" />