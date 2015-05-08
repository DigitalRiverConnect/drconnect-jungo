using DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Services;
using N2;
using N2.Engine;
using N2.Persistence;
using N2.Web;
using N2.Web.UI;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Infrastructure
{
    [Service(typeof (IFlushable))]
    class WebCachesFlushable : IFlushable
    {
        public void Flush()
        {           
            var urlParser = Context.Current.Resolve<IUrlParser>() as IFlushable;
            if (urlParser != null)
            {
                urlParser.Flush();
            }

            var linkGenerator = Context.Current.Resolve<ILinkGenerator>();
            ((IFlushable)linkGenerator).Flush();
            var cm = Context.Current.Resolve<ICacheManager>();
            if (cm.Enabled)
            {
                Logger.Debug("WebCachesFlushable flushing output cache");
                var cp = Context.Current.Persister as ContentPersister;
                if (cp != null)
                {
                    (cp as IFlushable).Flush();
                    Logger.Debug("WebCachesFlushable event");
                }
            }
        }
    }
}