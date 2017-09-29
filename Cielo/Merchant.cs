using System;

namespace Cielo
{
    public class Merchant
    {
        /// <summary>
        /// FIX: UTILIZAR A SUA CHAVE
        /// </summary>
        public static readonly Merchant SANDBOX = new Merchant(Guid.Parse("d0274285-581c-4f35-a495-3314590b6642"), "UPVHCCVVUJRLXNYGJYKMVHTEATEPTPEPQOTRBDES");

        public Merchant(Guid id, string key)
        {
            this.Id = id;
            this.Key = key;
        }

        public Guid Id { get; }

        public string Key { get; }
    }
}
