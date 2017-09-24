using System;

namespace Cielo
{
    public class Customer
    {
        public Customer()
        {
        }

        public Customer(string name)
        {
            this.Name = name;
        }

        public string Name { get; set; }
        public string Identity { get; set; }

        //[JsonConverter(typeof(StringEnumConverter))]
        public string IdentityType { get; set; }

        public void SetIdentityType(IdentityType value)
        {
            IdentityType = value.ToString();
        }

        public IdentityType GetIdentityType()
        {
            Enum.TryParse<IdentityType>(IdentityType, out IdentityType value);
            return value;
        }

        public string Email { get; set; }
        public DateTime? Birthdate { get; set; }
        public Address Address { get; set; }
        public Address DeliveryAddress { get; set; }
    }
}
