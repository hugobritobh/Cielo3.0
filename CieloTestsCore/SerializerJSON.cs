using Cielo;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace CieloTest
{
    /// <summary>
    /// Classe para Serializar e Deserializar JSON.
    /// </summary>
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
}