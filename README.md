# drconnect-jungo
DR Connect ASP.Net MVC reference architecture

##Welcome to Digital River's .NET N2CMS Connector for Global Commerce

This application was developed to provide a .NET connector between the open source CMS application N2CMS and Digital River's Global Commerce system using Digital River's RESTful Shopper APIs.  The goal of this application is to provide sample code for integrating a .NET based CMS with Digital River's Global Commerce backend.

The following scenarios are highlighted through the integration with Shopper API:
* Display offer content intended for product merchandising
* Display discounted pricing based on offer driven pricing
* Display partial and full product throughout an online store
* Display a list of products within a category
* Search a product catalog
* Display all product variations available for purchase
* Utilize non-standard product attributes for image display on product detail pages
* Display product cross sell opportunities
* Add/Remove products from a shopping cart
* Apply coupon driven discounts to a shopping cart
* Integrate shopping cart with commerce checkout flow hosted by Digital River

Please use this as a starting point of how you could build your own commerce driven application.  In order to gain Shopper API access, a Digital River Global Commerce store is required.  For more details on Digital River and Global Commerce go to: http://www.digitalriver.com/ and http://www.digitalriver.com/solutions/commerce/global-commerce/

##N2CMS
N2CMS is a lightweight CMS framework that we have leveraged to show a proof of concept of how a CMS based system can be integrated with the Global Commerce Shopper APIs to create a commerce driven CMS storefront.

For more information on N2CMS, please visit: http://n2cms.com/
Detailed installation instructions are available at: https://github.com/n2cms/n2cms/blob/master/README_SETUP.md
or in our documentation wiki: https://n2cmsdocs.atlassian.net/wiki/display/N2CMS/Getting+Started+using+N2CMS

##Global Commerce
Our full-service, enterprise-ready, cloud-based e-commerce solution supports any business model.  Digital River Global Commerce offers you our most powerful cloud commerce tools and services for selling digital and physical products across global online markets. Grow sales and expand into some of the fastest-growing online markets worldwide, while we handle the details specific to each country including taxes, regulations, payments, e-marketing and more. End-to-end, you’ll have everything you need to quickly and easily serve your customers - no matter how big your program, how global your business, or how unique your business model.  For more details on Global Commerce go to: http://www.digitalriver.com/solutions/commerce/global-commerce/

##Global Commerce Shopper APIs
Shopper APIs are the REST based integration points offered for Global Commerce.  These APIs are built for building dynamic commerce driven storefronts.  It all starts with “shoppers/me”. All calls made with “shoppers/me” work in the context of the current shopper as defined by your OAuth token. The APIs are shopper-aware and you can customize a shopping experience accordingly. Personalization is built-in to the foundation of the Shopper API. The DR Connect Shopper APIs operate on a store and products that are configured in Global Commerce.

Get more detail at: https://developers.digitalriver.com/
In order to gain access, please register at https://developers.digitalriver.com/user/register

##Getting Started
1. Go to $jungo/Config
2. Copy the environment.txt.sample and rename the copy to environment.txt
3. Open environment.txt and set the name of your environment. The name of the environment file indicates which folder will contain the environment overrides that will override the default settings. For example, Cloud.Dev will look in $jungo/Config/Cloud/Dev/ for config override files.
4. Copy the following files from $jungo/Config into $jungo/Config/<environment config folder>: CryptographicService.json, CryptographicServiceThumbprint.json, ExternalWebLink.json, ShopperApiKey.json, ShopperApiUri.json, Site.json
5. Generate an X509 RSA certificate and add it to your computer's certificate store under Personal/Certificates. Give permissions to NetworkServices to allow it to read the certificate.
6. Extract the Certificate's thumbprint.
7. Go to $jungo/Config/<environment config folder> and open CryptographicServiceThumbprint.json. Store the thumbprint here.
8. Generate an AES key, and AES IV and a Salt 
	a. Generate a 256-bit key. You can create one using RijndaelManaged.GenerateKey()
	b. Generate an IV. You can use RijndaelManaged.GenerateIV()
	c. Generate a Salt. It must be at least 8 bytes long.
	d. Encrypt each of these values using the X509 RSA certificate using PKCS#1 v1.5 padding and apply Base64 encoding.
	e. Store the Base64 encoded values in the $jungo/Config/<environment config folder>/CryptographicServiceThumbprint.json file.
9.  Open ExternalWebLink.json and set the public image directories here. "PublicUrl" is a cloud storage URL that is used to store your uploads via N2CMS. Currently it's set to use Azure Cloud storage, but you can use other storage methods if you wish. "ProductImageUrl" is the URL to your product's images on Digital River's Image CDN.
10. Open ShopperAPIKey.json and put in your shopper API key here.
11. Open ShopperApiUri.json and under sessionTokenUri, enter the URI that Digital River supplied to you that allows you to generate a session Token. 
12. Open up Site.json and enter your Digital River site specific information. If you are supporting more than one site, then you may add more than one site configuration.
13. Open the $jungo/Website/Web/Jungo folder and double click on the linksrc.bat batch file. This will generate a link to the n2cms administration directory which is needed to administer the site.
14. Open up IIS manager.
15. Create a .Net 4.0 application pool that runs under NetworkServices
16. Create a web site and set its physical path to $jungo\Website\Web\Jungo. Set its application pool to the one you created in step 15.
17. In order to initialize the site and create the database tables you must add do the following:
	a. Open Up web.config
	b. Look for the <forms> element.
	c. Add the following inside of the forms element:
		<credentials passwordFormat="Clear"><user name="Admin" password="<your password>"/></credentials>
	d. Go to the <n2> element and do the following:
		set <installer checkInstallationStatus="true" allowInstallation="true" />
18. Start up the website. When prompted, enter the user "Admin" and the password supplied in step 17.c.
19. Create the database tables. 
20. Import the supplied N2CMS site import.
21. Go to the admin by going to <site URL>/N2/
22. Log in using "Admin" and the password supplied in step 17.c.
23. Go to "users". Add an admin user.
24. Once the admin user is created, Edit the Web.config and do the following:
	a. Under <forms> remove <credentials passwordFormat="Clear"><user name="Admin" password="<your password>"/></credentials>
	b. Under <n2> change <installer> so it appears like the following: <installer checkInstallationStatus="false" allowInstallation="false" />

For any inquiries on the .NET connector project, please contact: N2cmsGCconnector@digitalriver.com

##Sports US Inc.
The fictitious sporting goods online retailer Sports US Inc. provides the perfect playground to highlight the capabilities offered by Global Commerce Shopper API.

You can see a working demo hosted on Azure at: http://jungodemo.digitalriver.com/sportsus/en-us/home

##Known Issues
D-02757: In IE 11, the cart quantity dropdown does not update the cart

##Recommended Customizations
* Upgrade Admin Authentication Security -- For the administrative interface, we suggest keeping access to only local users through IP whitelisting.  We also recommend to force a secure connection (HTTPS), upgrade the password policy, lock a user after 5 failed login attempts, provide CAPTCHA to avoid automated guessing and set the secure flag on the session cookies for users authenticating into the administrative interface.  For important operations, include a random token as an additional parameter in the request. This token should be randomly generated and should be unique for each user.  Also the application should ensure that the session is terminated on the server after a period of inactivity and when the user has logged out.  Enable the "X-FRAME-OPTIONS" header and implement frame-busting code on all sensitive pages.
* Caching -- A layer of data caching between the connector and Shopper APIs for performance and fault tolerant internet connectivity would be highly recommended. Caching framework like memcached can be easily added.
* Log mining -- A process to load application log files into database or a big data analysis engine to get rollup metrics would be helpful to measure & monitor system performance and analyze trends.
* CMS resource editor -- Instead of the current file based resource management, you can give more control to your end users by creating a CMS driven resource editor allowing for the drafting and deploying of resource value changes.
* Mini cart display -- The mini cart display is essential for any eCommerce site.  The mini cart display part exists but needs to be implemented into your custom drop zones for your site.
* Pagination -- When your catalog begins to grow and your catalog list page becomes overcrowded, we suggest creating a pagination that displays more results as the user scrolls through the list.
* Facetted search result navigation -- Within the search results API response, you will see facetted options to limit the search results based on product attribute.  It is suggested to present these facet options to the end user to help narrow their product results.
