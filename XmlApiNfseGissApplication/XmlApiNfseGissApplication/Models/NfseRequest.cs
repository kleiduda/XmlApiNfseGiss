using System.Text.Json.Serialization;

namespace XmlApiNfseGissApplication.Models
{
    public class NfseRequest
    {
        [JsonPropertyName("numero_lote")]
        public string NumeroLote { get; set; } = string.Empty;
        public Rps Rps { get; set; }
        public DateTime Competencia { get; set; }
        public Servico Servico { get; set; }
        public Prestador Prestador { get; set; }
        public TomadorServico TomadorServico { get; set; }
    }

    public class Rps
    {
        public string Numero { get; set; } = string.Empty;
        public string Serie { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        [JsonPropertyName("data_emissao")]
        public DateTime DataEmissao { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class Servico
    {
        [JsonPropertyName("valor_servicos")]
        public decimal ValorServicos { get; set; }
        [JsonPropertyName("valor_pis")]
        public decimal ValorPis { get; set; }
        [JsonPropertyName("valor_cofins")]
        public decimal ValorCofins { get; set; }
        [JsonPropertyName("valor_ir")]
        public decimal ValorIr { get; set; }
        [JsonPropertyName("valor_csll")]
        public decimal ValorCsll { get; set; }

        [JsonPropertyName("valor_outras_retencoes")]
        public decimal ValorOutrasRetencoes { get; set; }

        [JsonPropertyName("valor_iss")]
        public decimal ValorIss { get; set; }

        [JsonPropertyName("valor_inss")]
        public decimal ValorInss { get; set; }

        public decimal Aliquota { get; set; }

        [JsonPropertyName("iss_retido")]
        public string IssRetido { get; set; } = string.Empty;

        [JsonPropertyName("responsavel_retencao")]
        public string ResponsavelRetencao { get; set; } = string.Empty ;

        [JsonPropertyName("item_lista_servico")]
        public string ItemListaServico { get; set; } = string.Empty;

        [JsonPropertyName("codigo_tributacao_municipio")]
        public string CodigoTributacaoMunicipio { get; set; } = string.Empty;

        public string Discriminacao { get; set; } = string.Empty;

        [JsonPropertyName("codigo_municipio")]
        public string CodigoMunicipio { get; set; } = string.Empty;

        [JsonPropertyName("codigo_pais")]
        public string CodigoPais { get; set; } = string.Empty;

        [JsonPropertyName("exigibilidade_iss")]
        public string ExigibilidadeIss { get; set; } = string.Empty;

        [JsonPropertyName("municipio_incidencia")]
        public string MunicipioIncidencia { get; set; } = string.Empty;

    }

    public class Prestador
    {
        public string Cnpj { get; set; }
        [JsonPropertyName("inscricao_municipal")]
        public string InscricaoMunicipal { get; set; }
    }

    public class TomadorServico
    {
        public string Cnpj { get; set; } = string.Empty;

        [JsonPropertyName("razao_social")]
        public string RazaoSocial { get; set; } = string.Empty;

        [JsonPropertyName("inscricao_municipal")]
        public string InscricaoMunicipal { get; set; } = string.Empty;

        public Endereco Endereco { get; set; }
        public Contato Contato { get; set; }

        [JsonPropertyName("optante_simples_nacional")]
        public string OptanteSimplesNacional { get; set; } = string.Empty;

        [JsonPropertyName("incentivo_fiscal")]
        public string IncentivoFiscal { get; set; }
    }

    public class Endereco
    {
        public string Logradouro { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Complemento { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;

        [JsonPropertyName("codigo_municipio")]
        public string CodigoMunicipio { get; set; } = string.Empty;
        public string Uf { get; set; } = string.Empty;
        public string Cep { get; set; } = string.Empty;
    }

    public class Contato
    {
        public string Telefone { get; set; } = string.Empty;
    }
}
