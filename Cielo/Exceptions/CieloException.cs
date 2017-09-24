using System;

namespace Cielo
{
    public class CieloException : ApplicationException
    {
        private readonly string _json = string.Empty;
        private readonly ISerializerJSON _serializer;

        public CieloException(string message, string json, ISerializerJSON serializer) : base(message)
        {
            _json = json;
            _serializer = serializer;
        }

        /// <summary>
        /// Erros informado pela Cielo
        /// </summary>
        /// <returns></returns>
        public Error[] GetCieloErrors()
        {
            if (string.IsNullOrEmpty(_json))
            {
                return new Error[0];
            }

            return _serializer.Deserialize<Error[]>(_json);
        }
    }
}
