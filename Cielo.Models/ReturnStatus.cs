using System;
using System.Collections.Generic;

namespace Cielo
{
    public class ReturnStatus
    {
        public string CardToken { get; set; }
        public string ReturnCode { get; set; }
        public string ReturnMessage { get; set; }
        public string ReasonCode { get; set; }
        public string ReasonMessage { get; set; }
        public string ProviderReturnCode { get; set; }
        public string ProviderReturnMessage { get; set; }

        public string Status { get; set; }

        public Status GetStatus()
        {
            Enum.TryParse<Status>(Status, out Status value);
            return value;
        }

        public void SetInterval(Status value)
        {
            Status = value.ToString();
        }



        public List<Link> Links { get; set; }
    }
}
