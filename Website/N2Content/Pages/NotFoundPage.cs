//
// Copyright (c) 2012 by Digital River, Inc. All rights reserved.
// 

using N2;
using N2.Installation;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages
{
    [PageDefinition("Not Found Page", InstallerVisibility = InstallerHint.NeverRootOrStartPage,
        Description = "This is the page that the shopper is sent to when a CMS page could not be found.",
        IconUrl = "~/Content/img/icons/warning.png")]
    public class NotFoundPage : ContentPage
    {
    }
}
