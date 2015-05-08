//
// Copyright (c) 2013 by Digital River, Inc. All rights reserved.
// 

using N2;
using N2.Details;
using N2.Installation;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content.Pages
{
    [PageDefinition("Contact Us Page",
    Description = "Contact Us Page",
    SortOrder = 310,
    InstallerVisibility = InstallerHint.NeverRootOrStartPage,
    IconUrl = "~/Content/img/icons/phone.jpg")]
    [WithEditableName]
    public class ContactUsPage : ContentPage
    {
    }
}
