using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using PayHelp.Infrastructure;
using PayHelp.Domain.Security;
using PayHelp.Domain.Entities;

namespace PayHelp.Api.Controllers;

[ApiController]
[Route("dev")]
[AllowAnonymous]
public class DevController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly AppDbContext _db;
    public DevController(IWebHostEnvironment env, AppDbContext db)
    {
        _env = env; _db = db;
    }

    private ActionResult DevOnly()
        => Problem("Endpoint permitido apenas em Development.", statusCode: 404);

    public record ResetPasswordRequest(string Email, string NovaSenha);
    public record PurgeTicketsRequest(bool All = false, string? Contains = null, string? UserEmail = null, DateTime? BeforeUtc = null);


    [HttpGet("users")]
    public async Task<ActionResult> GetUsers([FromQuery] string? email = null)
    {
        if (!_env.IsDevelopment()) return DevOnly();
        IQueryable<User> q = _db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(email))
        {
            var em = email.Trim().ToLower();
            q = q.Where(u => u.Email.ToLower() == em);
        }
        var list = await q.Select(u => new { u.Id, u.Email, u.Nome, Role = u.Role.ToString() }).ToListAsync();
        return Ok(list);
    }

    [HttpPost("reset-db")] 
    public async Task<ActionResult> ResetDb()
    {
        if (!_env.IsDevelopment()) return DevOnly();
        await _db.Database.EnsureDeletedAsync();
        await _db.Database.MigrateAsync();
        if (!await _db.Users.AnyAsync())
        {
            _db.Users.Add(new User
            {
                NumeroInscricao = "0001",
                Nome = "Suporte 1",
                Email = "suporte@payhelp.local",
                SenhaHash = HashStub.Hash("123456"),
                Role = PayHelp.Domain.Enums.UserRole.Suporte
            });
            await _db.SaveChangesAsync();
        }
        return Ok(new { status = "ok" });
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        if (!_env.IsDevelopment()) return DevOnly();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == req.Email.ToLower());
        if (user == null) return NotFound(new { message = "Usuário não encontrado." });
        user.SenhaHash = HashStub.Hash(req.NovaSenha);
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
        return Ok(new { status = "ok" });
    }



    /// - All = true: remove todos os chamados.





    [HttpPost("purge-tickets")]
    public async Task<ActionResult> PurgeTickets([FromBody] PurgeTicketsRequest req)
    {
        if (!_env.IsDevelopment()) return DevOnly();

        var q = _db.Tickets.AsQueryable();


        if (!string.IsNullOrWhiteSpace(req.UserEmail))
        {
            var email = req.UserEmail.Trim().ToLower();
            var userIds = await _db.Users
                .Where(u => u.Email.ToLower() == email)
                .Select(u => u.Id)
                .ToListAsync();
            if (userIds.Count == 0) return Ok(new { removed = 0, note = "Nenhum usuário encontrado para o email informado." });
            q = q.Where(t => userIds.Contains(t.UserId));
        }


        if (!string.IsNullOrWhiteSpace(req.Contains))
        {
            var term = req.Contains.Trim().ToLower();
            q = q.Where(t => t.Titulo.ToLower().Contains(term) || t.Descricao.ToLower().Contains(term));
        }


        if (req.BeforeUtc.HasValue)
        {
            var dt = DateTime.SpecifyKind(req.BeforeUtc.Value, DateTimeKind.Utc);
            q = q.Where(t => t.CriadoEmUtc <= dt);
        }


        var anyFilter = req.All || !string.IsNullOrWhiteSpace(req.Contains) || !string.IsNullOrWhiteSpace(req.UserEmail) || req.BeforeUtc.HasValue;
        if (!anyFilter)
        {
            return BadRequest(new { message = "Defina All=true ou informe ao menos um filtro (Contains, UserEmail, BeforeUtc)." });
        }

        if (req.All)
        {
            // Apaga todos os tickets. FKs estão configuradas para cascade (mensagens, relatórios) e set null (FAQ).
            var removedAll = await _db.Tickets.ExecuteDeleteAsync();
            return Ok(new { removed = removedAll, scope = "all" });
        }
        else
        {
            var removed = await q.ExecuteDeleteAsync();
            return Ok(new { removed, scope = "filtered" });
        }
    }
}
