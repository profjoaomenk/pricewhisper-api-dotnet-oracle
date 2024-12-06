using System.Threading.Tasks;

namespace pricewhisper.Models
{
    public interface ICNPJaService
    {
        Task<CNPJaResponse?> ConsultarCNPJ(string cnpj);
    }
}
