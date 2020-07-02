using System;
using System.Text;

namespace Cielo
{
    public class CieloException : Exception
    {
        private readonly int _code = 0;
        private readonly string _json = string.Empty;
        private readonly ISerializerJSON _serializer;

        public CieloException(string message, string code) : base(message)
        {
            try
            {
                _code = Convert.ToInt32(code);
            }
            catch 
            {
                _code = -1;
            }
          
        }

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
                if (string.IsNullOrEmpty(Message))
                {
                    return new Error[0];
                }
                else
                {
                    var erro = new Error() { Code = _code, Message = Message };
                    return new Error[] { erro };
                }
            }
            else
            {
                if (!_json.Contains("["))
                {
                    var erro = _serializer.Deserialize<Error>(_json);
                    return new Error[] { erro };
                }
                
                return _serializer.Deserialize<Error[]>(_json);
            }
        }

        /// <summary>
        /// Retorna os erros formatados (Code - Message) em várias linhas (caso tenha mais de um erro)
        /// </summary>
        /// <returns></returns>
        public string GetCieloErrorsString()
        {
            StringBuilder sb = new StringBuilder();

            var erros = this.GetCieloErrors();

            if (erros != null)
            {
                foreach (var item in erros)
                {
                    sb.AppendLine($"{item.Code} - Message: {item.Message}");
                }
            }
            else
            {
                sb.AppendLine("No error");
            }

            return sb.ToString();
        }
    }
}
