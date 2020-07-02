using CieloTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Cielo.Tests
{
    [TestClass()]
    public class CieloApiTests
    {
        private string _nome;
        private string _nomeCartao;
        private string _descricao;
        private CieloApi _api;
        private DateTime _validDate;
        private DateTime _invalidDate;

        [TestInitialize]
        public void ConfigEnvironment()
        {
            ISerializerJSON json = new SerializerJSON();

            _api = new CieloApi(CieloEnvironment.SANDBOX, Merchant.SANDBOX, json);
            _validDate = DateTime.Now.AddYears(2);
            _invalidDate = DateTime.Now.AddYears(-2);

            _nome = "Hugo Alves";
            _nomeCartao = "Hugo de Brito V R Alves";
            _descricao = "Teste Cielo";
        }

        [TestMethod()]
        public string AutorizacaoCredito()
        {
            decimal value = 150.01M;
            CardBrand brand = CardBrand.Visa;


            var customer = new Customer(name: _nomeCartao)
            {
                Address = new Address()
                {
                    ZipCode = "3100000",
                    City = "Belo Horizonte",
                    State = "MG",
                    Complement = "apartamento 501",
                    District = "Pampulha",
                    Street = "Rua Carvalho Nova Campos",
                    Number = "321",
                    Country = "BR"
                }
            };

            customer.SetIdentityType(IdentityType.CPF);
            customer.Identity = "14258222402"; //numero gerado aleatoriamente
            customer.SetBirthdate(1990, 2, 1);
            customer.Email = "teste@gmail.com";

            var creditCard = new Card(
                cardNumber: SandboxCreditCard.Authorized1,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: brand);

            var payment = new Payment(
                amount: value,
                currency: Currency.BRL,
                installments: 1,
                capture: false,
                softDescriptor: _descricao,
                card: creditCard,
                returnUrl: "http://www.cielo.com.br");

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var returnTransaction = _api.CreateTransaction(Guid.NewGuid(), transaction);

            //Consultando
            var result = _api.GetTransaction(returnTransaction.Payment.PaymentId.Value);

            Assert.IsTrue(result.Payment.CreditCard.GetBrand() == brand, "Erro na bandeira do cartão");
            Assert.IsTrue(result.Payment.CreditCard.ExpirationDate == _validDate.ToString("MM/yyyy"), "Erro na data de vencimento do cartão");
            Assert.IsTrue(result.Payment.GetAmount() == value, "Erro no valor da fatura");
            Assert.IsTrue(result.Payment.GetStatus() == Status.Authorized, "Transação não foi autorizada");

            return merchantOrderId.ToString();
        }

        [TestMethod()]
        public void AutorizacaoDebito()
        {
            decimal value = 178.91M;
            CardBrand brand = CardBrand.Master;

            var customer = new Customer(name: _nome);

            var card = new Card(
                cardNumber: SandboxCreditCard.Authorized1,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: brand);

            var payment = new Payment(
                amount: value,
                currency: Currency.BRL,
                paymentType: PaymentType.DebitCard,
                installments: 1,
                capture: true,
                softDescriptor: _descricao,
                card: card,
                returnUrl: "http://www.cielo.com.br");

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var returnTransaction = _api.CreateTransaction(Guid.NewGuid(), transaction);

            //Consultando
            var result = _api.GetTransaction(returnTransaction.Payment.PaymentId.Value);

            Assert.IsTrue(result.Payment.DebitCard.GetBrand() == brand, "Erro na bandeira do cartão");
            Assert.IsTrue(result.Payment.DebitCard.ExpirationDate == _validDate.ToString("MM/yyyy"), "Erro na data de vencimento do cartão");
            Assert.IsTrue(result.Payment.GetAmount() == value, "Erro no valor da fatura");
            Assert.IsTrue(!string.IsNullOrEmpty(result.Payment.Links[0].Href), "Link para o redirecionamento não retornado");

            //No caso primeiro tem q ser redirecionado para depois consultar
            Assert.IsTrue(!String.IsNullOrEmpty(returnTransaction.Payment.AuthenticationUrl), "AuthenticationUrl não foi retornada");
        }

        [TestMethod()]
        public void TransacaoCapturada()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new Card(
                cardNumber: SandboxCreditCard.Authorized1,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa);

            var payment = new Payment(
                amount: 2500,
                currency: Currency.BRL,
                installments: 1,
                capture: true,
                softDescriptor: _descricao,
                card: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var returnTransaction = _api.CreateTransaction(Guid.NewGuid(), transaction);

            Assert.IsTrue(returnTransaction.Payment.GetStatus() == Status.PaymentConfirmed, "Transação não teve pagamento confirmado");
        }

        [TestMethod()]
        public void TransacaoCapturadaComCartaoNaoAutorizadaResultadoNaoAutorizada()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new Card(
                cardNumber: SandboxCreditCard.NotAuthorized,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa);

            var payment = new Payment(
                amount: 150.02M,
                currency: Currency.BRL,
                installments: 1,
                capture: true,
                softDescriptor: _descricao,
                card: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var returnTransaction = _api.CreateTransaction(Guid.NewGuid(), transaction);

            Assert.IsTrue(returnTransaction.Payment.GetStatus() == Status.Denied, "Transação não foi negada");
        }

        [TestMethod()]
        public void TransacaoCapturadaComCartaoBloqueadoResultadoNaoAutorizada()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new Card(
                cardNumber: SandboxCreditCard.NotAuthorizedCardBlocked,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa);

            var payment = new Payment(
                amount: 150.06M,
                currency: Currency.BRL,
                installments: 1,
                capture: true,
                softDescriptor: _descricao,
                card: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var returnTransaction = _api.CreateTransaction(Guid.NewGuid(), transaction);

            Assert.IsTrue(returnTransaction.Payment.GetStatus() == Status.Denied, "Transação não foi negada");
        }

        [TestMethod()]
        public void TransacaoCapturadaComCartaoCanceladoResultadoNaoAutorizada()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new Card(
                cardNumber: SandboxCreditCard.NotAuthorizedCardCanceled,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa);

            var payment = new Payment(
                amount: 150.03M,
                currency: Currency.BRL,
                installments: 1,
                capture: true,
                softDescriptor: _descricao,
                card: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var returnTransaction = _api.CreateTransaction(Guid.NewGuid(), transaction);

            Assert.IsTrue(returnTransaction.Payment.GetStatus() == Status.Denied, "Transação não foi negada");
        }

        [TestMethod()]
        public void TransacaoCapturadaComCartaoExpiradoResultadoNaoAutorizada()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new Card(
                cardNumber: SandboxCreditCard.NotAuthorizedCardExpired,
                holder: _nomeCartao,
                expirationDate: _invalidDate,
                securityCode: "123",
                brand: CardBrand.Visa);

            var payment = new Payment(
                amount: 150.04M,
                currency: Currency.BRL,
                installments: 1,
                capture: true,
                softDescriptor: _descricao,
                card: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            try
            {
                var returnTransaction = _api.CreateTransaction(Guid.NewGuid(), transaction);

                Assert.IsTrue(returnTransaction.Payment.GetStatus() == Status.Denied, "Transação não foi negada");
            }
            catch (CieloException ex)
            {
                Assert.IsTrue(ex.GetCieloErrors().Any(i => i.Code == 126));
            }
        }

        [TestMethod()]
        public void TransacaoCapturadaComCartaoComProblemasResultadoNaoAutorizada()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new Card(
                cardNumber: SandboxCreditCard.NotAuthorizedCardProblems,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa);

            var payment = new Payment(
                amount: 150.05M,
                currency: Currency.BRL,
                installments: 1,
                capture: true,
                softDescriptor: _descricao,
                card: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var returnTransaction = _api.CreateTransaction(Guid.NewGuid(), transaction);

            //Consultando
            var result = _api.GetTransaction(returnTransaction.Payment.PaymentId.Value);

            Assert.IsTrue(result.Payment.GetStatus() == Status.Denied, "Transação não foi negada");
        }

        [TestMethod()]
        public void TransacaoCapturadaComCartaoTimeOut()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new Card(
                cardNumber: SandboxCreditCard.NotAuthorizedTimeOut,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa);

            var payment = new Payment(
                amount: 150.06M,
                currency: Currency.BRL,
                installments: 1,
                capture: true,
                softDescriptor: _descricao,
                card: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var returnTransaction = _api.CreateTransaction(Guid.NewGuid(), transaction);

            Assert.IsTrue(returnTransaction.Payment.ReturnCode == "99", "Resultado esperado Time Out (Código 99).");
        }

        [TestMethod()]
        public void AutorizacaoDepoisCaptura()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new Card(
                cardNumber: SandboxCreditCard.Authorized1,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa);

            var payment = new Payment(
                amount: 150.07M,
                currency: Currency.BRL,
                installments: 1,
                capture: false,
                softDescriptor: _descricao,
                card: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var result = _api.CreateTransaction(Guid.NewGuid(), transaction);
            var captureTransaction = _api.CaptureTransaction(Guid.NewGuid(), result.Payment.PaymentId.Value);

            Assert.IsTrue(captureTransaction.GetStatus() == Status.PaymentConfirmed, "Captura não teve pagamento confirmado");
        }

        [TestMethod()]
        public void AutorizacaoDepoisCapturaDepoisCancelaResultadoCancelado()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new Card(
                cardNumber: SandboxCreditCard.Authorized1,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa);

            var payment = new Payment(
                amount: 150.08M,
                currency: Currency.BRL,
                installments: 1,
                capture: false,
                softDescriptor: _descricao,
                card: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var result = _api.CreateTransaction(Guid.NewGuid(), transaction);
            var captureTransaction = _api.CaptureTransaction(Guid.NewGuid(), result.Payment.PaymentId.Value);

            Assert.IsTrue(captureTransaction.GetStatus() == Status.PaymentConfirmed);

            var cancelationTransaction = _api.CancelTransaction(Guid.NewGuid(), result.Payment.PaymentId.Value);

            Assert.IsTrue(cancelationTransaction.GetStatus() == Status.Voided, "Cancelamento não teve sucesso");
        }

        [TestMethod()]
        public void AutorizacaoDepoisCapturaParcial()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new Card(
                cardNumber: SandboxCreditCard.Authorized1,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa);

            var payment = new Payment(
                amount: 150.25M,
                currency: Currency.BRL,
                installments: 1,
                capture: false,
                softDescriptor: _descricao,
                card: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var result = _api.CreateTransaction(Guid.NewGuid(), transaction);
            var captureTransaction = _api.CaptureTransaction(Guid.NewGuid(), result.Payment.PaymentId.Value, 25.00M);

            Assert.IsTrue(captureTransaction.GetStatus() == Status.PaymentConfirmed, "Transação não teve pagamento aprovado");
        }

        [TestMethod()]
        public void AutorizacaoComTokenizacaoDoCartao()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new Card(
                cardNumber: SandboxCreditCard.Authorized2,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa,
                saveCard: true);

            var payment = new Payment(
                amount: 157.37M,
                currency: Currency.BRL,
                installments: 1,
                capture: false,
                softDescriptor: _descricao,
                card: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var result = _api.CreateTransaction(Guid.NewGuid(), transaction);

            Assert.IsNotNull(result.Payment.CreditCard.CardToken, "Não foi criado o token");
        }

        [TestMethod()]
        public void TransacaoCapturadaComTokenizacao()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new Card(
                cardNumber: SandboxCreditCard.Authorized2,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa,
                saveCard: true);

            var payment = new Payment(
                amount: 150.09M,
                currency: Currency.BRL,
                installments: 1,
                capture: true,
                softDescriptor: _descricao,
                card: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var result = _api.CreateTransaction(Guid.NewGuid(), transaction);

            Assert.IsNotNull(result.Payment.CreditCard.CardToken, "Não foi criado o token");
        }

        [TestMethod()]
        public void TransacaoRecorrenteAgendada()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new Card(
                cardNumber: SandboxCreditCard.Authorized2,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Master,
                saveCard: true);

            var recurrentPayment = new RecurrentPayment(
                interval: Interval.Monthly,
                startDate: DateTime.Now.AddMonths(1),
                endDate: DateTime.Now.AddMonths(7));

            var payment = new Payment(
                amount: 150.01M,
                currency: Currency.BRL,
                installments: 1,
                softDescriptor: _descricao,
                card: creditCard,
                recurrentPayment: recurrentPayment);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var result = _api.CreateTransaction(Guid.NewGuid(), transaction);

            Assert.IsTrue(result.Payment.GetStatus() == Status.Scheduled, "Recorrência não foi programada");
            Assert.IsTrue(result.Payment.RecurrentPayment.RecurrentPaymentId.HasValue, "Não foi gerado o RecurrentPaymentId");
        }


        [TestMethod()]
        public void TransacaoRecorrenteAgora()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new Card(
                cardNumber: SandboxCreditCard.Authorized1,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa,
                saveCard: false);

            var recurrentPayment = new RecurrentPayment(
                interval: Interval.Monthly,
                endDate: DateTime.Now.AddYears(20));

            var payment = new Payment(
                amount: 150.02M,
                currency: Currency.BRL,
                installments: 1,
                softDescriptor: _descricao,
                card: creditCard,
                recurrentPayment: recurrentPayment);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var result = _api.CreateTransaction(Guid.NewGuid(), transaction);

            Assert.IsTrue(result.Payment.GetStatus() == Status.Authorized, "Recorrência não foi autorizada");
            Assert.IsTrue(result.Payment.RecurrentPayment.RecurrentPaymentId.HasValue, "Não foi gerado o RecurrentPaymentId");
        }

        [TestMethod()]
        public void TransacaoCancelarRecorrencia()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new Card(
                cardNumber: SandboxCreditCard.Authorized2,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa,
                saveCard: false);

            var recurrentPayment = new RecurrentPayment(
                interval: Interval.Monthly,
                startDate: DateTime.Now.AddDays(2),
                endDate: DateTime.Now.AddMonths(6));

            var payment = new Payment(
                amount: 150.03M,
                currency: Currency.BRL,
                installments: 1,
                softDescriptor: _descricao,
                card: creditCard,
                recurrentPayment: recurrentPayment);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            //https://apisandbox.cieloecommerce.cielo.com.br/1/RecurrentPayment/{RecurrentPaymentId}/Deactivate

            var result = _api.CreateTransaction(Guid.NewGuid(), transaction);
            var result2 = _api.DeactivateRecurrent(Guid.NewGuid(), result.Payment.RecurrentPayment.RecurrentPaymentId.Value);

            Assert.IsTrue(result2, "Recorrência não foi desativada");
        }

        [TestMethod()]
        public void TransacaoReabilitarRecorrencia()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new Card(
                cardNumber: SandboxCreditCard.Authorized2,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa,
                saveCard: false);

            var recurrentPayment = new RecurrentPayment(
                interval: Interval.Monthly,
                startDate: DateTime.Now.AddDays(2),
                endDate: DateTime.Now.AddMonths(6));

            var payment = new Payment(
                amount: 150.05M,
                currency: Currency.BRL,
                installments: 1,
                softDescriptor: _descricao,
                card: creditCard,
                recurrentPayment: recurrentPayment);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var result = _api.CreateTransaction(Guid.NewGuid(), transaction);
            var result2 = _api.ActivateRecurrent(Guid.NewGuid(), result.Payment.RecurrentPayment.RecurrentPaymentId.Value);

            Assert.IsTrue(result2);

            var result3 = _api.DeactivateRecurrent(Guid.NewGuid(), result.Payment.RecurrentPayment.RecurrentPaymentId.Value);

            Assert.IsTrue(result3, "Recorrência não foi reativada");
        }

        [TestMethod()]
        public void Boleto()
        {
            /*
                VERIFICAR NA CIELO SE SEU CADASTRO PERMITE FAZER PAGAMENTO POR BOLETO
            */

            decimal value = 162.55M;
            string boletoNumber = "0123456789";

            var customer = new Customer(name: _nome)
            {
                Address = new Address()
                {
                    ZipCode = "3100000",
                    City = "BH",
                    State = "MG",
                    District = "Centro",
                    Street = "Rua Teste",
                    Number = "321",
                    Country = "BR"
                }
            };

            var payment = new Payment(value,
                                      PaymentType.Boleto,
                                      Provider.Simulado,
                                      boletoNumber,
                                      "Instructions 123");

            var date = _validDate.AddDays(3);
            payment.SetExpirationDate(date);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);


            var returnTransaction = _api.CreateTransaction(Guid.NewGuid(), transaction);

            Assert.IsTrue(returnTransaction.Payment.GetPaymentType() == PaymentType.Boleto, "Erro no tipo de pagamento");
            Assert.IsTrue(!string.IsNullOrEmpty(returnTransaction.Payment.BarCodeNumber), "Erro código de barra");
            Assert.IsTrue(!string.IsNullOrEmpty(returnTransaction.Payment.DigitableLine), "Erro linha digitável");
            Assert.IsTrue(!string.IsNullOrEmpty(returnTransaction.Payment.Links[0].Href), "Erro na url para redirecionar para o Boleto");
        }

        [TestMethod()]
        public void TransferenciaOnline()
        {
            /*
                 VERIFICAR NA CIELO SE SEU CADASTRO PERMITE FAZER PAGAMENTO POR TEF
            */

            decimal value = 162.55M;

            var customer = new Customer(name: _nome);

            var payment = new Payment(value,
                                      PaymentType.EletronicTransfer,
                                      Provider.Simulado,
                                      returnUrl: "www.cielo.com.br");

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);


            var returnTransaction = _api.CreateTransaction(Guid.NewGuid(), transaction);

            Assert.IsTrue(returnTransaction.Payment.GetPaymentType() == PaymentType.EletronicTransfer, "Erro no tipo de pagamento");
            Assert.IsTrue(!string.IsNullOrEmpty(returnTransaction.Payment.Links[0].Href), "Erro na url para redirecionar para transferencia eletronica");
        }

        [TestMethod()]
        public void TransacaoCreateToken()
        {
            var creditCard = new Card(
                cardNumber: SandboxCreditCard.Authorized2,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa);

            var result = _api.CreateToken(Guid.NewGuid(), creditCard);

            Assert.IsTrue(!string.IsNullOrEmpty(result.CardToken), "Não foi gerado Token do cartão");
        }

        [TestMethod()]
        [ExpectedException(typeof(CieloException))]
        public void TransacaoComErroAddress()
        {
            var customer = new Customer(_nome)
            {
                Address = new Address()
                {
                    City = "Belo Horizonte",
                    Complement = "Condiminio Teste",
                    District = "Buritis da Pampulha",
                    Number = "9999",
                    State = "MG",
                    Street = "Rua dos Otonis com Afonso Pena",
                    ZipCode = "3111111",
                    Country = "BR"
                }
            };

            var creditCard = new Card(
                cardNumber: SandboxCreditCard.Authorized2,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa,
                saveCard: true);

            var payment = new Payment(
                amount: 157.37M,
                currency: Currency.BRL,
                installments: 1,
                capture: false,
                softDescriptor: _descricao,
                card: creditCard);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            _api.CreateTransaction(Guid.NewGuid(), transaction);
        }


        [TestMethod()]
        [ExpectedException(typeof(CieloException))]
        public void TransacaoCreateTokenErro()
        {
            var creditCard = new Card(
           cardNumber: "0000000000000011",
           holder: _nomeCartao,
           expirationDate: _validDate,
           securityCode: "123",
           brand: CardBrand.Visa);

            var result = _api.CreateToken(Guid.NewGuid(), creditCard);

            //Era para dar exception por causa do Mod 10 do número do cartão
            Assert.IsTrue(string.IsNullOrEmpty(result.CardToken), "Foi gerado Token do cartão");
        }

        [TestMethod()]
        public void TransacaoCreateTokenValid()
        {
            var creditCard = new Card(
                cardNumber: SandboxCreditCard.Authorized2,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa,
                saveCard: false);

            var result = _api.CreateTokenValid(Guid.NewGuid(), creditCard);

            Assert.IsTrue(!string.IsNullOrEmpty(result.CardToken), "Não foi gerado Token do cartão");
        }

        [TestMethod()]
        public void TransacaoCreateTokenInvalid()
        {
            var creditCard = new Card(
                cardNumber: SandboxCreditCard.NotAuthorizedCardProblems,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa,
                saveCard: false);

            ReturnStatusLink result = _api.CreateTokenValid(Guid.NewGuid(), creditCard);
            Assert.IsTrue(string.IsNullOrEmpty(result.CardToken), "Foi gerado Token do cartão inválido.");
        }

        [TestMethod()]
        public void GetRecurrentPayment()
        {
            var customer = new Customer(name: _nome);

            var creditCard = new Card(
                cardNumber: SandboxCreditCard.Authorized2,
                holder: _nomeCartao,
                expirationDate: _validDate,
                securityCode: "123",
                brand: CardBrand.Visa,
                saveCard: false);

            var recurrentPayment = new RecurrentPayment(
                interval: Interval.Monthly,
                endDate: DateTime.Now.AddMonths(6));

            var payment = new Payment(
                amount: 150.05M,
                currency: Currency.BRL,
                installments: 1,
                softDescriptor: _descricao,
                card: creditCard,
                recurrentPayment: recurrentPayment);

            /* store order number */
            var merchantOrderId = new Random().Next();

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var result = _api.CreateTransaction(Guid.NewGuid(), transaction);

            var resultGet = _api.GetRecurrentPayment(Guid.NewGuid(), result.Payment.RecurrentPayment.RecurrentPaymentId.Value);

            Assert.IsTrue(resultGet.RecurrentPayment.GetStatus() == Status.Authorized, "Transação não foi autorizada");
            Assert.IsTrue(resultGet.RecurrentPayment.RecurrentTransactions.Count > 0, "Não foi registrado nenhuma recorrência");
        }

        [TestMethod()]
        public void GetMerchandOrderID()
        {
            var orderId = AutorizacaoCredito();

            var result = _api.GetMerchandOrderID(orderId);

            Assert.IsTrue(result.ReasonMessage == "Successful");
            Assert.IsTrue(result.Payments.Count == 1 && result.Payments[0] != null);
        }
    }
}
