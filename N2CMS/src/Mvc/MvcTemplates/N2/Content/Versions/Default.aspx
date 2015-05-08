<%@ Page MasterPageFile="..\Framed.Master" Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="N2.Edit.Versions.Default" Title="Previous Versions" %>
<%@ Register TagPrefix="edit" Namespace="N2.Edit.Web.UI.Controls" Assembly="N2.Management" %>
<asp:Content ID="ContentToolbar" ContentPlaceHolderID="Toolbar" runat="server">
	<edit:CancelLink ID="hlCancel" runat="server" meta:resourceKey="hlCancel">Cancel</edit:CancelLink>
</asp:Content>
<asp:Content ID="ContentContent" ContentPlaceHolderID="Content" runat="server">
	<asp:CustomValidator ID="cvVersionable" runat="server" Text="This item is not versionable." CssClass="alert alert-margin" meta:resourceKey="cvVersionable" Display="Dynamic" />
	<edit:PermissionPanel id="ppPermitted" runat="server" meta:resourceKey="ppPermitted">

	<asp:GridView ID="gvHistory" runat="server" AutoGenerateColumns="false" DataKeyNames="VersionIndex" CssClass="table table-striped table-hover table-condensed" UseAccessibleHeader="true" BorderWidth="0" OnRowCommand="gvHistory_RowCommand" OnRowDeleting="gvHistory_RowDeleting">
		<Columns>
			<asp:TemplateField HeaderText="Version" meta:resourceKey="v" ItemStyle-CssClass="Version">
				<ItemTemplate>
					<%# IsPublished(Eval("Content")) ? "<img src='../../Resources/icons/bullet_green.png' alt='published' />" : string.Empty%>
					<%# IsFuturePublished(Eval("Content")) ? "<img src='../../Resources/icons/clock.png' />" : ""%>
					<span title='<%# Eval("State") %>'><%# ((N2.ContentItem)Eval("Content")).VersionIndex%></span>
				</ItemTemplate>
			</asp:TemplateField>
			<asp:TemplateField HeaderText="Title" meta:resourceKey="title" >
				<ItemTemplate><a href="<%# Eval("Content.Url") %>" title="<%# Eval("ID") %>"><img alt="icon" src='<%# ResolveUrl((string)Eval("IconUrl")) %>'/><%# string.IsNullOrEmpty(((N2.ContentItem)Eval("Content")).Title) ? "(untitled)" : ((N2.ContentItem)Eval("Content")).Title%></a></ItemTemplate>
			</asp:TemplateField>
			<asp:TemplateField HeaderText="State" meta:resourceKey="state">
				<ItemTemplate>
				    <a target="_blank" href="<%# VersionUrl(Eval("Content"))%>">
					<asp:Literal runat="server" Text='<%# GetLocalResourceString("ContentState." + Eval("State"), Eval("State").ToString()) %>' /></a>
				</ItemTemplate>
			</asp:TemplateField>
            <asp:TemplateField HeaderText="Published" meta:resourceKey="published" >
				<ItemTemplate><%# PublishString(Eval("Content")) %></ItemTemplate>
			</asp:TemplateField>
			<asp:BoundField HeaderText="Expired" DataField="Expires" meta:resourceKey="expires" />
			<asp:BoundField HeaderText="Last updated" DataField="Updated" />
			<asp:TemplateField HeaderText="Saved by" meta:resourceKey="savedBy" >
				<ItemTemplate><img alt="icon" src='<%# ResolveUrl((string)Eval("UserIconUrl")) %>'/><%# Eval("SavedBy")%></ItemTemplate>
			</asp:TemplateField>
			<asp:TemplateField>
				<ItemTemplate>
					<asp:HyperLink runat="server" ID="hlEdit" meta:resourceKey="hlEdit" Text="Edit" NavigateUrl='<%# Engine.ManagementPaths.GetEditExistingItemUrl((N2.ContentItem)Eval("Content")) %>' Visible='<%# CanEdit(Eval("Content")) %>' />
				</ItemTemplate>
			</asp:TemplateField>
			<asp:TemplateField>
				<ItemTemplate>
					<asp:LinkButton runat="server" ID="btnPublish" meta:resourceKey="btnPublish" Text="Publish" CommandName="Publish" CommandArgument='<%# Eval("VersionIndex") %>' Visible='<%# CanPublish(Eval("Content")) %>' />
				</ItemTemplate>
			</asp:TemplateField>
			<asp:TemplateField>
				<ItemTemplate>
					<asp:LinkButton runat="server" ID="btnDelete" meta:resourceKey="btnDelete" Text="Delete" CommandName="Delete" CommandArgument='<%# Eval("VersionIndex") %>' Visible='<%# CanDeleteVersion(Eval("Content")) %>'
						OnClientClick="return confirm('Are you sure you want to Delete this version?');" />
				</ItemTemplate>
			</asp:TemplateField>
		</Columns>
	</asp:GridView>
	</edit:PermissionPanel>
</asp:Content>
