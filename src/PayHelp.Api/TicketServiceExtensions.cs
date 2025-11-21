using System;
using System.Threading.Tasks;
using PayHelp.Application.Services;
using PayHelp.Application.Abstractions;
using PayHelp.Domain.Entities;

namespace PayHelp.Api
{
    // Temporary shim to unblock build if the interface in a given solution configuration
    // does not expose the newer methods yet. If the concrete implementation has them,
    // we delegate; otherwise we throw a clear exception.
    internal static class TicketServiceExtensions
    {
        public static Task<Ticket> ResolverPeloUsuarioAsync(this ITicketService svc, Guid ticketId, Guid usuarioId, string? feedback, int? nota)
        {
            if (svc is TicketService concrete)
            {
                return concrete.ResolverPeloUsuarioAsync(ticketId, usuarioId, feedback, nota);
            }

            // Try reflection as a last resort
            var mi = svc.GetType().GetMethod("ResolverPeloUsuarioAsync");
            if (mi != null)
            {
                var result = mi.Invoke(svc, new object?[] { ticketId, usuarioId, feedback, nota });
                return (Task<Ticket>)result!;
            }

            throw new NotSupportedException("ITicketService.ResolverPeloUsuarioAsync não está disponível nesta configuração.");
        }

        public static Task<System.Collections.Generic.IEnumerable<PayHelp.Application.DTOs.UserFeedbackSummaryDto>> ObterFeedbackUsuariosAsync(this ITicketService svc)
        {
            if (svc is TicketService concrete)
            {
                return concrete.ObterFeedbackUsuariosAsync();
            }

            var mi = svc.GetType().GetMethod("ObterFeedbackUsuariosAsync");
            if (mi != null)
            {
                var result = mi.Invoke(svc, Array.Empty<object?>());
                return (Task<System.Collections.Generic.IEnumerable<PayHelp.Application.DTOs.UserFeedbackSummaryDto>>)result!;
            }

            throw new NotSupportedException("ITicketService.ObterFeedbackUsuariosAsync não está disponível nesta configuração.");
        }
    }
}
