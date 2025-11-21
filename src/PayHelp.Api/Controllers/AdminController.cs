using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayHelp.Application.Abstractions;
using PayHelp.Application.Services;

namespace PayHelp.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Master")]
public class AdminController : ControllerBase
{
    private readonly IUserRepository _users;
    private readonly ISystemSettingsRepository _settings;
    private readonly IAuthService _auth;

    public AdminController(IUserRepository users, ISystemSettingsRepository settings, IAuthService auth)
    { _users = users; _settings = settings; _auth = auth; }

    public record SettingsDto(string? SupportVerificationWord, string? PublicBaseUrl);

    [HttpGet("settings")]
    public async Task<ActionResult<SettingsDto>> GetSettings()
    {
        var support = (await _settings.GetAsync("SupportVerificationWord"))?.Value;
        var baseUrl = (await _settings.GetAsync("PublicBaseUrl"))?.Value;
        return Ok(new SettingsDto(support, baseUrl));
    }

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] SettingsDto dto)
    {
        if (dto.SupportVerificationWord != null)
            await _settings.SetAsync("SupportVerificationWord", dto.SupportVerificationWord);
        if (dto.PublicBaseUrl != null)
            await _settings.SetAsync("PublicBaseUrl", dto.PublicBaseUrl);
        return NoContent();
    }

    public record BlockDto(bool Blocked);
    [HttpPost("users/{id:guid}/block")]
    public async Task<IActionResult> BlockUser(Guid id, [FromBody] BlockDto dto)
    {
        var u = await _users.GetByIdAsync(id);
        if (u == null) return NotFound();
        u.IsBlocked = dto.Blocked;
        await _users.UpdateAsync(u);
        return NoContent();
    }

    public record PasswordDto(string NewPassword);
    [HttpPost("users/{id:guid}/password")]
    public async Task<IActionResult> SetPassword(Guid id, [FromBody] PasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NewPassword)) return BadRequest("Senha obrigatória");
        var ok = await _auth.SetPasswordAsync(id, dto.NewPassword);
        return ok ? NoContent() : NotFound();
    }

    public record UserListItem(Guid Id, string Nome, string Email, string Role, bool IsBlocked);
    [HttpGet("users")]
    public async Task<IEnumerable<UserListItem>> ListUsers()
    {
        var list = await _users.GetAllAsync();
        return list.Select(u => new UserListItem(u.Id, u.Nome, u.Email, u.Role.ToString(), u.IsBlocked));
    }
}
