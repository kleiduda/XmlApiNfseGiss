##### JSON RECEPCIONAR #####
{
    "numero_lote":"numero do lote que sera criado",
    "rps": {
      "numero": "numero do RPS",
      "serie": "E",
      "tipo": "1",
      "data_emissao": "2025-02-01",
      "status": "1"
    },
    "competencia": "2025-02-01",
    "servico": {
      "valor_servicos": "1.17",
      "valor_pis": "0.00",
      "valor_cofins": "0.00",
      "valor_ir": "0.00",
      "valor_csll": "0.00",
      "valor_outras_retencoes":"0.00",
      "valor_iss": "0.06",
      "valor_inss": "0.00",
      "aliquota": 0.05,
      "iss_retido": "2",
      "responsavel_retencao": "1",
      "item_lista_servico": "10.01",
      "codigo_tributacao_municipio": "521170101",
      "discriminacao": "Item:1 - INSP. NAO INVASIVA DE CNTR FCL - SCANNER - R$ 121 Item:2 - ARMAZENAGEM - R$ 847 Item:3",
      "codigo_municipio": "codigo do municipio",
      "codigo_pais": "0076",
      "exigibilidade_iss": "1",
      "municipio_incidencia": "3548500"
    },
    "prestador": {
      "cnpj": "cnpj do certificado",
      "inscricao_municipal": "inscricao municipal do prestador"
    },
    "tomadorServico": {
      "cnpj": "000000000000000",
      "razao_social": "",
      "inscricao_municipal":"isento",
      "endereco": {
        "logradouro": "",
        "numero": "",
        "complemento": "",
        "bairro": "",
        "codigo_municipio": "",
        "uf": "SP",
        "cep": ""
      },
      "contato": {
        "telefone": ""
      },
      "optante_simples_nacional":"2",
      "incentivo_fiscal":"2"
    }
  }

  #### JSON CONSULTA #####
  {
  "numero_lote": "numero do lote da recepcao",
  "cnpj": "cnpj do prestador",
  "inscricao_municipal": "1831854",
  "protocolo": "protocolo retornado na recepcao"
  }

  #### JSON CANCELEMENTO #####
  {
  "numero_nota_fiscal": "263593",
  "codigo_cancelamento": 2,
  "cnpj": "cnpj do prestador",
  "inscricao_municipal": "1831854",
  "codigo_municipio": 3548500
}
  
