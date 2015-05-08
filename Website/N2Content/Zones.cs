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

namespace DigitalRiver.CloudLink.Commerce.Nimbus.N2Content
{
	/// <summary>
	/// Provides access to some zone-related constants. Use these constants 
	/// instead of strings for better compile-time checking.
	/// </summary>
	public class Zones
	{
		/// <summary>Right column on whole site.</summary>
		public const string SiteRight = "SiteRight";
		/// <summary>Above content on whole site.</summary>
		public const string SiteTop = "SiteTop";
		/// <summary>Left of content on whole site.</summary>
		public const string SiteLeft = "SiteLeft";

		/// <summary>Header primary navigation</summary>
		public const string Navigation = "Navigation";
		/// <summary>Header primary navigation on this and child pages.</summary>
		public const string RecursiveNavigation = "RecursiveNavigation";

		/// <summary>Top header area</summary>
		public const string Header = "Header";
		/// <summary>Top header area on this and child pages</summary>
		public const string RecursiveHeader = "RecursiveHeader";

        /// <summary>Bottom footer area</summary>
        public const string Footer = "Footer";
        /// <summary>Bottom footer area on this and child pages</summary>
        public const string RecursiveFooter = "RecursiveFooter";

		/// <summary>Site Action, such as, Mini-Cart Count Viewer</summary>
        public const string SiteAction = "SiteAction";
	    public const string HeaderSiteAction = "HeaderSiteAction";
        public const string SiteSearch = "SiteSearch";

		/// <summary>To the right on this and child pages.</summary>
		public const string RecursiveRight = "RecursiveRight";
		/// <summary>To the left on this and child pages.</summary>
		public const string RecursiveLeft = "RecursiveLeft";
		/// <summary>Above the content area on this and child pages.</summary>
		public const string RecursiveAbove = "RecursiveAbove";
		/// <summary>Below the content area on this and child pages.</summary>
		public const string RecursiveBelow = "RecursiveBelow";


		/// <summary>Right on this page.</summary>
		public const string Right = "Right";
		/// <summary>Left on this page</summary>
		public const string Left = "Left";

		/// <summary>On the left side of a two column container</summary>
		public const string ColumnLeft = "ColumnLeft";
		/// <summary>On the right side of a two column container</summary>
		public const string ColumnRight = "ColumnRight";

		/// <summary>In the content column (below text) on this page.</summary>
		public const string Content = "Content";
		/// <summary>In the content column (below text) on this page and child pages</summary>
		public const string RecursiveContent = "RecursiveContent";

		/// <summary> Questions on FAQ pages </summary>
		public const string Questions = "Questions";

        /// <summary>Hero Banner zone</summary>
        public const string HeroBanner = "HeroBanner";
		/// <summary>Extra zone for adding to Hero Banner Title area</summary>
        public const string HeroBannerTitle = "HeroBannerTitle";


		/// <summary></summary>
		public const string BannerTopLeft = "BannerTopLeft";

		/// <summary></summary>
		public const string BannerTopRight = "BannerTopRight";

		/// <summary></summary>
		public const string BannerBottomLeft = "BannerBottomLeft";

		/// <summary></summary>
		public const string BannerBottomRight = "BannerBottomRight";

		/// <summary></summary>
		public const string BannerImage = "BannerImage";
	}
}