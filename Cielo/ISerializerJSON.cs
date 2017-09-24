using System.Net.Http;

namespace Cielo
{
    /// <summary>
    /// Classe para Serializar e Deserializar JSON.
    /// </summary>
    public interface ISerializerJSON
    {
        string Serialize<T>(T value);

        T Deserialize<T>(HttpContent content);

        T Deserialize<T>(string json);
    }
}