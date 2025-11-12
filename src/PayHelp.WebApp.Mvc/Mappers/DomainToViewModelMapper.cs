using PayHelp.Domain.Entities;
using PayHelp.WebApp.Mvc.ViewModels;

namespace PayHelp.WebApp.Mvc.Mappers;

public static class DomainToViewModelMapper
{
    public static TicketListItemViewModel ToListItem(Ticket t) => new()
    {
        Id = t.Id,
        Titulo = t.Titulo,
        Status = t.Status,
        CriadoEmUtc = t.CriadoEmUtc,
        EncerradoEmUtc = t.EncerradoEmUtc
    };



    public static ChatViewModel ToChat(Ticket t, string titulo, Guid currentUserId, string currentUserName, bool currentUserIsSupport = false)
    {
        return new ChatViewModel
        {
            TicketId = t.Id,
            Titulo = titulo,
            Status = t.Status,
            Mensagens = t.Mensagens
                .OrderBy(m => m.EnviadoEmUtc)
                .Select(m => new ChatMessageVM
                {
                    RemetenteUserId = m.RemetenteUserId,
                    RemetenteNome = m.Automatica
                        ? "BOT"
                        : (m.RemetenteUserId == currentUserId
                            ? currentUserName
                            : (currentUserIsSupport ? "Cliente" : "Suporte")),
                    Conteudo = m.Conteudo,
                    EnviadoEm = m.EnviadoEmUtc.ToLocalTime(),
                    Automatica = m.Automatica
                }).ToList()
        };
    }
}
