## Características
* [x] Funções Assincronas.
* [x] HttpClient.
* [x] Sem dependência a bibliotecas JSON (NewtonSoft, Jil, MessagePack...)
* [x] Testes Unitários
* [x] .Net Standard 2.0, .NET CORE 2.0, Xamarin, NET 4.6.1 ...

## Principais recursos

* [x] Pagamentos por cartão de crédito.
* [x] Pagamentos recorrentes.
    * [x] Com autorização na primeira recorrência.
    * [x] Com autorização a partir da primeira recorrência.
* [x] Pagamentos por cartão de débito.
* [x] Geração de token para o cartão para armazenamento seguro
* [x] Boleto
* [x] Transferência Eletrônica
* [x] Cancelamento de autorização.
* [x] Consulta de pagamentos.

## Implemente seu provider JSON
Você pode utilizar qualquer provider JSON. Para isso implemente a interface ISerializerJSON. Exemplo utilizando Newtonsoft:

    public class SerializerJSON : ISerializerJSON
    {
        public string Serialize<T>(T value)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value);
        }

        public T Deserialize<T>(HttpContent content)
        {
             return Deserialize<T>(content.ReadAsStringAsync().Result);
        }

        public T Deserialize<T>(string json)
        {
             return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }
    }
## Transformando Async para Sync
  Coloque no final da função o Result. Por exemplo:
    
    api.CreateTransaction(Guid.NewGuid(), transaction).Result;
    
## Adicione a sua chave da Cielo para teste
Caso queira executar o teste unitário pode deixar a chave minha do Sandbox, se deseja utilizar a sua chave altere a classe:

      public class Merchant
      {
              public static readonly Merchant SANDBOX = 
                     new Merchant(Guid.Parse("00000000-0000-0000-0000-000000000000"), "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

      }
## Documentação da Cielo
* [Visão Geral](http://developercielo.github.io/Webservice-3.0/#visão-geral---api-cielo-ecommerce)

## MIT License
Copyright (c) 2017 Hugo de Brito V. R. Alves

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

