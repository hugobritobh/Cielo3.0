namespace Cielo
{
    public class ReturnTId
    {
        public string MerchantOrderId { get; set; }
        public string AcquirerOrderId { get; set; }
        public Customer Customer { get; set; }
        public Payment Payment { get; set; }
    }
}
