using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using XmlApiNfseGissApiBusiness.Interfaces;
using XmlApiNfseGissApplication.Models;
using XmlApiNfseGissApplication.Models.Cancelamento;
using XmlApiNfseGissBusiness.Interfaces;

namespace XmlApiNfseGissBusiness.Services
{
    public class NfseService : INfseService
    {
        private const string NamespaceGissTipos = "http://www.giss.com.br/tipos-v2_04.xsd";
        private const string NamespaceGissEnvio = "http://www.giss.com.br/enviar-lote-rps-envio-v2_04.xsd";
        private const string NamespaceXmlDsig = "http://www.w3.org/2000/09/xmldsig#";

        private readonly ICertificateService _certificateService;

        public NfseService(ICertificateService certificateService)
        {
            _certificateService = certificateService;
        }
        public string ConverterParaXml(NfseRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "A requisição não pode ser nula.");

            var xmlDoc = new XmlDocument();
            var xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            xmlDoc.AppendChild(xmlDeclaration);

            // Criando <ns4:EnviarLoteRpsEnvio>
            var enviarLoteRpsEnvio = xmlDoc.CreateElement("ns4", "EnviarLoteRpsEnvio", NamespaceGissEnvio);
            enviarLoteRpsEnvio.SetAttribute("xmlns:ns2", NamespaceGissTipos);
            enviarLoteRpsEnvio.SetAttribute("xmlns:ns4", NamespaceGissEnvio);
            enviarLoteRpsEnvio.SetAttribute("xmlns:nss03", NamespaceXmlDsig);
            xmlDoc.AppendChild(enviarLoteRpsEnvio);

            // Criando <ns4:LoteRps>
            var loteRps = xmlDoc.CreateElement("ns4", "LoteRps", NamespaceGissEnvio);
            loteRps.SetAttribute("Id", $"LOTERPS_{request.NumeroLote}");
            loteRps.SetAttribute("versao", "2.04");
            enviarLoteRpsEnvio.AppendChild(loteRps);

            // Criando <ns2:NumeroLote>
            AdicionarElementoComNamespace(xmlDoc, loteRps, "ns2", "NumeroLote", request.NumeroLote, NamespaceGissTipos);

            // Criando <ns2:Prestador>
            var prestadorElement = xmlDoc.CreateElement("ns2", "Prestador", NamespaceGissTipos);
            var cpfCnpjElement = xmlDoc.CreateElement("ns2", "CpfCnpj", NamespaceGissTipos);
            AdicionarElementoComNamespace(xmlDoc, cpfCnpjElement, "ns2", "Cnpj", request.Prestador.Cnpj, NamespaceGissTipos);
            prestadorElement.AppendChild(cpfCnpjElement);
            AdicionarElementoComNamespace(xmlDoc, prestadorElement, "ns2", "InscricaoMunicipal", request.Prestador.InscricaoMunicipal, NamespaceGissTipos);
            loteRps.AppendChild(prestadorElement);

            // Criando <ns2:QuantidadeRps>
            AdicionarElementoComNamespace(xmlDoc, loteRps, "ns2", "QuantidadeRps", "1", NamespaceGissTipos);

            // Criando <ns2:ListaRps>
            var listaRpsElement = xmlDoc.CreateElement("ns2", "ListaRps", NamespaceGissTipos);
            loteRps.AppendChild(listaRpsElement);

            // Criando e adicionando <InfDeclaracaoPrestacaoServico>
            var infDeclaracao = MontaInformacaoDeclaracao(xmlDoc, request);

            // Importar infDeclaracao para garantir que pertence a xmlDoc
            infDeclaracao = (XmlElement)xmlDoc.ImportNode(infDeclaracao, true);

            // Criando <ns2:Rps> e adicionando <InfDeclaracaoPrestacaoServico>
            var rpsElement = xmlDoc.CreateElement("ns2", "Rps", NamespaceGissTipos);
            rpsElement.AppendChild(infDeclaracao);

            // Adicionar <ns2:Rps> dentro de <ns2:ListaRps> ANTES de assinar
            listaRpsElement.AppendChild(rpsElement);

            Console.WriteLine("XML antes da assinatura:");
            Console.WriteLine(xmlDoc.OuterXml);

            

            // Assinar <ns2:InfDeclaracaoPrestacaoServico>
            XmlElement assinatura = AssinarXml(xmlDoc, infDeclaracao);

            // Adicionar a assinatura gerada dentro de <ns2:Rps>
            rpsElement.AppendChild(xmlDoc.ImportNode(assinatura, true));

            Console.WriteLine("XML depois da 1 assinatura:");
            Console.WriteLine(xmlDoc.OuterXml);

            // Assinar <ns4:LoteRps>
            XmlElement assinaturaLoteRps = AssinarXml(xmlDoc, loteRps);

            // 🔹 Adicionar a assinatura **APÓS** <ns4:LoteRps> e não dentro
            enviarLoteRpsEnvio.InsertAfter(xmlDoc.ImportNode(assinaturaLoteRps, true), loteRps);


            Console.WriteLine("XML final após assinaturas:");
            Console.WriteLine(xmlDoc.OuterXml);

            // Retornar XML formatado
            return xmlDoc.OuterXml;
        }

        public string ConverterParaXmlConsulta(ConsultaNfseRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "A requisição não pode ser nula.");

            var xmlDoc = new XmlDocument();
            var xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            xmlDoc.AppendChild(xmlDeclaration);

            // Criando o elemento raiz <con:ConsultarLoteRpsEnvio>
            var rootElement = xmlDoc.CreateElement("con", "ConsultarLoteRpsEnvio", "http://www.giss.com.br/consultar-lote-rps-envio-v2_04.xsd");
            //rootElement.SetAttribute("Id", $"LOTERPS_{request.NumeroLote}");
            rootElement.SetAttribute("xmlns:tip", "http://www.giss.com.br/tipos-v2_04.xsd");
            xmlDoc.AppendChild(rootElement);

            // Criando <con:Prestador>
            var prestadorElement = xmlDoc.CreateElement("con", "Prestador", "http://www.giss.com.br/consultar-lote-rps-envio-v2_04.xsd");

            // Criando <tip:CpfCnpj>
            var cpfCnpjElement = xmlDoc.CreateElement("tip", "CpfCnpj", "http://www.giss.com.br/tipos-v2_04.xsd");
            var cnpjElement = xmlDoc.CreateElement("tip", "Cnpj", "http://www.giss.com.br/tipos-v2_04.xsd");
            cnpjElement.InnerText = request.Cnpj;
            cpfCnpjElement.AppendChild(cnpjElement);
            prestadorElement.AppendChild(cpfCnpjElement);

            // Criando <tip:InscricaoMunicipal>
            var inscricaoMunicipalElement = xmlDoc.CreateElement("tip", "InscricaoMunicipal", "http://www.giss.com.br/tipos-v2_04.xsd");
            inscricaoMunicipalElement.InnerText = request.InscricaoMunicipal;
            prestadorElement.AppendChild(inscricaoMunicipalElement);

            rootElement.AppendChild(prestadorElement);

            // Criando <con:Protocolo>
            var protocoloElement = xmlDoc.CreateElement("con", "Protocolo", "http://www.giss.com.br/consultar-lote-rps-envio-v2_04.xsd");
            protocoloElement.InnerText = request.Protocolo;
            rootElement.AppendChild(protocoloElement);

            //Assinar XML
            // Assinar <ns2:InfDeclaracaoPrestacaoServico>
            XmlElement assinatura = AssinarXmlConsulta(xmlDoc, rootElement);

            // Adicionar a assinatura gerada dentro de <ns2:Rps>
            rootElement.AppendChild(xmlDoc.ImportNode(assinatura, true));

            return xmlDoc.OuterXml;
        }

        public string ConverterParaXmlCancelamento(CancelamentoNfseRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "A requisição não pode ser nula.");

            var xmlDoc = new XmlDocument();

            // Declaração XML: Definindo a versão do XML, o tipo de codificação e o fato de ser standalone.
            var xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            xmlDoc.AppendChild(xmlDeclaration);

            // Criando o elemento raiz <can:CancelarNfseEnvio>
            var rootElement = xmlDoc.CreateElement("can", "CancelarNfseEnvio", "http://www.giss.com.br/cancelar-nfse-envio-v2_04.xsd");
            rootElement.SetAttribute("xmlns:tip", "http://www.giss.com.br/tipos-v2_04.xsd");
            rootElement.SetAttribute("xmlns:xd", "http://www.w3.org/2000/09/xmldsig#");
            xmlDoc.AppendChild(rootElement);

            // Criando o elemento <can:Pedido>
            var pedidoElement = xmlDoc.CreateElement("can", "Pedido", "http://www.giss.com.br/cancelar-nfse-envio-v2_04.xsd");

            // Criando o elemento <tip:InfPedidoCancelamento> com um atributo "Id"
            var infPedidoCancelamentoElement = xmlDoc.CreateElement("tip", "InfPedidoCancelamento", "http://www.giss.com.br/tipos-v2_04.xsd");
            infPedidoCancelamentoElement.SetAttribute("Id", "N_" + request.NumeroNotaFiscal);  // Defina o valor correto para o atributo "Id"

            // Adicionando a Identificação da NFSe
            var identificacaoNfseElement = xmlDoc.CreateElement("tip", "IdentificacaoNfse", "http://www.giss.com.br/tipos-v2_04.xsd");
            var numeroElement = xmlDoc.CreateElement("tip", "Numero", "http://www.giss.com.br/tipos-v2_04.xsd");
            numeroElement.InnerText = request.NumeroNotaFiscal;  // O número da NFSe
            identificacaoNfseElement.AppendChild(numeroElement);

            var cpfCnpjElement = xmlDoc.CreateElement("tip", "CpfCnpj", "http://www.giss.com.br/tipos-v2_04.xsd");
            var cnpjElement = xmlDoc.CreateElement("tip", "Cnpj", "http://www.giss.com.br/tipos-v2_04.xsd");
            cnpjElement.InnerText = request.Cnpj;  
            cpfCnpjElement.AppendChild(cnpjElement);
            identificacaoNfseElement.AppendChild(cpfCnpjElement);

            // Caso opcional: Inscrição Municipal
            if (!string.IsNullOrEmpty(request.InscricaoMunicipal))
            {
                var inscricaoMunicipalElement = xmlDoc.CreateElement("tip", "InscricaoMunicipal", "http://www.giss.com.br/tipos-v2_04.xsd");
                inscricaoMunicipalElement.InnerText = request.InscricaoMunicipal;
                identificacaoNfseElement.AppendChild(inscricaoMunicipalElement);
            }

            var codigoMunicipioElement = xmlDoc.CreateElement("tip", "CodigoMunicipio", "http://www.giss.com.br/tipos-v2_04.xsd");
            codigoMunicipioElement.InnerText = request.CodigoMunicipio.ToString();  // Código do município
            identificacaoNfseElement.AppendChild(codigoMunicipioElement);

            infPedidoCancelamentoElement.AppendChild(identificacaoNfseElement);

            // Adicionando o Código de Cancelamento
              //Código de cancelamento com base na tabela de Erros e alertas.
              //1 – Erro na emissão
              //2 – Serviço não prestado
              //3 – Erro de assinatura
              //4 – Duplicidade da nota
              //5 – Erro de processamento

            var codigoCancelamentoElement = xmlDoc.CreateElement("tip", "CodigoCancelamento", "http://www.giss.com.br/tipos-v2_04.xsd");
            codigoCancelamentoElement.InnerText = request.CodigoCancelamento.ToString();
            infPedidoCancelamentoElement.AppendChild(codigoCancelamentoElement);

            pedidoElement.AppendChild(infPedidoCancelamentoElement);

            rootElement.AppendChild(pedidoElement);

            //Assinar XML
            // Assinar <tip:InfPedidoCancelamento>
            XmlElement assinatura = AssinarXml(xmlDoc, infPedidoCancelamentoElement);

            // Adicionar a assinatura gerada após o <tip:InfPedidoCancelamento> e antes do próximo elemento (se houver) dentro de <can:Pedido>
            pedidoElement.InsertAfter(xmlDoc.ImportNode(assinatura, true), infPedidoCancelamentoElement);

            // Retornar o XML gerado
            return xmlDoc.OuterXml;
        }

        public XmlElement MontaInformacaoDeclaracao(XmlDocument xmlDoc, NfseRequest request)
        {
            var infDeclaracao = xmlDoc.CreateElement("ns2", "InfDeclaracaoPrestacaoServico", NamespaceGissTipos);
            infDeclaracao.SetAttribute("Id", $"INFRPS_{request.Rps.Numero}");

            // Criando <ns2:Rps>
            var rpsElement = xmlDoc.CreateElement("ns2", "Rps", NamespaceGissTipos);
            var identificacaoRps = xmlDoc.CreateElement("ns2", "IdentificacaoRps", NamespaceGissTipos);
            AdicionarElementoComNamespace(xmlDoc, identificacaoRps, "ns2", "Numero", request.Rps.Numero, NamespaceGissTipos);
            AdicionarElementoComNamespace(xmlDoc, identificacaoRps, "ns2", "Serie", request.Rps.Serie, NamespaceGissTipos);
            AdicionarElementoComNamespace(xmlDoc, identificacaoRps, "ns2", "Tipo", request.Rps.Tipo, NamespaceGissTipos);
            rpsElement.AppendChild(identificacaoRps);
            AdicionarElementoComNamespace(xmlDoc, rpsElement, "ns2", "DataEmissao", request.Rps.DataEmissao.ToString("yyyy-MM-dd"), NamespaceGissTipos);
            AdicionarElementoComNamespace(xmlDoc, rpsElement, "ns2", "Status", request.Rps.Status, NamespaceGissTipos);
            infDeclaracao.AppendChild(rpsElement);

            // Competência
            AdicionarElementoComNamespace(xmlDoc, infDeclaracao, "ns2", "Competencia", request.Competencia.ToString("yyyy-MM-dd"), NamespaceGissTipos);

            // Criando <ns2:Servico>
            var servicoElement = xmlDoc.CreateElement("ns2", "Servico", NamespaceGissTipos);
            var valoresElement = xmlDoc.CreateElement("ns2", "Valores", NamespaceGissTipos);
            AdicionarElementoComNamespace(xmlDoc, valoresElement, "ns2", "ValorServicos", request.Servico.ValorServicos.ToString("F2", CultureInfo.InvariantCulture), NamespaceGissTipos);
            AdicionarElementoComNamespace(xmlDoc, valoresElement, "ns2", "ValorDeducoes", "0.00", NamespaceGissTipos);
            AdicionarElementoComNamespace(xmlDoc, valoresElement, "ns2", "ValorPis", request.Servico.ValorPis.ToString("F2", CultureInfo.InvariantCulture), NamespaceGissTipos);
            AdicionarElementoComNamespace(xmlDoc, valoresElement, "ns2", "ValorCofins", request.Servico.ValorCofins.ToString("F2", CultureInfo.InvariantCulture), NamespaceGissTipos);
            AdicionarElementoComNamespace(xmlDoc, valoresElement, "ns2", "ValorInss", request.Servico.ValorInss.ToString("F2", CultureInfo.InvariantCulture), NamespaceGissTipos);
            AdicionarElementoComNamespace(xmlDoc, valoresElement, "ns2", "ValorIr", request.Servico.ValorIr.ToString("F2", CultureInfo.InvariantCulture), NamespaceGissTipos);
            AdicionarElementoComNamespace(xmlDoc, valoresElement, "ns2", "ValorCsll", request.Servico.ValorCsll.ToString("F2", CultureInfo.InvariantCulture), NamespaceGissTipos);
            AdicionarElementoComNamespace(xmlDoc, valoresElement, "ns2", "OutrasRetencoes", "0.00", NamespaceGissTipos);
            AdicionarElementoComNamespace(xmlDoc, valoresElement, "ns2", "ValorIss", "123.29", NamespaceGissTipos);
            AdicionarElementoComNamespace(xmlDoc, valoresElement, "ns2", "Aliquota", request.Servico.Aliquota.ToString("F2", CultureInfo.InvariantCulture), NamespaceGissTipos);
            servicoElement.AppendChild(valoresElement);
            AdicionarElementoComNamespace(xmlDoc, servicoElement, "ns2", "IssRetido", request.Servico.IssRetido, NamespaceGissTipos);
            AdicionarElementoComNamespace(xmlDoc, servicoElement, "ns2", "ItemListaServico", request.Servico.ItemListaServico, NamespaceGissTipos);
            AdicionarElementoComNamespace(xmlDoc, servicoElement, "ns2", "CodigoTributacaoMunicipio", request.Servico.CodigoTributacaoMunicipio, NamespaceGissTipos);
            AdicionarElementoComNamespace(xmlDoc, servicoElement, "ns2", "Discriminacao", request.Servico.Discriminacao, NamespaceGissTipos);
            AdicionarElementoComNamespace(xmlDoc, servicoElement, "ns2", "CodigoMunicipio", request.Servico.CodigoMunicipio, NamespaceGissTipos);
            AdicionarElementoComNamespace(xmlDoc, servicoElement, "ns2", "CodigoPais", request.Servico.CodigoPais, NamespaceGissTipos);
            AdicionarElementoComNamespace(xmlDoc, servicoElement, "ns2", "ExigibilidadeISS", request.Servico.ExigibilidadeIss, NamespaceGissTipos);
            AdicionarElementoComNamespace(xmlDoc, servicoElement, "ns2", "MunicipioIncidencia", request.Servico.MunicipioIncidencia, NamespaceGissTipos);
            infDeclaracao.AppendChild(servicoElement);

            // Criando <ns2:Prestador>
            var prestadorElement = xmlDoc.CreateElement("ns2", "Prestador", NamespaceGissTipos);
            var cpfCnpjElement = xmlDoc.CreateElement("ns2", "CpfCnpj", NamespaceGissTipos);
            AdicionarElementoComNamespace(xmlDoc, cpfCnpjElement, "ns2", "Cnpj", request.Prestador.Cnpj, NamespaceGissTipos);
            prestadorElement.AppendChild(cpfCnpjElement);
            AdicionarElementoComNamespace(xmlDoc, prestadorElement, "ns2", "InscricaoMunicipal", request.Prestador.InscricaoMunicipal, NamespaceGissTipos);
            infDeclaracao.AppendChild(prestadorElement);

            // Criando <ns2:TomadorServico>
            var tomadorElement = xmlDoc.CreateElement("ns2", "TomadorServico", NamespaceGissTipos);
            var identificacaoTomador = xmlDoc.CreateElement("ns2", "IdentificacaoTomador", NamespaceGissTipos);
            var cpfCnpjTomador = xmlDoc.CreateElement("ns2", "CpfCnpj", NamespaceGissTipos);
            AdicionarElementoComNamespace(xmlDoc, cpfCnpjTomador, "ns2", "Cnpj", request.TomadorServico.Cnpj, NamespaceGissTipos);
            identificacaoTomador.AppendChild(cpfCnpjTomador);

            AdicionarElementoComNamespace(xmlDoc, identificacaoTomador, "ns2", "InscricaoMunicipal", request.TomadorServico.InscricaoMunicipal, NamespaceGissTipos);
            tomadorElement.AppendChild(identificacaoTomador);
            AdicionarElementoComNamespace(xmlDoc, tomadorElement, "ns2", "RazaoSocial", request.TomadorServico.RazaoSocial, NamespaceGissTipos);
            infDeclaracao.AppendChild(tomadorElement);

            // Criando <ns2:OptanteSimplesNacional> e <ns2:IncentivoFiscal>
            AdicionarElementoComNamespace(xmlDoc, infDeclaracao, "ns2", "OptanteSimplesNacional", request.TomadorServico.OptanteSimplesNacional, NamespaceGissTipos);
            AdicionarElementoComNamespace(xmlDoc, infDeclaracao, "ns2", "IncentivoFiscal", request.TomadorServico.IncentivoFiscal, NamespaceGissTipos);

            Console.WriteLine("XML gerado dentro de MontaInformacaoDeclaracao:");
            Console.WriteLine(infDeclaracao.OuterXml);

            return infDeclaracao;
        }

        public XmlElement AssinarXml(XmlDocument xmlDoc, XmlElement elementoAssinar)
        {
            // Carregar Certificado
            var certificado = _certificateService.GetCertificate();

            if (!elementoAssinar.HasAttribute("Id"))
            {
                throw new CryptographicException("O elemento a ser assinado não possui um atributo 'Id'.");
            }

            string idElemento = elementoAssinar.GetAttribute("Id");

            SignedXml signedXml = new SignedXml(xmlDoc);
            signedXml.SigningKey = certificado.GetRSAPrivateKey();

            Reference reference = new Reference
            {
                Uri = $"#{idElemento}",
                DigestMethod = SignedXml.XmlDsigSHA1Url
            };

            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(new XmlDsigC14NTransform());

            signedXml.AddReference(reference);

            KeyInfo keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(certificado));
            signedXml.KeyInfo = keyInfo;

            signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA1Url;

            signedXml.ComputeSignature();

            return signedXml.GetXml();
        }

        public XmlElement AssinarXmlConsulta(XmlDocument xmlDoc, XmlElement elementoAssinar)
        {
            // Carregar Certificado
            var certificado = _certificateService.GetCertificate();

            // Verificando se o elemento não possui atributo 'Id', pois não será necessário
            if (elementoAssinar == null)
            {
                throw new CryptographicException("O elemento a ser assinado não pode ser nulo.");
            }

            SignedXml signedXml = new SignedXml(xmlDoc);
            signedXml.SigningKey = certificado.GetRSAPrivateKey();

            // Definindo a URI como vazia para assinar o conteúdo do elemento diretamente
            Reference reference = new Reference
            {
                Uri = "",  // URI vazia, indicando que será assinado o conteúdo do próprio elemento
                DigestMethod = SignedXml.XmlDsigSHA1Url
            };

            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(new XmlDsigC14NTransform());

            signedXml.AddReference(reference);

            KeyInfo keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(certificado));
            signedXml.KeyInfo = keyInfo;

            signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA1Url;

            signedXml.ComputeSignature();

            return signedXml.GetXml();
        }

        private void AdicionarElementoComNamespace(XmlDocument doc, XmlElement parent, string prefixo, string nome, string valor, string ns)
        {
            var elemento = doc.CreateElement(prefixo, nome, ns);
            elemento.InnerText = valor;
            parent.AppendChild(elemento);
        }
 
    }
}
