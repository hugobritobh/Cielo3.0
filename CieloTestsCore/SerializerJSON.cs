using Cielo;
using System.Net.Http;

namespace CieloTest
{
    /// <summary>
    /// Classe para Serializar e Deserializar JSON.
    /// </summary>
    public class SerializerJSON : ISerializerJSON
    {
        public string Serialize<T>(T value)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value);

           // return Jil.JSON.Serialize<T>(value);
        }

        public T Deserialize<T>(HttpContent content)
        {
             return Deserialize<T>(content.ReadAsStringAsync().Result);
        }

        public T Deserialize<T>(string json)
        {
            //var json = MessagePackSerializer.Serialize(p);
            //return MessagePackSerializer.Deserialize<Transaction>(json);

            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);

            // return Jil.JSON.Deserialize<T>(json);
        }
    }
}