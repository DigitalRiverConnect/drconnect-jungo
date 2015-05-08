namespace DigitalRiver.CloudLink.Commerce.Nimbus.ViewModels.Shopper
{
    public class ShopperNameViewModel
    {
        public ShopperNameViewModel(string firstName, string middleName, string lastName)
        {
            FirstName = firstName;
            MiddleName = middleName;
            LastName = lastName;
            Initialize();
        }

        public string FirstName { get; private set; }

        public string MiddleName { get; private set; }

        public string LastName { get; private set; }

        private void Initialize()
        {

        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", FirstName, MiddleName, LastName);
        }
    }
}
