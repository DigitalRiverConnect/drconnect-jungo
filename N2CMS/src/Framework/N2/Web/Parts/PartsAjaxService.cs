using System.Collections.Specialized;
using System.Security.Principal;
using System.Text;
using System.Web;
using N2.Engine;

namespace N2.Web.Parts
{
    /// <summary>
    /// Ajax service that adds itself to the ajax request dispatcher upon start.
    /// Used by interactive editor (see parts.js)
    /// </summary>
    public abstract class PartsAjaxService : IAjaxService
    {
        protected Logger<PartsAjaxService> Logger;

        public abstract string Name { get; }

        public bool RequiresEditAccess
        {
            get { return true; }
        }

        public bool IsValidHttpMethod(string httpMethod)
        {
            return httpMethod == "POST";
        }

        protected IPrincipal Principal { get; private set; }

        public void Handle(HttpContextBase context)
        {
            Principal = context.User;

            Logger.DebugFormat("AJAX {0} > {1} ", GetType().Name, ToJson(context.Request.Form, true));

            NameValueCollection response = HandleRequest(context.Request.Form);
            context.Response.ContentType = "application/json";
            var result = ToJson(response);
            context.Response.Write(result);

            Logger.DebugFormat("AJAX {0} : {1} => {2}", GetType().Name, ToJson(context.Request.Form, true), result);
        }

        protected string ToJson(NameValueCollection response, bool compact = false)
        {
            var sb = new StringBuilder();
            using (new Persistence.NH.Finder.StringWrapper(sb, "{", "}"))
            {
                if (!compact)
                    sb.AppendFormat(@"""{0}"": ""{1}""", "error", "false");

                foreach (string key in response.Keys)
                {
                    if (!compact || !string.IsNullOrEmpty(response[key]))
                        sb.AppendFormat(@", ""{0}"": ""{1}""", key, response[key]);
                }
            }
            return sb.ToString();
        }

        public abstract NameValueCollection HandleRequest(NameValueCollection request);
    }
}
