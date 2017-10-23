using System;

namespace Cielo
{
    /// <summary>
    /// Holder
    /// </summary>
    public class Customer
    {
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

        public void SetBirthdate(DateTime value)
        {
            Birthdate = value.ToString("yyyy-MM-dd");
        }

        public void SetBirthdate(int year, int month, int day)
        {
            SetBirthdate(new DateTime(year, month, day));
        }

        public IdentityType GetIdentityType()
        {
            Enum.TryParse<IdentityType>(IdentityType, out IdentityType value);
            return value;
        }

        public string Email { get; set; }
        /// <summary>
        /// yyyy-MM-dd
        /// </summary>
        public string Birthdate { get; set; }
        public Address Address { get; set; }
        public Address DeliveryAddress { get; set; }
    }
}
