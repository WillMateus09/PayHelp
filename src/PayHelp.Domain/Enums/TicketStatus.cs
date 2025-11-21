namespace PayHelp.Domain.Enums;

public enum TicketStatus
{
    Aberto = 0,
    EmAtendimento = 1,
    Encerrado = 2,
    ResolvidoPeloUsuario = 3  // Novo status para resolução via IA
}
