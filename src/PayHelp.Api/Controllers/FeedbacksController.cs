using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayHelp.Infrastructure;

namespace PayHelp.Api.Controllers;

[ApiController]
[Route("api/feedbacks")]
[Authorize(Roles = "Suporte")]
public class FeedbacksController : ControllerBase
{
    private readonly AppDbContext _db;
    public FeedbacksController(AppDbContext db) => _db = db;

    /// <summary>
    /// Lista todos os feedbacks de tickets com dados do usuário e título do ticket.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FeedbackFullDto>>> Listar()
    {
        var query = from fb in _db.TicketFeedbacks.AsNoTracking()
                    join t in _db.Tickets.AsNoTracking() on fb.TicketId equals t.Id
                    join u in _db.Users.AsNoTracking() on fb.UsuarioId equals u.Id
                    orderby fb.DataCriacaoUtc descending
                    select new FeedbackFullDto
                    {
                        Id = fb.Id,
                        TicketId = fb.TicketId,
                        TicketTitulo = t.Titulo,
                        UsuarioId = fb.UsuarioId,
                        // Fallback para email caso Nome esteja vazio
                        UsuarioNome = string.IsNullOrWhiteSpace(u.Nome) ? u.Email : u.Nome,
                        UsuarioEmail = u.Email,
                        Nota = fb.Nota,
                        Comentario = fb.Comentario,
                        DataCriacaoUtc = fb.DataCriacaoUtc
                    };
        var itens = await query.ToListAsync();
        return Ok(itens);
    }
}

public class FeedbackFullDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string TicketTitulo { get; set; } = string.Empty;
    public Guid UsuarioId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public string UsuarioEmail { get; set; } = string.Empty;
    public int? Nota { get; set; }
    public string? Comentario { get; set; }
    public DateTime DataCriacaoUtc { get; set; }
}