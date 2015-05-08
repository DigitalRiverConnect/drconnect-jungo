//
// Copyright (c) 2012 by Digital River, Inc. All rights reserved.
// Last Modified: $Date: $
// Modified by: $Author: $
// Revision: $Revision: $
//
//  History:
//
//  Date        Developer      Description
//  ----------  -------------  ---------------------------------------------------------
//  12/13/2012  HGodinez           Created
// 

namespace DigitalRiver.CloudLink.Commerce.Nimbus.SportsUs.Controllers.Parts
{
#if ENABLE_API_POPS

using System.Web.Mvc;
using DigitalRiver.CloudLink.Commerce.Web.Utils;
using N2.Web;

namespace DigitalRiver.CloudLink.Commerce.Nimbus.Mvc.Controllers.Parts
{
    [Controls(typeof(PopPart))]
    public class PopController : ViewTemplateController<PopPart>
    {
         public override ActionResult Index()
         {
             var attrMap = CurrentItem.Mapping.ToDictionary(m => m.Source, m => m.Target);

             // TODO handle no data
             var offers = ViewModelUtils.GetPromotion(CurrentItem.POP, attrMap);
             offers.DisplayName = CurrentItem.Title;
             offers.Values = attrMap;

             ViewBag.ComponentID = "item_" + CurrentItem.ID; // unique id for DOM element / CSS
             ViewBag.ClassAttr = offers.Value("Class");

             return TemplateView("PopTemplates", CurrentItem.ViewTemplate, offers);
         }
    }
}

#endif
}