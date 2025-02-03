using System.Text;
using XmlApiNfseGissInfra.Interfaces;
using XmlApiNfseGissBusiness.Interfaces;

namespace XmlApiNfseGissBusiness.Services
{
    public class NfseHttpService : INfseHttpService
    {
        private readonly IHttpNfseClientFactory _httpClientFactory;
        private const string EndpointUrl = "https://v2-ws-homologacao.giss.com.br/service-ws/nf/nfse-ws";

        public NfseHttpService(IHttpNfseClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> EnviarNfseAsync(string xmlAssinado)
        {
            var client = _httpClientFactory.CreateClient();

            // 🔹 Remover a declaração XML para evitar erro de parsing
            if (xmlAssinado.StartsWith("<?xml"))
            {
                int index = xmlAssinado.IndexOf("?>") + 2;
                xmlAssinado = xmlAssinado.Substring(index).Trim();
            }

            // Criando o envelope SOAP no formato correto
            var soapEnvelope = $@"
                                <soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:nfse='http://nfse.abrasf.org.br'>
                                    <soapenv:Header/>
                                    <soapenv:Body>
                                        <nfse:RecepcionarLoteRpsRequest>
                                            <nfseCabecMsg>
                                                <![CDATA[<cabecalho xmlns='http://www.giss.com.br/cabecalho-v2_04.xsd'
                                                        xmlns:tipos='http://www.giss.com.br/tipos-v2_04.xsd' versao='2.04'>
                                                        <versaoDados>2.04</versaoDados>
                                                </cabecalho>]]>
                                            </nfseCabecMsg>
                                            <nfseDadosMsg>
                                                <![CDATA[{xmlAssinado}]]>
                                            </nfseDadosMsg>
                                        </nfse:RecepcionarLoteRpsRequest>
                                    </soapenv:Body>
                                </soapenv:Envelope>";

            var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");

            Console.WriteLine("Enviando XML para o WebService...");
            Console.WriteLine(soapEnvelope);

            using var response = await client.PostAsync(EndpointUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Resposta do WebService:");
            Console.WriteLine(responseString);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Erro no envio da NFSe: {response.StatusCode} - {responseString}");
            }

            return responseString;
        }

    }


}
