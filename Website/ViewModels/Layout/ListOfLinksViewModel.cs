using System.Collections.Generic;
using Jungo.Models.ShopperApi.Catalog;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Layout
{
    public class ListofLinksViewModel : PageViewModelBase
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string BackgroundColor { get; set; }
        public string ForegroundColor { get; set; }
        public bool UseButton { get; set; }
        public int TemplateItems { get; set; }
        public IList<ListOfLinksItem> Links { get; set; }
    }

    public class ListOfLinksItem
    {
        public ListOfLinksItem(string tite, string target, string linkText, string targetUrl, bool suppressLinks)
        {
            Title = tite;
            Target = target;
            LinkText = linkText;
            SuppressLinks = suppressLinks;
            TargetUrl = targetUrl;
        }

        public string Title { get; set; }
        public string Target { get; set; }
        public string LinkText { get; set; }
        public bool SuppressLinks { get; set; }
        public string TargetUrl { get; set; }
    }

    public class RichListOfLinksItem : ListOfLinksItem
    {
        public RichListOfLinksItem(string title, string target,
                                            string linkText, string targetUrl, bool suppressLinks,
                                            string tagLine, string text, string image, string htmlContent)
            : base(title, target, linkText, targetUrl, suppressLinks)
        {
            TagLine = tagLine;
            Text = text;
            Image = image;
            HtmlContent = htmlContent;
        }

        public string TagLine { get; set; }
        public string Text { get; set; }

        public string Image { get; set; }
        public string HtmlContent { get; set; }
    }

    public class ProductRichListOfLinksItem : RichListOfLinksItem
    {
        public ProductRichListOfLinksItem(string title, string target, string linkText, string targetUrl, bool suppressLinks, string tagLine, string text, string image, string productId, bool showProductDetail, string htmlContent, Product productDetails)
            : base(title, target, linkText, targetUrl, suppressLinks, tagLine, text, image, htmlContent)
        {
            ProductId = productId;
            ShowProductDetail = showProductDetail;
            ProductDetails = productDetails;
            HtmlContent = htmlContent;
        }

        public string ProductId { get; set; }
        public bool ShowProductDetail { get; set; }
        public new string HtmlContent { get; set; }
        public Product ProductDetails { get; set; }

    }
}
