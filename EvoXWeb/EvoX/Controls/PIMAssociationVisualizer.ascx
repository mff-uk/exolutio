<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="EvoX.Web.Controls.PIMAssociationVisualizer" Codebehind="PIMAssociationVisualizer.ascx.cs" %>
<%@ Import Namespace="EvoX.Model.PIM" %>
<div class="associationBoundingBox">
    <asp:Literal ID="litAnchor" runat="server"></asp:Literal>
    <div id="w<%=PIMAssociationId%>">
        <div class="associationHeader" onclick="selectComponents(new Array(<%=ParticipantsArray %>))">
            <asp:Label ID="lHeader" Text="Association 1" runat="server" />
        </div>
        <div class="associationContent">
            <div class="associationParticipants">
                <asp:Repeater ID="repeaterParticipants" runat="server" OnItemDataBound="repeaterParticipants_OnItemDataBound">
                    <HeaderTemplate>
                        <ul>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <li>
                            Class:
                            <asp:HyperLink ID="lParticipantName" Text="part" runat="server" />
                            <asp:Label ID="lRole" Text="part" runat="server" />
                            <br />
                            Cardinality:
                            <asp:Label ID="lCardinality" Text="ID" runat="server" />
                        </li>
                    </ItemTemplate>
                    <SeparatorTemplate>
                        <br />
                    </SeparatorTemplate>
                    <FooterTemplate>
                        </ul>
                    </FooterTemplate>
                </asp:Repeater>
            </div>
        </div>
    </div>
</div>
