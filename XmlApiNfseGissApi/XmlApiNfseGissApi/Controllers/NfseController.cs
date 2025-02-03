using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using XmlApiNfseGissApplication.Models;
using XmlApiNfseGissApplication.Models.Cancelamento;
using XmlApiNfseGissBusiness.Interfaces;
using XmlApiNfseGissInfra.NfseWSDL;
using Microsoft.AspNetCore.Mvc;
using XmlApiNfseGissApiBusiness.Interfaces;

namespace XmlApiNfseGiss.Controllers
{
    [ApiController]
    [Route("api/nfse")]
    public class NfseController : ControllerBase
    {
        private readonly INfseHttpService _nfseHttpService;
        private readonly INfseService _nfseService; // Serviço que gera o XML
        private readonly nfseClient _nfseClient;
        public NfseController(INfseHttpService nfseHttpService, INfseService nfseService)
        {
            _nfseHttpService = nfseHttpService;
            _nfseService = nfseService;

            var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;

            //var endpoint = new EndpointAddress("https://v2-ws-homologacao.giss.com.br/service-ws/nf/nfse-ws");
            var endpoint = new EndpointAddress("https://ws-santos.giss.com.br/service-ws/nf/nfse-ws");

            _nfseClient = new nfseClient(binding, endpoint);

            // 🔹 Carrega o certificado digital
            var cert = LoadCertificate("c:\\dados\\eudmarco2.pfx", "luis1955");

            // 🔹 Valida a expiração do certificado antes de usá-lo
            if (!IsCertificateValid(cert))
            {
                throw new Exception($"O certificado digital expirou em {cert.NotAfter}. Atualize o certificado antes de continuar.");
            }

            _nfseClient.ClientCredentials.ClientCertificate.Certificate = cert;
           
        }

        [HttpPost]
        [Route("enviar")]
        public async Task<IActionResult> EnviarNfse([FromBody] NfseRequest request)
        {
            try
            {
                // Gerar XML assinado
                string cabecalhoXml = "<cabecalho xmlns='http://www.giss.com.br/cabecalho-v2_04.xsd' xmlns:tipos='http://www.giss.com.br/tipos-v2_04.xsd' versao='2.04'><versaoDados>2.04</versaoDados></cabecalho>";
                var xmlAssinado = _nfseService.ConverterParaXml(request);

                var response = await _nfseClient.RecepcionarLoteRpsAsync(cabecalhoXml, xmlAssinado);
              

                return Ok(new { mensagem = "NFSe enviada com sucesso!", response.outputXML });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        [HttpPost]
        [Route("consultar")]
        public async Task<IActionResult> ConsultarNfse([FromBody] ConsultaNfseRequest request)
        {
            try
            {
                // Gerar XML assinado
                string cabecalhoXml = @"<p:cabecalho versao=""2.00"" xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"" 
                             xmlns:p=""http://www.giss.com.br/cabecalho-v2_04.xsd"" 
                             xmlns:p1=""http://www.giss.com.br/tipos-v2_04.xsd"" 
                             xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
                    <p:versaoDados>2.00</p:versaoDados>
                </p:cabecalho>";
                var xmlAssinado = _nfseService.ConverterParaXmlConsulta(request);

                var response = await _nfseClient.ConsultarLoteRpsAsync(cabecalhoXml, xmlAssinado);


                return Ok(new { mensagem = "Consulta NFE realizada!", response.outputXML });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        [HttpPost]
        [Route("cancelar")]
        public async Task<IActionResult> CancelarNfse([FromBody] CancelamentoNfseRequest request)
        {
            try
            {
                // Gerar XML assinado
                string cabecalhoXml = @"<p:cabecalho versao=""2.04"" 
                                        xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"" 
                                        xmlns:p=""http://www.giss.com.br/cabecalho-v2_04.xsd"" 
                                        xmlns:p1=""http://www.giss.com.br/tipos-v2_04.xsd"" 
                                        xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
                                            <p:versaoDados>2.04</p:versaoDados>
                                       </p:cabecalho>";
                var xmlAssinado = _nfseService.ConverterParaXmlCancelamento(request);

                var response = await _nfseClient.CancelarNfseAsync(cabecalhoXml, xmlAssinado);


                return Ok(new { mensagem = "Consulta NFE realizada!", response.outputXML });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        // Método para validar a expiração do certificado
        private bool IsCertificateValid(X509Certificate2 cert)
        {
            return cert != null && DateTime.Now < cert.NotAfter;
        }

        private X509Certificate2 LoadCertificate(string path, string password)
        {
            return new X509Certificate2(path, password, X509KeyStorageFlags.MachineKeySet);
        }
    }
}   
