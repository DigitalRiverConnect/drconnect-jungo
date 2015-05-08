using System;
using System.Net;

namespace Jungo.Models.ShopperApi.Common
{
    public class ShopperApiException : Exception
    {
        public override string Message
        {
            get
            {
                return String.Format("ShopperApiException: {0} {1}, Description: {2}, Uri: {3}", (int)HttpStatusCode, HttpStatusCode,
                    ShopperApiError.Description, Uri);
            }
        }

        public string Uri { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
        public Error ShopperApiError { get; set; }
    }
}
