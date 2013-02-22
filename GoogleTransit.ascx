<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="GoogleTransit.ascx.cs" Inherits="GoogleTransit.GoogleTransit" %>
<%@ Register Namespace="umbraco" TagPrefix="umb" Assembly="umbraco" %>
<%@ Register Namespace="umbraco.uicontrols" TagPrefix="cc1" Assembly="controls" %>

<asp:Panel ID="Content1" runat="server">
    <cc1:PropertyPanel ID="pp1" Width="400px" runat="server">
        <p><b>Feed Status:</b></p>
        <asp:Literal ID="status" runat="server"></asp:Literal>
        <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Publish Feed!" /><br /><br />
        <asp:Literal ID="message" runat="server"></asp:Literal>
    </cc1:PropertyPanel>
</asp:Panel>