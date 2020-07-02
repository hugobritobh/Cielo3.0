namespace Cielo
{
    public static class SandboxCreditCard
    {
        /*
         https://developercielo.github.io/Webservice-3.0/?shell#cartão-de-crédito---sandbox
        */
        public const string Authorized1 = "4674949725897271";
        public const string Authorized2 = "4955807991560454";

        public const string NotAuthorized = "0000000000000002";
        public const string NotAuthorizedCardExpired = "0000000000000003";

        public const string NotAuthorizedCardBlocked = "0000000000000005";
        public const string NotAuthorizedTimeOut = "0000000000000006";
        public const string NotAuthorizedCardCanceled = "0000000000000007";
        public const string NotAuthorizedCardProblems = "0000000000000008";

        public const string SucessfulOrTimeout = "0000000000000009";
    }
}
