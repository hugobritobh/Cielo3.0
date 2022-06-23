using System.Net.Http;
using System.Threading.Tasks;

namespace Cielo
{
    /// <summary>
    /// Classe para Serializar e Deserializar JSON.
    /// </summary>
    public interface ISerializerJSON
    {
        string Serialize<T>(T value);

        /// <summary>
        /// Utilizado para Deserializar os Erros
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonText"></param>
        /// <returns></returns>
        T Deserialize<T>(string jsonText);

        /// <summary>
        /// Utilizado para deserializar as requisições
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <returns></returns>
        Task<T> DeserializeAsync<T>(HttpContent content);


    }
}