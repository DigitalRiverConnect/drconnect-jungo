<%@ Page MasterPageFile="Framed.Master" Language="C#" AutoEventWireup="true" CodeBehind="Edit.aspx.cs" Inherits="N2.Edit.Edit" Title="Edit" meta:resourcekey="PageResource1" EnableEventValidation="false" %>
<%@ Register TagPrefix="edit" Namespace="N2.Edit.Web.UI.Controls" Assembly="N2.Management" %>
<%@ Register Src="AvailableZones.ascx" TagName="AvailableZones" TagPrefix="uc1" %>
<%@ Register Src="ItemInfo.ascx" TagName="ItemInfo" TagPrefix="uc1" %>
<%@ Register Src="Versions/RecentVersions.ascx" TagName="RecentVersions" TagPrefix="uc1" %>
<%@ Register Src="Activity/RecentActivity.ascx" TagName="RecentActivity" TagPrefix="uc1" %>
<asp:Content ID="ch" ContentPlaceHolderID="Head" runat="server">
</asp:Content>
<asp:Content ID="ct" ContentPlaceHolderID="Toolbar" runat="server">
    <div class="rightAligned">
        <%--onclick="$(document.body).toggleClass('rightExpanded');"--%>
        <asp:PlaceHolder runat="server" ID="phPluginArea" />
    </div>
    <n2:OptionsMenu ID="om" runat="server">
        <asp:LinkButton ID="btnPreview" data-icon-class="n2-icon-eye-open" OnCommand="OnPreviewCommand" runat="server" CssClass="command plain iconed preview" meta:resourceKey="btnPreview">Save and preview</asp:LinkButton>
        <asp:LinkButton ID="btnSavePublish" data-icon-class="n2-icon-play-sign" OnCommand="OnPublishCommand" runat="server" CssClass="command iconed publish" meta:resourceKey="btnSave">Save and publish</asp:LinkButton>
        <asp:LinkButton ID="btnSaveUnpublished" data-icon-class="n2-icon-save" OnCommand="OnSaveUnpublishedCommand" runat="server" CssClass="command plain iconed save" meta:resourceKey="btnSaveUnpublished">Save an unpublished version</asp:LinkButton>
        <asp:HyperLink ID="hlFuturePublish" data-icon-class="n2-icon-time" NavigateUrl="#futurePanel" CssClass="command plain iconed future hidden-action" runat="server" meta:resourceKey="hlSavePublishInFuture">Save and publish version in future</asp:HyperLink>
        <asp:LinkButton ID="btnUnpublish" data-icon-class="n2-icon-stop" OnCommand="OnUnpublishCommand" runat="server" CssClass="command plain iconed unpublish action" meta:resourceKey="btnUnpublish">Unpublish</asp:LinkButton>
    </n2:OptionsMenu>
    <asp:HyperLink ID="hlCancel" runat="server" CssClass="cancel command" meta:resourceKey="hlCancel">Cancel</asp:HyperLink>
</asp:Content>
<asp:Content ID="co" ContentPlaceHolderID="Outside" runat="server">
    <uc1:ItemInfo ID="ucInfo" runat="server" />
    <uc1:RecentVersions ID="ucVersions" runat="server" />
    <uc1:RecentActivity ID="ucActivity" runat="server" />
    <asp:PlaceHolder runat="server" ID="phSidebar" />
    <uc1:AvailableZones ID="ucZones" runat="server" />
</asp:Content>
<asp:Content ID="cc" ContentPlaceHolderID="Content" runat="server">
    <a href="javascript:void(0);" class="rightOpener"><span class='rightOpener-open'>&laquo;</span><span class='rightOpener-close'>&raquo;</span></a>
    <edit:PermissionPanel ID="ppPermitted" RequiredPermission="Write" runat="server" meta:resourceKey="ppPermitted">
        <asp:HyperLink ID="hlNewerVersion" runat="server" Text="There is a newer unpublished version of this page." CssClass="alert alert-margin" Visible="False" meta:resourcekey="hlNewerVersionResource1" />
        <asp:HyperLink ID="hlOlderVersion" runat="server" Text="This is a version of another item." CssClass="alert alert-info alert-margin" Visible="False" meta:resourcekey="hlOlderVersionResource1" />
        <asp:ValidationSummary ID="vsEdit" runat="server" CssClass="alert alert-block alert-margin" HeaderText="The item couldn't be saved. Please look at the following:" meta:resourceKey="vsEdit" />
        <asp:CustomValidator ID="cvException" runat="server" Display="None" />

        <div id="futurePanel" class="popup">
            <n2:DatePicker Label-Text="When" ID="dpFuturePublishDate" runat="server" meta:resourceKey="dpFuturePublishDate" />
            <asp:Button ID="btnSavePublishInFuture" Text="OK" OnCommand="OnSaveFuturePublishCommand" CssClass="ok" runat="server" meta:resourceKey="btnSavePublishInFuture" />
            <asp:HyperLink ID="hlCancelSavePublishInFuture" NavigateUrl="javascript:void(0);" runat="server" CssClass="cancel" meta:resourceKey="hlCancelSavePublishInFuture">Cancel</asp:HyperLink>
        </div>

        <n2:ItemEditor ID="ie" runat="server" />
    </edit:PermissionPanel>

    <script type="text/javascript">
        $(document).ready(function () {
            // future publish
            $("#futurePanel").hide().click(function (e) { e.stopPropagation(); return false; });
            $(".future").click(function (e) {
                $("#futurePanel").css({ left: e.clientX + "px", top: e.clientY + "px" }).show();
                $("#futurePanel input:first").focus();
                e.preventDefault();
                e.stopPropagation();
            });

            $("#futurePanel .cancel").click(function () {
                $("#futurePanel").hide();
            });
            $(document.body).click(function (e) {
                if ($(e.target).closest(".jCalendar").length == 0)
                    $("#futurePanel").hide();
            });

            // zones
            $(".showZones").toggle(function () {
                n2toggle.show(this, ".zonesBox");
            }, function () {
                n2toggle.hide(this, ".zonesBox");
            });

            // info
            if ($.cookie(".infoBox"))
                $(".showInfo").click();
            if ($.cookie(".zonesBox"))
                $(".showZones").click();

            $(".help-tooltip").tooltip({});
            $(".help-popover").each(function () {
                var title = $(this).attr("title");
                var content = $(this).attr("data-content");
                $(this).attr("title", "");
                $(this).tooltip({ html: true, title: "<h6>" + title + "</h6><p>" + content + "</p>" });
            });
        });

		</script>
</asp:Content>
