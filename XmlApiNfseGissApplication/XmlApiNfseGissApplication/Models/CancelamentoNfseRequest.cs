using System.Text.Json.Serialization;

namespace XmlApiNfseGissApplication.Models.Cancelamento
{
    public class CancelamentoNfseRequest
    {
        [JsonPropertyName("numero_nota_fiscal")]
        public string NumeroNotaFiscal { get; set; } = string.Empty;

        [JsonPropertyName("codigo_cancelamento")]
        public int CodigoCancelamento { get; set; }

        [JsonPropertyName("cnpj")]
        public string Cnpj { get; set; } = string.Empty;

        [JsonPropertyName("inscricao_municipal")]
        public string InscricaoMunicipal { get; set; } = string.Empty;

        [JsonPropertyName("codigo_municipio")]
        public int CodigoMunicipio { get; set; }
    }
}
