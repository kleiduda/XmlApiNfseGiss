using System.Text.Json.Serialization;

namespace XmlApiNfseGissApplication.Models
{
    public class ConsultaNfseRequest
    {
        [JsonPropertyName("numero_lote")]
        public string NumeroLote { get; set; } = string.Empty;

        [JsonPropertyName("cnpj")]
        public string Cnpj { get; set; } = string.Empty ;

        [JsonPropertyName("inscricao_municipal")]
        public string InscricaoMunicipal { get; set; } = string.Empty;

        [JsonPropertyName("protocolo")]
        public string Protocolo { get; set; } = string.Empty;
    }
}
