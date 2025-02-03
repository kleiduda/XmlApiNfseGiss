using XmlApiNfseGissApplication.Models;
using XmlApiNfseGissApplication.Models.Cancelamento;

namespace XmlApiNfseGissApiBusiness.Interfaces
{
    public interface INfseService
    {
        string ConverterParaXml(NfseRequest request);
        string ConverterParaXmlConsulta(ConsultaNfseRequest request);
        string ConverterParaXmlCancelamento(CancelamentoNfseRequest request);
    }
}
