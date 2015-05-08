namespace Jungo.Models.ShopperApi.Common
{
    public class Address : ResourceLink
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string City { get; set; }
        public string CountrySubdivision { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string CountryName { get; set; }
        public string PhoneNumber { get; set; }
        public string CountyName { get; set; }
    }
}
