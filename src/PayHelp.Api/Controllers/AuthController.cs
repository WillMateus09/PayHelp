using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayHelp.Application.Services;
using PayHelp.Api.Services;

namespace PayHelp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly IJwtTokenService _jwt;
    private readonly Application.Abstractions.ISystemSettingsRepository _settings;
    public AuthController(IAuthService auth, IJwtTokenService jwt, Application.Abstractions.ISystemSettingsRepository settings){ _auth = auth; _jwt = jwt; _settings = settings; }

    public record RegisterSimplesRequest(string NumeroInscricao, string Nome, string Email, string Senha);
    public record RegisterSuporteRequest(string NumeroInscricao, string Nome, string Email, string Senha, string PalavraVerificacao);
    public record LoginRequest(string Email, string Senha);
    public record AuthResponse(Guid UserId, string Nome, string Email, string Role, string Token, DateTime ExpiresAtUtc);

    [HttpPost("register/simples")]
    public async Task<ActionResult<AuthResponse>> RegisterSimples([FromBody] RegisterSimplesRequest req)
    {
        try
        {
            var u = await _auth.RegistrarUsuarioSimplesAsync(req.NumeroInscricao, req.Nome, req.Email, req.Senha);
            var (token, exp) = _jwt.CreateToken(u);
            return Ok(new AuthResponse(u.Id, u.Nome, u.Email, u.Role.ToString(), token, exp));
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
            var expected = (await _settings.GetAsync("SupportVerificationWord"))?.Value ?? "SUP";
            if (!string.Equals(req.PalavraVerificacao, expected, StringComparison.OrdinalIgnoreCase))
                return Problem("Palavra de verificação inválida.", statusCode: 400);

            var u = await _auth.RegistrarUsuarioSuporteAsync(req.NumeroInscricao, req.Nome, req.Email, req.Senha, req.PalavraVerificacao);
            var (token, exp) = _jwt.CreateToken(u);
            return Ok(new AuthResponse(u.Id, u.Nome, u.Email, u.Role.ToString(), token, exp));
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
        var (token, exp) = _jwt.CreateToken(u);
        return Ok(new AuthResponse(u.Id, u.Nome, u.Email, u.Role.ToString(), token, exp));
    }
}
