using System;

namespace Cielo
{
    public class Merchant
    {
        /// <summary>
        /// FIX: UTILIZAR A SUA CHAVE
        /// </summary>
        public static readonly Merchant SANDBOX = new Merchant(Guid.Parse("00000000-0000-0000-0000-000000000000"), "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        public Merchant(Guid id, string key)
        {
            this.Id = id;
            this.Key = key;
        }

        public Guid Id { get; }

        public string Key { get; }
    }
}
