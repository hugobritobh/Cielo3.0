using Cielo.Helper;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Cielo
{
    public class Payment : ReturnStatus
    {
        private static readonly Regex softDescriptorMatch = new Regex("^[a-zA-Z0-9]?", RegexOptions.Compiled);
        private string _softDescriptor;

        public Payment()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="currency"></param>
        /// <param name="softDescriptor"></param>
        /// <param name="paymentType">Tipo do pagamento (Cartão, Boleto, Transferência)</param>
        /// <param name="provider">Banco que será realizado o pagamento</param>
        /// <param name="nossoNumero">Quando for boleto</param>
        /// <param name="instrucao"></param>
        /// <param name="country"></param>
        /// <param name="returnUrl">obrigatório quando é cartão de débito e transferência eletronica</param>
        public Payment(decimal amount, PaymentType paymentType, Provider provider, string nossoNumero = "", string instrucao = "", string returnUrl = "", Currency currency = Cielo.Currency.BRL, string country = Cielo.Country.BRA)
        {
            SetPaymentType(paymentType);
            SetAmount(amount);
            SetCurrency(currency);
            SetProvider(provider);
            BoletoNumber = nossoNumero;
            Instructions = instrucao;
            this.ReturnUrl = returnUrl;
            //this.SoftDescriptor = softDescriptor;
            this.Country = country;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="currency"></param>
        /// <param name="installments">Numero de Parcelas</param>
        /// <param name="capture"></param>
        /// <param name="softDescriptor"></param>
        /// <param name="creditCard"></param>
        /// <param name="paymentType">Tipo do pagamento (Cartão, Boleto, Transferência)</param>
        /// <param name="country"></param>
        /// <param name="returnUrl">obrigatório quando é cartão de débito e transferência eletronica</param>
        public Payment(decimal amount, Currency currency, int installments, bool capture, string softDescriptor, Card card, PaymentType paymentType = PaymentType.CreditCard, string country = Cielo.Country.BRA, string returnUrl = "", RecurrentPayment recurrentPayment = null, Wallet wallet = null)
        {
            SetPaymentType(paymentType);
            SetAmount(amount);
            SetCurrency(currency);
            this.Installments = installments;
            this.Capture = capture;
            this.SoftDescriptor = softDescriptor;
            this.RecurrentPayment = recurrentPayment;

            SetCard(card, paymentType);

            this.Country = country;
            this.ReturnUrl = returnUrl;
            this.Wallet = wallet;
        }

        private void SetCard(Card card, PaymentType paymentType)
        {
            if (paymentType == PaymentType.CreditCard)
            {
                this.CreditCard = card;
            }
            else if (paymentType == PaymentType.DebitCard)
            {
                this.DebitCard = card;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="currency"></param>
        /// <param name="installments">Numero de Parcelas</param>
        /// <param name="softDescriptor"></param>
        /// <param name="card"></param>
        /// <param name="recurrentPayment"></param>
        /// <param name="country"></param>
        public Payment(decimal amount, Currency currency, int installments, string softDescriptor, Card card, RecurrentPayment recurrentPayment, string country = Cielo.Country.BRA)
        {
            SetPaymentType(PaymentType.CreditCard);
            SetAmount(amount);
            SetCurrency(currency);
            SetCard(card, PaymentType.CreditCard);

            this.Installments = installments;
            this.SoftDescriptor = softDescriptor;
            this.RecurrentPayment = recurrentPayment;
            this.Country = country;

        }

        public int? Installments { get; set; }

        public string Interest { get; set; }

        public void SetInterest(Interest value)
        {
            Interest = value.ToString();
        }

        public Interest GetInterest()
        {
            Enum.TryParse<Interest>(Interest, out Interest value);
            return value;
        }

        public bool? Capture { get; set; }
        /// <summary>
        /// Define se o comprador será direcionado ao Banco emissor para autenticação do cartão.
        /// False por padrão. 
        /// </summary>
        public bool? Authenticate { get; set; }
        public bool? Recurrent { get; set; }
        public RecurrentPayment RecurrentPayment { get; set; }
        public Card CreditCard { get; set; }
        public string Tid { get; set; }
        public string ProofOfSale { get; set; }
        public string AuthorizationCode { get; set; }

        public string AuthenticationUrl { get; set; }
        public string BoletoNumber { get; private set; }
        public string Instructions { get; private set; }
        public string Country { get; set; }
        public List<object> ExtraDataCollection { get; set; }
        public string ExpirationDate { get; set; }
        public string Url { get; set; }
        public string Number { get; set; }
        public string BarCodeNumber { get; set; }
        public string DigitableLine { get; set; }
        public string Address { get; set; }

        public string SoftDescriptor
        {
            get
            {
                return _softDescriptor;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && (value.Length > 13 || !softDescriptorMatch.IsMatch(value)))
                {
                    throw new ArgumentException("SoftDescriptor: it has a limit of 13 characters (not special) and no spaces.");
                }

                _softDescriptor = value;
            }
        }
        public string ReturnUrl { get; set; }

        /// <summary>
        /// Carteira utilizada no cartão de débito
        /// </summary>
        public Wallet Wallet { get; set; }

        public string Provider { get; set; }

        public void SetProvider(Provider value)
        {
            Provider = value.ToString();
        }

        public Provider GetProvider()
        {
            Enum.TryParse<Provider>(Provider, out Provider value);
            return value;
        }

        public Guid? PaymentId { get; set; }

        public string Type { get; set; }

        public void SetPaymentType(PaymentType value)
        {
            Type = value.ToString();
        }

        public PaymentType GetPaymentType()
        {
            Enum.TryParse<PaymentType>(Type, out PaymentType value);
            return value;
        }

        public string ServiceTaxAmount { get; set; }

        public void SetServiceTaxAmount(decimal value)
        {
            ServiceTaxAmount = NumberHelper.DecimalToInteger(value).ToString();
        }

        public decimal GetServiceTaxAmount()
        {
            if (string.IsNullOrEmpty(ServiceTaxAmount))
                return 0;

            return NumberHelper.IntegerToDecimal(ServiceTaxAmount);
        }

        public string Amount { get; set; }

        public void SetAmount(decimal value)
        {
            Amount = NumberHelper.DecimalToInteger(value).ToString();
        }

        public decimal GetAmount()
        {
            if (string.IsNullOrEmpty(Amount))
                return 0;

            return NumberHelper.IntegerToDecimal(Amount);
        }

        public string CapturedAmount { get; set; }

        public void SetCapturedAmount(decimal value)
        {
            CapturedAmount = NumberHelper.DecimalToInteger(value).ToString();
        }

        public decimal GetCapturedAmount()
        {
            if (string.IsNullOrEmpty(CapturedAmount))
                return 0;

            return NumberHelper.IntegerToDecimal(CapturedAmount);
        }

        public DateTime? ReceivedDate { get; set; }
        public DateTime? CapturedDate { get; set; }

        public FraudAnalysis FraudAnalysis { get; set; }

        public string Currency { get; set; }
        public Card DebitCard { get; set; }

        public void SetCurrency(Currency value)
        {
            Currency = value.ToString();
        }

        public Currency GetCurrency()
        {
            Enum.TryParse<Currency>(Currency, out Currency value);
            return value;
        }


        public void SetExpirationDate(DateTime date)
        {
            this.ExpirationDate = date.ToString("dd/MM/yyyy");
        }

    }
}
