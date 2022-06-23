namespace Cielo
{
    public class RecurrentTransactions
    {
        public string PaymentId { get; set; }

        /// <summary>
        /// Número da Recorrência. A primeira é zero
        /// </summary>
        public int PaymentNumber { get; set; }

        public int TryNumber { get; set; }
    }
}
