using System;

namespace Cielo
{

    public class FraudAnalysis
    {
        public int ReasonCode { get; set; }
        public int Score { get; set; }
        public string Status { get; set; }
        public string FactorCode { get; set; }

        public FraudStatus GetStatus()
        {
            Enum.TryParse<FraudStatus>(Status, out FraudStatus value);
            return value;
        }
    }

    public enum FraudStatus
    {
        /// <summary>
        /// Transação recebida pela Cielo.
        /// </summary>
        Started,
        /// <summary>
        /// Transação aceita após análise de fraude.
        /// </summary>
        Accept,
        /// <summary>
        /// Transação em revisão após análise de fraude.
        /// </summary>
        Review,
        /// <summary>
        /// Transação rejeitada após análise de fraude.
        /// </summary>
        Reject,
        /// <summary>
        /// Transação não finalizada por algum erro interno no sistema.
        /// </summary>
        Unfinished,
        /// <summary>
        /// Transação esperando analise
        /// </summary>
        Pendent,
        /// <summary>
        /// Transação com erro no provedor de antifraude.
        /// </summary>
        ProviderError
    }
}
