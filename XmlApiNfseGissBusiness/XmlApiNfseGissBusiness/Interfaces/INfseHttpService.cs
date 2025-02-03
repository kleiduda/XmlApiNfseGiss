namespace XmlApiNfseGissBusiness.Interfaces
{
    public interface INfseHttpService
    {
        Task<string> EnviarNfseAsync(string xmlAssinado);
    }

}
