using Microsoft.AspNetCore.Mvc;
using PayHelp.Application.Services;

namespace PayHelp.WebApp.Mvc.Areas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth){ _auth = auth; }

    public record RegisterSimplesRequest(string NumeroInscricao, string Nome, string Email, string Senha);
    public record RegisterSuporteRequest(string NumeroInscricao, string Nome, string Email, string Senha, string PalavraVerificacao);
    public record LoginRequest(string Email, string Senha);
    public record AuthResponse(Guid UserId, string Nome, string Email, string Role);

    [HttpPost("register/simples")]
    public async Task<ActionResult<AuthResponse>> RegisterSimples([FromBody] RegisterSimplesRequest req)
    {
        try
        {
            var u = await _auth.RegistrarUsuarioSimplesAsync(req.NumeroInscricao, req.Nome, req.Email, req.Senha);
            return Ok(new AuthResponse(u.Id, u.Nome, u.Email, u.Role.ToString()));
        }
        catch (Exception ex)
        {
            return Problem(ex.Message, statusCode: 400);
        }
    }

    [HttpPost("register/suporte")]
    public async Task<ActionResult<AuthResponse>> RegisterSuporte([FromBody] RegisterSuporteRequest req)
    {
        try
        {
            var u = await _auth.RegistrarUsuarioSuporteAsync(req.NumeroInscricao, req.Nome, req.Email, req.Senha, req.PalavraVerificacao);
            return Ok(new AuthResponse(u.Id, u.Nome, u.Email, u.Role.ToString()));
        }
        catch (Exception ex)
        {
            return Problem(ex.Message, statusCode: 400);
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
    {
        var u = await _auth.LoginAsync(req.Email, req.Senha);
        if (u == null) return Unauthorized();
        return Ok(new AuthResponse(u.Id, u.Nome, u.Email, u.Role.ToString()));
    }
}
