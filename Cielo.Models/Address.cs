namespace Cielo
{
    public class Address
    {
        public string Street { get; set; }
        public string Number { get; set; }
        public string Complement { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }

        /// <summary>
        /// Bairro
        /// </summary>
        public string District { get; set; }

        /// <summary>
        /// Comentário Banco Do Brasil:
        /// Os campos Customer.Address.Street; Customer.Address.Number; 
        /// Customer.Address.Complement; Customer.Address.District devem totalizar até 60 caracteres.
        /// </summary>
        public string IsValid()
        {
            //Estou evitando de utilizar recursos novos da linguagem 
            //para aumentar a compatibilidade
            int tam = 0;
            tam += GetLength(Street);
            tam += GetLength(Number);
            tam += GetLength(Complement);
            tam += GetLength(District);

            if (tam >= 60)
            {
                return "Os campos Customer.Address.Street; Customer.Address.Number; Customer.Address.Complement; Customer.Address.District devem totalizar até 60 caracteres.";
            }

            return string.Empty;
        }

        private static int GetLength(string street)
        {
            return string.IsNullOrEmpty(street) ? 0 : street.Length;
        }
    }
}
