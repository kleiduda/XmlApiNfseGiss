using System.Security.Cryptography.X509Certificates;
using XmlApiNfseGissInfra.Interfaces;

namespace XmlApiNfseGissInfra.Http
{
    public class HttpNfseClientFactory : IHttpNfseClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly X509Certificate2 _certificado;

        public HttpNfseClientFactory(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;

            // 🔹 Carregar certificado do arquivo PFX 
            _certificado = new X509Certificate2("c:\\dados\\certificado3.pfx", "luis1955",
                             X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
        }

        public HttpClient CreateClient()
        {
            var handler = new HttpClientHandler
            {
                ClientCertificates = { _certificado },
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true // Ignorar erro de SSL autoassinado (se necessário)
            };

            var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            client.DefaultRequestHeaders.Add("Accept", "text/xml");
            return client;
        }
    }


}
