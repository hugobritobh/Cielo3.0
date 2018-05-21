using System;

namespace Cielo
{
    public class CreditCard
    {
        public CreditCard()
        {
        }

        public CreditCard(Guid cardToken, string securityCode, CardBrand brand)
        {
            this.CardToken = cardToken;
            this.SecurityCode = securityCode;
            SetBrand(brand);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <param name="holder"></param>
        /// <param name="expirationDate"></param>
        /// <param name="securityCode"></param>
        /// <param name="brand"></param>
        /// <param name="saveCard">Booleano que identifica se o cartão será salvo para gerar o CardToken.</param>
        public CreditCard(string cardNumber, string holder, DateTime expirationDate, string securityCode, CardBrand brand, bool saveCard = false)
        {
            this.CardNumber = cardNumber;
            this.Holder = holder;

            this.SetExpirationDate(expirationDate);

            this.SecurityCode = securityCode;
            SetBrand(brand);
            this.SaveCard = saveCard;
        }

        public string CardNumber { get; set; }
        public Guid? CardToken { get; set; }
        public string Holder { get; set; }

        /// <summary>
        /// Format MM/yyyy
        /// </summary>
        public string ExpirationDate { get; set; }

        public void SetExpirationDate(DateTime date)
        {
            ExpirationDate = date.ToString("MM/yyyy");
        }

        public string SecurityCode { get; set; }
        public bool? SaveCard { get; set; }

      //  [JsonConverter(typeof(StringEnumConverter))]
        public string Brand { get; set; }

        public void SetBrand(CardBrand value)
        {
            Brand = value.ToString();
        }

        public CardBrand GetBrand()
        {
            Enum.TryParse<CardBrand>(Brand, out CardBrand value);
            return value;
        }
    }
}
