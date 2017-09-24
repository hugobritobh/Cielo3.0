using System;

namespace Cielo
{
    public class CancellationTokenException : ApplicationException
    {
        private const string _msg = "Timeout out.";

        public CancellationTokenException()
            : base(_msg) { }

        public CancellationTokenException(Exception innerException)
           : base(_msg, innerException) { }
    }
}
