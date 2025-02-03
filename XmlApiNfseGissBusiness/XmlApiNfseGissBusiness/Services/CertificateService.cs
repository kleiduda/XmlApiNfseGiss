using Microsoft.Extensions.Configuration;
using System.Security.Cryptography.X509Certificates;
using XmlApiNfseGissBusiness.Interfaces;

namespace XmlApiNfseGissBusiness.Services
{
    public class CertificateService : ICertificateService
    {
        private readonly X509Certificate2 _certificate;

        public CertificateService(IConfiguration configuration)
        {
            var certPath = configuration["NfseSettings:CertificatePath"];
            var certPassword = configuration["NfseSettings:CertificatePassword"];

            if (string.IsNullOrEmpty(certPath) || string.IsNullOrEmpty(certPassword))
            {
                throw new ArgumentException("As configurações do certificado não foram encontradas no appsettings.json.");
            }

            if (!File.Exists(certPath))
            {
                throw new FileNotFoundException($"O certificado não foi encontrado no caminho especificado: {certPath}");
            }

            _certificate = new X509Certificate2(certPath, certPassword);

            if (!IsCertificateValid(_certificate))
            {
                throw new Exception($"O certificado digital expirou em {_certificate.NotAfter}. Atualize o certificado antes de continuar.");
            }
        }

        public X509Certificate2 GetCertificate()
        {
            return _certificate;
        }

        private bool IsCertificateValid(X509Certificate2 cert)
        {
            return cert.NotAfter > DateTime.UtcNow;
        }
    }

}
