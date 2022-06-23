## Características
* [x] Funções Assincronas.
* [x] HttpClient.
* [x] Sem dependência a bibliotecas JSON (NewtonSoft, Jil, MessagePack...)
* [x] Testes Unitários
* [x] .Net Standard 2

## Principais recursos

* [x] Pagamentos por cartão de crédito.
* [x] Pagamentos por cartão de débito.
* [x] Pagamentos recorrentes (de acordo com a Cielo funciona somente para crédito)
    * [x] Com autorização na primeira recorrência ou a partir de uma data.
* [x] Boleto
* [x] Transferência Eletrônica
* [x] Cancelamento de autorização.
* [x] Consulta de pagamentos.
* [x] Tokenização do Cartão 
    * [x] GetRecurrentPayment - Busca informações sobre a recorrência
    * [x] CreateToken - Tokeniza o cartão válido (ou inválido).
    * [x] CreateTokenValid - Gera um pagamento de 1 real (cancela logo em seguida) para garantir que o cartão é válido e retorna a Token.

## Como utilizar
Como não tem uma documentação formal utilize o projeto de Teste (CieloTestsCore) para ver os exemplos criados (e verificar se ainda estão funcionando).

## Implemente seu provider JSON
Você pode utilizar qualquer provider JSON. Para isso implemente a interface ISerializerJSON. Exemplo utilizando Newtonsoft:

    public class SerializerJSON : ISerializerJSON
    {
        public string Serialize<T>(T value)
        {
            // return System.Text.Json.JsonSerializer.Serialize<T>(value);

            return Newtonsoft.Json.JsonConvert.SerializeObject(value);
        }

        public T Deserialize<T>(string jsonText)
        {
            //var json = MessagePackSerializer.Serialize(p);
            //return MessagePackSerializer.Deserialize<Transaction>(jsonText);

            // return Jil.JSON.Deserialize<T>(jsonText);

            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonText);
        }

        public async Task<T> DeserializeAsync<T>(HttpContent content)
        {
            // return await System.Text.Json.JsonSerializer.DeserializeAsync<T>(await content.ReadAsStreamAsync(), _jsonOptions).ConfigureAwait(false);

            var serializer = new JsonSerializer();
            using (var textReader = new StreamReader(await content.ReadAsStreamAsync().ConfigureAwait(false)))
            using (var jsonReader = new JsonTextReader(textReader))
            {
                return serializer.Deserialize<T>(jsonReader);
            }
        }
    }
    
## Métodos Async e Sync
  Método com o Sufixo Async será executado de forma assincrona.
    
       await api.CreateTransactionAsync(Guid.NewGuid(), transaction);

   Quando não tiver o sufixo Async será executado de forma sincrona. 
   
       api.CreateTransaction(Guid.NewGuid(), transaction));
    
## Chave do Sandbox
Caso queira executar o teste unitário pode deixar a minha chave do Sandbox, se deseja utilizar a sua altere a classe:

      public class Merchant
      {
              public static readonly Merchant SANDBOX = 
                     new Merchant(Guid.Parse("00000000-0000-0000-0000-000000000000"), "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

      }
     
## Nuget
Install-Package CieloRest -Version 1.0.16
      
## Documentação da Cielo
* [Visão Geral](http://developercielo.github.io/Webservice-3.0/#visão-geral---api-cielo-ecommerce)

* [Github](https://github.com/DeveloperCielo/Webservice-3.0/blob/57e2c5f3a3fc595b4693d286a2c47129bf5f388d/source/index.md)

## Exemplo de utilização (utilize os casos de Teste para ver mais exemplos)

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

            var transaction = new Transaction(
                merchantOrderId: Random().Next().ToString(),
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

## MIT License
Copyright (c) 2019 Hugo de Brito V. R. Alves

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

