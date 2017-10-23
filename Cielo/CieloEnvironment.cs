namespace Cielo
{
    public struct CieloEnvironment
    {
        public static readonly CieloEnvironment PRODUCTION = new CieloEnvironment("https://api.cieloecommerce.cielo.com.br", "https://apiquery.cieloecommerce.cielo.com.br");

        public static readonly CieloEnvironment SANDBOX = new CieloEnvironment("https://apisandbox.cieloecommerce.cielo.com.br", "https://apiquerysandbox.cieloecommerce.cielo.com.br");
       
        private readonly string _transactionUrl;
        private readonly string _queryUrl;

        public CieloEnvironment(string transactionUrl, string queryUrl)
        {
            _transactionUrl = transactionUrl;
            _queryUrl = queryUrl;
        }

        public string GetTransactionUrl(string path) => _transactionUrl + path;


        public string GetQueryUrl(string path) => _queryUrl + path;
    }
}
