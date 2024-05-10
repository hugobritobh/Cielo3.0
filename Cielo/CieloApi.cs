using Cielo.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cielo
{
    public class CieloApi
    {
        private readonly static HttpClient _http = new HttpClient();

        /// <summary>
        /// Tempo para TimeOut da requisição, por default é 60 segundos
        /// </summary>
        private readonly int _timeOut = 0;

        public Merchant Merchant { get; }
        public ISerializerJSON SerializerJSON { get; }
        public CieloEnvironment Environment { get; }

        static CieloApi()
        {
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
        public CieloApi(CieloEnvironment environment, Merchant merchant, ISerializerJSON serializer, int timeOut = 60000, SecurityProtocolType securityProtocolType = SecurityProtocolType.Tls12)
        {
            Environment = environment;
            Merchant = merchant;
            SerializerJSON = serializer;
            _timeOut = timeOut;

            ServicePointManager.SecurityProtocol = securityProtocolType;
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
            return new Dictionary<string, string>(1)
            {
                { "RequestId", requestId.ToString() }
            };
        }

        private async Task<HttpResponseMessage> CreateRequestAsync(string resource, Method method = Method.GET, IDictionary<string, string> headers = null)
        {
            return await CreateRequestAsync<object>(resource, null, method, headers);
        }

        private async Task<HttpResponseMessage> CreateRequestAsync<T>(string resource, T value, Method method = Method.POST, IDictionary<string, string> headers = null)
        {
            StringContent content = null;

            if (value != null)
            {
                string json = SerializerJSON.Serialize<T>(value);
                content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return await ExecuteAsync(resource, headers, method, content);
        }

        private async Task<HttpResponseMessage> ExecuteAsync(string fullUrl, IDictionary<string, string> headers = null, Method method = Method.POST, StringContent content = null)
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
                HttpMethod httpMethod = null;

                if (method == Method.POST)
                {
                    httpMethod = HttpMethod.Post;
                    if (headers != null)
                    {
                        foreach (var item in headers)
                        {
                            content.Headers.Add(item.Key, item.Value);
                        }
                    }
                }
                else if (method == Method.GET)
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

                using (var request = GetExecute(fullUrl, headers, httpMethod, content))
                {
                    return await _http.SendAsync(request, tokenSource.Token).ConfigureAwait(false);
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

            if (method != HttpMethod.Post)
            {
                foreach (var item in headers)
                {
                    request.Headers.Add(item.Key, item.Value);
                }
            }

            return request;
        }
        private async Task<T> GetResponseAsync<T>(HttpResponseMessage response)
        {
            await CheckResponseAsync(response).ConfigureAwait(false);

            return await SerializerJSON.DeserializeAsync<T>(response.Content).ConfigureAwait(false);
        }

        private async Task CheckResponseAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new CieloException($"Error code {response.StatusCode}.", result, this.SerializerJSON);
            }
        }

        /// <summary>
        /// Envia uma transação
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<Transaction> CreateTransactionAsync(Guid requestId, Transaction transaction)
        {
            if (transaction != null &&
                transaction.Customer != null &&
                transaction.Customer.Address != null)
            {
                var error = transaction.Customer.Address.IsValid();
                if (!string.IsNullOrEmpty(error))
                {
                    throw new CieloException(error, "1");
                }
            }

            var headers = GetHeaders(requestId);
            using (var response = await CreateRequestAsync(Environment.GetTransactionUrl("/1/sales/"), transaction, Method.POST, headers))
            {

                return await GetResponseAsync<Transaction>(response);
            }
        }

        /// <summary>
        /// Consulta uma transação
        /// </summary>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        public async Task<Transaction> GetTransactionAsync(Guid paymentId)
        {
            return await GetTransactionAsync(Guid.NewGuid(), paymentId);
        }

        /// <summary>
        /// Consulta uma transação
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        public async Task<Transaction> GetTransactionAsync(Guid requestId, Guid paymentId)
        {
            var headers = GetHeaders(requestId);

            using (var response = await CreateRequestAsync(Environment.GetQueryUrl($"/1/sales/{paymentId}"), Method.GET, headers))
            {
                return await GetResponseAsync<Transaction>(response);
            }
        }


        /// <summary>
        /// Consulta uma transação
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        public async Task<ReturnRecurrentPayment> GetRecurrentPaymentAsync(Guid requestId, Guid recurrentPaymentId)
        {
            var headers = GetHeaders(requestId);
            var response = await CreateRequestAsync(Environment.GetQueryUrl($"/1/RecurrentPayment/{recurrentPaymentId}"), Method.GET, headers);

            return await GetResponseAsync<ReturnRecurrentPayment>(response);
        }

        /// <summary>
        /// Cancela uma transação (parcial ou total)
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="paymentId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task<ReturnStatus> CancelTransactionAsync(Guid requestId, Guid paymentId, decimal? amount = null)
        {
            var url = Environment.GetTransactionUrl($"/1/sales/{paymentId}/void");

            if (amount.HasValue)
            {
                url += $"?Amount={NumberHelper.DecimalToInteger(amount)}";
            }

            var headers = GetHeaders(requestId);
            var response = await CreateRequestAsync(url, Method.PUT, headers);

            return await GetResponseAsync<ReturnStatus>(response);
        }

        /// <summary>
        /// Captura uma transação (parcial ou total)
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="paymentId"></param>
        /// <param name="amount"></param>
        /// <param name="serviceTaxAmount"></param>
        /// <returns></returns>
        public async Task<ReturnStatus> CaptureTransactionAsync(Guid requestId, Guid paymentId, decimal? amount = null, decimal? serviceTaxAmount = null)
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
            var response = await CreateRequestAsync(url, Method.PUT, headers);

            return await GetResponseAsync<ReturnStatus>(response);
        }

        /// <summary>
        /// Ativa uma recorrência
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="recurrentPaymentId"></param>
        /// <exception cref="CieloException">Ocorreu algum erro ao tentar alterar a recorrência</exception>
        /// <returns></returns>
        public async Task<bool> ActivateRecurrentAsync(Guid requestId, Guid recurrentPaymentId)
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
        public async Task<bool> DeactivateRecurrentAsync(Guid requestId, Guid recurrentPaymentId)
        {
            return await ManagerRecurrent(requestId, recurrentPaymentId, false);
        }

        /// <summary>
        /// Altera o valor apartir da próxima recorrencia.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="recurrentPaymentId">Identificador da recorrencia que deseja alterar.</param>
        /// <param name="amount">Novo valor da recorrencia.</param>
        /// <returns>true em caso de sucesso.</returns>
        public async Task<bool> ChangeAmountRecurrentAsync(Guid requestId, Guid recurrentPaymentId, decimal amount)
        {
            return await ManagerRecurrent(requestId, recurrentPaymentId, NumberHelper.DecimalToInteger(amount), "Amount");
        }

        /// <summary>
        /// Altera o dia da recorrência.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="recurrentPaymentId">Identificador da recorrencia que deseja alterar.</param>
        /// <param name="day">Novo dia para a recorrencia.</param>
        /// <returns>true em caso de sucesso.</returns>
        public async Task<bool> ChangeDayRecurrentAsync(Guid requestId, Guid recurrentPaymentId, int day)
        {
            return await ManagerRecurrent(requestId, recurrentPaymentId, day, "RecurrencyDay");
        }

        /// <summary>
        /// Altera a data do próximo pagamento.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="recurrentPaymentId">Identificador da recorrencia que deseja alterar.</param>
        /// <param name="nextPayment">Nova data para o próximo pagamento.</param>
        /// <returns>true em caso de sucesso.</returns>
        public async Task<bool> ChangeNextPaymentDateRecurrentAsync(Guid requestId, Guid recurrentPaymentId, DateTime nextPayment)
        {
            return await ManagerRecurrent(requestId, recurrentPaymentId, nextPayment, "NextPaymentDate");
        }

        /// <summary>
        /// Alterar os dados de pagamento da recorrência.
        /// Atenção: Essa alteração afeta a todos os dados do nó Payment. Então para manter os dados anteriores você deve informar os campos que não vão sofre alterações com os mesmos valores que já estavam salvos.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="recurrentPaymentId">Identificador da recorrencia que deseja alterar.</param>
        /// <param name="payment">Informações de pagamento.</param>
        /// <returns>true em caso de sucesso.</returns>
        public async Task<bool> ChangePaymentRecurrentAsync(Guid requestId, Guid recurrentPaymentId, Payment payment)
        {
            return await ManagerRecurrent(requestId, recurrentPaymentId, payment, "Payment");
        }

        /// <summary>
        /// Altera dados do comprador da recorrência.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="recurrentPaymentId">Identificador da recorrencia que deseja alterar.</param>
        /// <param name="customer">Dados do comprador da recorrência.</param>
        /// <returns>true em caso de sucesso.</returns>
        public async Task<bool> ChangeCustomerRecurrentAsync(Guid requestId, Guid recurrentPaymentId, Customer customer)
        {
            return await ManagerRecurrent(requestId, recurrentPaymentId, customer, "Customer");
        }

        /// <summary>
        /// Altera dados do comprador da recorrência.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="recurrentPaymentId">Identificador da recorrencia que deseja alterar.</param>
        /// <param name="endDate">Data final da recorrência.</param>
        /// <returns>true em caso de sucesso.</returns>
        public async Task<bool> ChangeEndDateRecurrentAsync(Guid requestId, Guid recurrentPaymentId, DateTime endDate)
        {
            return await ManagerRecurrent(requestId, recurrentPaymentId, endDate, "EndDate");
        }

        /// <summary>
        /// Altera a data final da recorrência.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="recurrentPaymentId">Identificador da recorrencia que deseja alterar.</param>
        /// <param name="interval">Intervalo da recorrencia.</param>
        /// <returns>true em caso de sucesso.</returns>
        public async Task<bool> ChangeIntervalRecurrentAsync(Guid requestId, Guid recurrentPaymentId, Interval interval)
        {
            return await ManagerRecurrent(requestId, recurrentPaymentId, interval, "Interval");
        }

        /// <summary>
        /// Cria uma Token de um cartão válido ou não.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="recurrentPaymentId"></param>
        /// <param name="active">Parâmetro que define se uma recorrência será desativada ou ativada novamente</param>
        /// <exception cref="CieloException">Ocorreu algum erro ao tentar alterar a recorrência</exception>
        /// <returns>Se retornou true é porque a operação foi realizada com sucesso</returns>
        public async Task<ReturnStatusLink> CreateTokenAsync(Guid requestId, Card card)
        {
            card.CustomerName = card.Holder;
            card.SecurityCode = string.Empty;

            var url = Environment.GetTransactionUrl($"/1/Card");
            var headers = GetHeaders(requestId);
            var response = await CreateRequestAsync(url, card, Method.POST, headers);

            //Se tiver errado será levantado uma exceção
            return await GetResponseAsync<ReturnStatusLink>(response);
        }

        /// <summary>
        /// Faz pagamento de 1 real e cancela logo em seguida para testar se o cartão é válido.
        /// Gera token somente de cartão válido
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        public async Task<ReturnStatusLink> CreateTokenValidAsync(Guid requestId, Card creditCard, string softDescriptor = "Validando", Currency currency = Currency.BRL)
        {
            creditCard.SaveCard = true;
            creditCard.CustomerName = creditCard.Holder;

            var customer = new Customer(creditCard.CustomerName);

            var payment = new Payment(amount: 1,
                                      currency: currency,
                                      paymentType: PaymentType.CreditCard,
                                      installments: 1,
                                      capture: true,
                                      recurrentPayment: null,
                                      softDescriptor: softDescriptor,
                                      card: creditCard,
                                      returnUrl: string.Empty);

            var transaction = new Transaction(Guid.NewGuid().ToString(), customer, payment);

            var result = await CreateTransactionAsync(requestId, transaction);
            var status = result.Payment.GetStatus();
            if (status == Status.Authorized || status == Status.PaymentConfirmed)
            {
                //Cancelando pagamento de 1 REAL
                var resultCancel = await CancelTransactionAsync(Guid.NewGuid(), result.Payment.PaymentId.Value, 1);
                var status2 = resultCancel.GetStatus();
                if (status2 != Status.Voided)
                {
                    return new ReturnStatusLink
                    {
                        ReturnCode = resultCancel.ReturnCode,
                        ReturnMessage = resultCancel.ReturnMessage,
                        Status = resultCancel.Status,
                        Links = resultCancel.Links.FirstOrDefault()
                    };
                }
            }
            else
            {
                return new ReturnStatusLink
                {
                    ReturnCode = result.Payment.ReturnCode,
                    ReturnMessage = result.Payment.ReturnMessage,
                    Status = result.Payment.Status,
                    Links = result.Payment.Links.FirstOrDefault()
                };
            }

            var token = result.Payment.CreditCard.CardToken.HasValue ? result.Payment.CreditCard.CardToken.Value.ToString() : string.Empty;
            var statusLink = new ReturnStatusLink
            {
                CardToken = token,
                ReturnCode = result.Payment.ReturnCode,
                ReturnMessage = result.Payment.ReturnMessage,
                Status = result.Payment.Status,
                Links = result.Payment.Links.FirstOrDefault()
            };

            return statusLink;
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
            var response = await CreateRequestAsync(url, Method.PUT, headers);

            //Se tiver errado será levantado uma exceção
            await CheckResponseAsync(response);

            return true;
        }

        private async Task<bool> ManagerRecurrent(Guid requestId, Guid recurrentPaymentId, object data, string service)
        {
            var url = Environment.GetTransactionUrl($"/1/RecurrentPayment/{recurrentPaymentId}/{service}");

            var headers = GetHeaders(requestId);
            var response = await CreateRequestAsync(url, data, Method.PUT, headers);

            //Se tiver errado será levantado uma exceção
            await CheckResponseAsync(response);

            return true;
        }

        #region Método Sincronos

        /// <summary>
        ///  Cria uma Token de um cartão válido ou não.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        public ReturnStatusLink CreateToken(Guid requestId, Card card)
        {
            return RunTask(() =>
            {
                return CreateTokenAsync(requestId, card);
            });
        }

        public ReturnRecurrentPayment GetRecurrentPayment(Guid requestId, Guid recurrentPaymentId)
        {
            return RunTask(() =>
            {
                return GetRecurrentPaymentAsync(requestId, recurrentPaymentId);
            });
        }

        public ReturnMerchandOrderID GetMerchandOrderID(string merchantOrderId)
        {
            return RunTask(() =>
            {
                return GetMerchandOrderIDAsync(merchantOrderId);
            });
        }

        /// <summary>
        /// Consulta uma transação
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        public async Task<ReturnMerchandOrderID> GetMerchandOrderIDAsync(string merchantOrderId)
        {
            var headers = GetHeaders(Guid.NewGuid());
            using (var response = await CreateRequestAsync(Environment.GetQueryUrl($"/1/sales?merchantOrderId={merchantOrderId}"), Method.GET, headers))
            {

                return await GetResponseAsync<ReturnMerchandOrderID>(response);
            }
        }

        public ReturnTId GetTId(string tid)
        {
            return RunTask(() =>
            {
                return GetTIdAsync(tid);
            });
        }

        /// <summary>
        /// Para consultar uma venda através do número de referência único da transação na adquirente (TId), execute um GET conforme descrito a seguir.
        /// São elegíveis para a consulta apenas transações dentro dos últimos três meses.
        /// </summary>
        /// <param name="tid"></param>
        /// <returns></returns>
        public async Task<ReturnTId> GetTIdAsync(string tid)
        {
            var headers = GetHeaders(Guid.NewGuid());
            using (var response = await CreateRequestAsync(Environment.GetQueryUrl($"/1/sales/acquirerTid/{tid}"), Method.GET, headers))
            {
                return await GetResponseAsync<ReturnTId>(response);
            }
        }

        /// <summary>
        /// Faz pagamento de 1 real e cancela logo em seguida para testar se o cartão é válido.
        /// Gera token somente de cartão válido
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="creditCard"></param>
        /// <param name="softDescriptor"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        public ReturnStatusLink CreateTokenValid(Guid requestId, Card creditCard, string softDescriptor = "Validando", Currency currency = Currency.BRL)
        {
            return RunTask(() =>
            {
                return CreateTokenValidAsync(requestId, creditCard, softDescriptor, currency);
            });
        }

        /// <summary>
        /// Captura uma transação (parcial ou total)
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="paymentId"></param>
        /// <param name="amount"></param>
        /// <param name="serviceTaxAmount"></param>
        /// <returns></returns>
        public ReturnStatus CaptureTransaction(Guid requestId, Guid paymentId, decimal? amount = null, decimal? serviceTaxAmount = null)
        {
            return RunTask(() =>
            {
                return CaptureTransactionAsync(requestId, paymentId, amount, serviceTaxAmount);
            });
        }

        public bool ActivateRecurrent(Guid requestId, Guid recurrentPaymentId)
        {
            return RunTask(() =>
            {
                return ActivateRecurrentAsync(requestId, recurrentPaymentId);
            });
        }

        public bool DeactivateRecurrent(Guid requestId, Guid recurrentPaymentId)
        {
            return RunTask(() =>
            {
                return DeactivateRecurrentAsync(requestId, recurrentPaymentId);
            });
        }

        public bool ChangeAmountRecurrent(Guid requestId, Guid recurrentPaymentId, decimal amount)
        {
            return RunTask(() =>
            {
                return ChangeAmountRecurrentAsync(requestId, recurrentPaymentId, amount);
            });
        }

        /// <summary>
        /// Altera o dia da recorrência.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="recurrentPaymentId">Identificador da recorrencia que deseja alterar.</param>
        /// <param name="day">Novo dia para a recorrencia.</param>
        /// <returns>true em caso de sucesso.</returns>
        public bool ChangeDayRecurrent(Guid requestId, Guid recurrentPaymentId, int day)
        {
            return RunTask(() =>
            {
                return ChangeDayRecurrentAsync(requestId, recurrentPaymentId, day);
            });
        }

        /// <summary>
        /// Altera a data do próximo pagamento.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="recurrentPaymentId">Identificador da recorrencia que deseja alterar.</param>
        /// <param name="nextPayment">Nova data para o próximo pagamento.</param>
        /// <returns>true em caso de sucesso.</returns>
        public bool ChangeNextPaymentDateRecurrent(Guid requestId, Guid recurrentPaymentId, DateTime nextPayment)
        {
            return RunTask(() =>
            {
                return ChangeNextPaymentDateRecurrentAsync(requestId, recurrentPaymentId, nextPayment);
            });
        }

        /// <summary>
        /// Alterar os dados de pagamento da recorrência.
        /// Atenção: Essa alteração afeta a todos os dados do nó Payment. Então para manter os dados anteriores você deve informar os campos que não vão sofre alterações com os mesmos valores que já estavam salvos.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="recurrentPaymentId">Identificador da recorrencia que deseja alterar.</param>
        /// <param name="payment">Dados de pagamento.</param>
        /// <returns>true em caso de sucesso.</returns>
        public bool ChangePaymentRecurrent(Guid requestId, Guid recurrentPaymentId, Payment payment)
        {
            return RunTask(() =>
            {
                return ChangePaymentRecurrentAsync(requestId, recurrentPaymentId, payment);
            });
        }

        /// <summary>
        /// Altera dados do comprador da recorrência.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="recurrentPaymentId">Identificador da recorrencia que deseja alterar.</param>
        /// <param name="customer">Dados do comprador da recorrência.</param>
        /// <returns>true em caso de sucesso.</returns>
        public bool ChangeCustomerRecurrent(Guid requestId, Guid recurrentPaymentId, Customer customer)
        {
            return RunTask(() =>
            {
                return ChangeCustomerRecurrentAsync(requestId, recurrentPaymentId, customer);
            });
        }

        /// <summary>
        /// Altera a data final da recorrência.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="recurrentPaymentId">Identificador da recorrencia que deseja alterar.</param>
        /// <param name="endDate">Data final da recorrencia.</param>
        /// <returns>true em caso de sucesso.</returns>
        public bool ChangeEndDateRecurrent(Guid requestId, Guid recurrentPaymentId, DateTime endDate)
        {
            return RunTask(() =>
            {
                return ChangeEndDateRecurrentAsync(requestId, recurrentPaymentId, endDate);
            });
        }

        /// <summary>
        /// Altera a data final da recorrência.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="recurrentPaymentId">Identificador da recorrencia que deseja alterar.</param>
        /// <param name="interval">Intervalo da recorrencia.</param>
        /// <returns>true em caso de sucesso.</returns>
        public bool ChangeIntervalRecurrent(Guid requestId, Guid recurrentPaymentId, Interval interval)
        {
            return RunTask(() =>
            {
                return ChangeIntervalRecurrentAsync(requestId, recurrentPaymentId, interval);
            });
        }

        public ReturnStatus CancelTransaction(Guid requestId, Guid paymentId, decimal? amount = null)
        {
            return RunTask(() =>
            {
                return CancelTransactionAsync(requestId, paymentId, amount);
            });
        }

        /// <summary>
        /// Consulta uma transação
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        public Transaction GetTransaction(Guid requestId, Guid paymentId)
        {
            return RunTask(() =>
            {
                return GetTransactionAsync(requestId, paymentId);
            });
        }

        /// <summary>
        /// Consulta uma transação
        /// </summary>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        public Transaction GetTransaction(Guid paymentId)
        {
            return RunTask(() =>
            {
                return GetTransactionAsync(paymentId);
            });
        }

        /// <summary>
        /// Envia uma transação
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public Transaction CreateTransaction(Guid requestId, Transaction transaction)
        {
            return RunTask(() =>
            {
                return CreateTransactionAsync(requestId, transaction);
            });
        }

        private static TResult RunTask<TResult>(Func<Task<TResult>> method)
        {
            try
            {
                return Task.Run(method).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                if (e.InnerException is CieloException ex)
                {
                    throw ex;
                }
                else if (e is CieloException exCielo)
                {
                    throw exCielo;
                }

                throw;
            }
        }
        #endregion
    }
}
