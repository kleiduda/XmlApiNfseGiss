using System.Security.Cryptography.X509Certificates;

namespace XmlApiNfseGissBusiness.Interfaces
{
    public interface ICertificateService
    {
        X509Certificate2 GetCertificate();
    }

}
