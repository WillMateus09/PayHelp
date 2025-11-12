using PayHelp.Domain.Entities;

namespace PayHelp.Application.Abstractions;

public interface IFaqService
{
    Task<FaqEntry> RegistrarAsync(Guid ticketId, string tituloProblema, string descricaoProblema, string solucao);
    Task<IEnumerable<FaqEntry>> BuscarSimilarAsync(string texto, double limiarSimilaridade = 0.7, int max = 5);
    Task<FaqEntry?> ObterPorTicketAsync(Guid ticketId);
}
