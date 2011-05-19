<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="EvoX.Web.Controls.PIMClassVisualizer" Codebehind="PIMClassVisualizer.ascx.cs" %>
<%@ Import Namespace="EvoX.Model.PIM" %>
<%@ Import Namespace="EvoX.Model.PSM" %>
<div class="classBoundingBox">
    <asp:Literal ID="litAnchor" runat="server"></asp:Literal>
    <div id="w<%=PIMClassId%>">
        <div class="classHeader">
            <asp:Label ID="lHeader" Text="Class 1" runat="server" />
        </div>
        <div class="classContent">
            Attributes:
            <div class="classProperties">
                <asp:Repeater ID="repeaterAttributes" runat="server">
                    <HeaderTemplate>
                        <ul>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <li id="w<%# ((PIMAttribute) Container.DataItem).ID %>"><asp:Label ID="attributeName" runat="server" Text='<%# FullDisplayAttribute((PIMAttribute) Container.DataItem) %>'></asp:Label></li>
                    </ItemTemplate>
                    <FooterTemplate>
                        </ul>
                    </FooterTemplate>
                </asp:Repeater>
            </div>
            Associations:
            <div class="classAssociations">
                <asp:Repeater ID="repeaterAssociations" runat="server">
                    <HeaderTemplate>
                        <ul>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <li><asp:Label ID="classAssociation" runat="server" Text='<%# FullDisplayClassAssociation((PIMAssociationEnd) Container.DataItem) %>'></asp:Label></li>
                    </ItemTemplate>
                    <FooterTemplate>
                        </ul>
                    </FooterTemplate>
                </asp:Repeater>
            </div>
            <span title="PSM classes that are <%=PIMClassName%>'s interpretation">PSM classes (interpr.):</span>
            <div class="classDerivedClasses">
                <asp:Repeater ID="repeaterDerivedClasses" runat="server">
                    <HeaderTemplate>
                        <ul>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <li><asp:Label ID="classDerivedClass" runat="server" Text='<%# FullDisplayDerivedClass((PSMClass) Container.DataItem) %>'></asp:Label></li>
                    </ItemTemplate>
                    <FooterTemplate>
                        </ul>
                    </FooterTemplate>
                </asp:Repeater>
            </div>
        </div>
    </div>
</div>
