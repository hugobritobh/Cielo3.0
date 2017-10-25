using System;

namespace Cielo
{
    public class RecurrentPayment 
    {
        public RecurrentPayment()
        {
        }

        public RecurrentPayment(Interval interval, DateTime endDate)
        {
            SetInterval(interval);
            this.SetEndDate(endDate);
            this.AuthorizeNow = true;
        }

        public RecurrentPayment(Interval interval, DateTime startDate, DateTime endDate)
        {
            if (startDate.Date <= DateTime.Now.Date)
            {
                throw new ArgumentException("startDate: the starting date must be in the future");
            }

            SetInterval(interval);
            this.SetStartDate(startDate);
            this.SetEndDate(endDate);
            this.AuthorizeNow = false;
        }

        /// <summary>
        /// Identifica Pedido de recorrencia. Um RecurrentPaymentID possui inumeros PaymentID vinculados a ela. Essa é a variavel usada para Cancelar uma Recorrencia Programada
        /// </summary>
        public Guid? RecurrentPaymentId { get; set; }

        /// <summary>
        /// Define que qual o momento que uma recorrencia será criada. Se for enviado como True, ela é criada no momento da autorização, se False, a recorrencia ficará suspensaaté a data escolhida para ser iniciada.
        /// </summary>
        public bool? AuthorizeNow { get; set; }

        //[JsonConverter(typeof(DateConverter))]
        public string StartDate { get; set; }

        public void SetStartDate(DateTime date)
        {
            StartDate = date.ToString("yyyy-MM-dd");
        }

      //  [JsonConverter(typeof(DateConverter))]
        public string EndDate { get; set; }

        public void SetEndDate(DateTime date)
        {
            EndDate = date.ToString("yyyy-MM-dd");
        }

      //  [JsonConverter(typeof(DateConverter))]
        public string NextRecurrency { get; set; }

        public void SetNextRecurrency(DateTime date)
        {
            NextRecurrency = date.ToString("yyyy-MM-dd");
        }

       // [JsonConverter(typeof(StringEnumConverter))]
        public string Interval { get; set; }

        public void SetInterval(Interval value)
        {
            Interval = value.ToString();
        }

        public Interval GetInterval()
        {
            Enum.TryParse<Interval>(Interval, out Interval value);
            return value;
        }

        public Link Link { get; set; }
        public string ReasonCode { get; set; }

        public string ReasonMessage { get; set; }
    }
}
