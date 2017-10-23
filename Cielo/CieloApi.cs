using Cielo.Helper;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cielo
{
    public class CieloApi
    {
        private readonly static HttpClient _http;

        /// <summary>
        /// Tempo para TimeOut da requisição, por default é 60 segundos
        /// </summary>
        private int _timeOut = 0;

        public Merchant Merchant { get; }
        public ISerializerJSON SerializerJSON { get; }
        public CieloEnvironment Environment { get; }

        static CieloApi()
        {
            _http = new HttpClient();
            _http.DefaultRequestHeaders.ExpectContinue = false;

            /*
             O proxy pode ser definido no Web.config ou App.config da sua aplicação
             */
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="merchant"></param>
        /// <param name="serializer">Crie o seu Provider Json</param>
        /// <param name="timeOut">Tempo para TimeOut da requisição, por default é 60 segundos</param>
        public CieloApi(CieloEnvironment environment, Merchant merchant, ISerializerJSON serializer, int timeOut = 60000)
        {
            Environment = environment;
            Merchant = merchant;
            SerializerJSON = serializer;
            _timeOut = timeOut;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="merchant"></param>
        /// <param name="serializer">Crie o seu Provider Json</param>
        /// <param name="timeOut">Tempo para TimeOut da requisição, por default é 60 segundos</param>
        public CieloApi(Merchant merchant, ISerializerJSON serializer, int timeOut = 60000)
                : this(CieloEnvironment.PRODUCTION, merchant, serializer, timeOut) { }

        private IDictionary<string, string> GetHeaders(Guid requestId)
        {
            return new Dictionary<string, string>
            {
                { "RequestId", requestId.ToString() }
            };
        }

        private async Task<HttpResponseMessage> CreateRequest(string resource, Method method = Method.GET, IDictionary<string, string> headers = null)
        {
            return await CreateRequest<object>(resource, null, method, headers);
        }

        private async Task<HttpResponseMessage> CreateRequest<T>(string resource, T value, Method method = Method.POST, IDictionary<string, string> headers = null)
        {
            StringContent content = null;

            if (value != null)
            {
                string json = SerializerJSON.Serialize<T>(value);
                content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return await Execute(resource, headers, method, content);
        }

        private async Task<HttpResponseMessage> Execute(string fullUrl, IDictionary<string, string> headers = null, Method method = Method.POST, StringContent content = null)
        {
            if (headers == null)
            {
                headers = new Dictionary<string, string>(2);
            }

            headers.Add("MerchantId", Merchant.Id.ToString());
            headers.Add("MerchantKey", Merchant.Key);

            var tokenSource = new CancellationTokenSource(_timeOut);

            try
            {
                HttpMethod httpMethod = HttpMethod.Post;

                if (method == Method.POST)
                {
                    if (headers != null)
                    {
                        foreach (var item in headers)
                        {
                            content.Headers.Add(item.Key, item.Value);
                        }
                    }

                    return await _http.PostAsync(fullUrl, content, tokenSource.Token);
                }
                else
                {
                    if (method == Method.GET)
                    {
                        httpMethod = HttpMethod.Get;
                    }
                    else if (method == Method.PUT)
                    {
                        httpMethod = HttpMethod.Put;
                    }
                    else if (method == Method.DELETE)
                    {
                        httpMethod = HttpMethod.Delete;
                    }

                    var request = GetExecute(fullUrl, headers, httpMethod);
                    return await _http.SendAsync(request, tokenSource.Token);
                }
            }
            catch (OperationCanceledException e)
            {
                throw new CancellationTokenException(e);
            }
            finally
            {
                tokenSource.Dispose();
            }
        }

        private static HttpRequestMessage GetExecute(string fullUrl, IEnumerable<KeyValuePair<string, string>> headers, HttpMethod method, StringContent content = null)
        {
            var request = new HttpRequestMessage(method, fullUrl)
            {
                Content = content
            };

            foreach (var item in headers)
            {
                request.Headers.Add(item.Key, item.Value);
            }

            return request;
        }
        private T GetResponse<T>(HttpResponseMessage response)
        {
            CheckResponse(response);

            return SerializerJSON.Deserialize<T>(response.Content);
        }

        private void CheckResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new CieloException($"Error code {response.StatusCode}.", response.Content.ReadAsStringAsync().Result, this.SerializerJSON);
            }
        }

        /// <summary>
        /// Envia uma transação
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<Transaction> CreateTransaction(Guid requestId, Transaction transaction)
        {
            var headers = GetHeaders(requestId);
            var response = await CreateRequest(Environment.GetTransactionUrl("/1/sales/"), transaction, Method.POST, headers);

            return GetResponse<Transaction>(response);
        }

        public async Task<Transaction> GetTransaction(Guid paymentId)
        {
            return await GetTransaction(Guid.NewGuid(), paymentId);
        }

        /// <summary>
        /// Consulta uma transação
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        public async Task<Transaction> GetTransaction(Guid requestId, Guid paymentId)
        {
            var headers = GetHeaders(requestId);
            var response = await CreateRequest(Environment.GetQueryUrl($"/1/sales/{paymentId}"), Method.GET, headers);

            return GetResponse<Transaction>(response);
        }

        /// <summary>
        /// Cancela uma transação (parcial ou total)
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="paymentId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task<ReturnStatus> CancelTransaction(Guid requestId, Guid paymentId, decimal? amount = null)
        {
            var url = Environment.GetTransactionUrl($"/1/sales/{paymentId}/void");

            if (amount.HasValue)
            {
                url += $"?Amount={NumberHelper.DecimalToInteger(amount)}";
            }

            var headers = GetHeaders(requestId);
            var response = await CreateRequest(url, Method.PUT, headers);

            return GetResponse<ReturnStatus>(response);
        }

        /// <summary>
        /// Captura uma transação (parcial ou total)
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="paymentId"></param>
        /// <param name="amount"></param>
        /// <param name="serviceTaxAmount"></param>
        /// <returns></returns>
        public async Task<ReturnStatus> CaptureTransaction(Guid requestId, Guid paymentId, decimal? amount = null, decimal? serviceTaxAmount = null)
        {
            var url = Environment.GetTransactionUrl($"/1/sales/{paymentId}/capture");

            if (amount.HasValue)
            {
                url += $"?Amount={NumberHelper.DecimalToInteger(amount)}";
            }

            if (serviceTaxAmount.HasValue)
            {
                url += $"?SeviceTaxAmount={NumberHelper.DecimalToInteger(serviceTaxAmount)}";
            }

            var headers = GetHeaders(requestId);
            var response = await CreateRequest(url, Method.PUT, headers);

            return GetResponse<ReturnStatus>(response);
        }

        /// <summary>
        /// Ativa uma recorrência
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="recurrentPaymentId"></param>
        /// <exception cref="CieloException">Ocorreu algum erro ao tentar alterar a recorrência</exception>
        /// <returns></returns>
        public async Task<bool> ActivateRecurrent(Guid requestId, Guid recurrentPaymentId)
        {
            return await ManagerRecurrent(requestId, recurrentPaymentId, true);
        }

        /// <summary>
        /// Desativa uma recorrência
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="recurrentPaymentId"></param>
        /// <exception cref="CieloException">Ocorreu algum erro ao tentar alterar a recorrência</exception>
        /// <returns></returns>
        public async Task<bool> DeactivateRecurrent(Guid requestId, Guid recurrentPaymentId)
        {
            return await ManagerRecurrent(requestId, recurrentPaymentId, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="recurrentPaymentId"></param>
        /// <param name="active">Parâmetro que define se uma recorrência será desativada ou ativada novamente</param>
        /// <exception cref="CieloException">Ocorreu algum erro ao tentar alterar a recorrência</exception>
        /// <returns>Se retornou true é porque a operação foi realizada com sucesso</returns>
        private async Task<bool> ManagerRecurrent(Guid requestId, Guid recurrentPaymentId, bool active)
        {
            var url = Environment.GetTransactionUrl($"/1/RecurrentPayment/{recurrentPaymentId}/Deactivate");

            if (active)
            {
                //Ativar uma recorrência novamente
                url = Environment.GetTransactionUrl($"/1/RecurrentPayment/{recurrentPaymentId}/Reactivate");
            }

            var headers = GetHeaders(requestId);
            var response = await CreateRequest(url, Method.PUT, headers);

            //Se tiver errado será levantado uma exceção
            CheckResponse(response);

            return true;
        }
    }
}
