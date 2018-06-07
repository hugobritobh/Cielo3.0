using System;

namespace Cielo
{
    /// <summary>
    /// Classe semelhante ao ReturnStatus, mas só possui um Link.
    /// 
    /// Utilizado na geração de token
    /// </summary>
    public class ReturnStatusLink
    {
        public string CardToken { get; set; }
        public string ReturnCode { get; set; }
        public string ReturnMessage { get; set; }
        public string ReasonCode { get; set; }
        public string ReasonMessage { get; set; }

        public string Status { get; set; }

        public Status GetStatus()
        {
            Enum.TryParse<Status>(Status, out Status value);
            return value;
        }

        public Link Links { get; set; }
    }
}
