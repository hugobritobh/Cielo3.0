namespace Cielo
{
    /// <summary>
    /// Utilizado para cartão de Débito
    /// </summary>
    public class Wallet
    {
        public TypeWallet Type { get; set; }
        public string WalletKey { get; set; }
        public int Eci { get; set; }

        public AdditionalData[] AdditionalData { get; set; }
        public string SecurityCode { get; set; }
    }

    public class AdditionalData
    {
        public string CaptureCode { get; set; }
    }

    public enum TypeWallet
    {
        VisaCheckout,
        Masterpass
    }
}
