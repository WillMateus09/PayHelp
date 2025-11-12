using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PayHelp.WebApp.Mvc.ViewModels;
using PayHelp.WebApp.Mvc.Services;

namespace PayHelp.WebApp.Mvc.Controllers;

public class AccountController : Controller
{
    private readonly ApiService _api;

    public AccountController(ApiService api)
    {
        _api = api;
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        try
        {

            var isSup = string.Equals(vm.TipoUsuario, "Suporte", StringComparison.OrdinalIgnoreCase);
            if (isSup)
            {
                var resp = await _api.PostAsync<object, AuthResponse>("auth/register/suporte", new { vm.NumeroInscricao, vm.Nome, vm.Email, Senha = vm.Senha, PalavraVerificacao = vm.Verificacao ?? string.Empty });
                await StoreTokenAndSignIn(resp);
            }
            else
            {
                var resp = await _api.PostAsync<object, AuthResponse>("auth/register/simples", new { vm.NumeroInscricao, vm.Nome, vm.Email, Senha = vm.Senha });
                await StoreTokenAndSignIn(resp);
            }
            return RedirectToAction("Index", "Home");
        }
        catch (UnauthorizedAccessException)
        {
            ModelState.AddModelError(string.Empty, "Não autorizado. Verifique suas permissões ou tente novamente.");
            return View(vm);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "API indisponível. Tente novamente em instantes.");
            return View(vm);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
    }

    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        try
        {
            var email = (vm.Email ?? string.Empty).Trim().ToLowerInvariant();
            var resp = await _api.PostAsync<object, AuthResponse>("auth/login", new { Email = email, Senha = vm.Senha });
            if (resp is null)
            {
                ModelState.AddModelError(string.Empty, "Email ou senha inválidos");
                return View(vm);
            }
            await StoreTokenAndSignIn(resp);
            return RedirectToAction("Index", "Home");
        }
        catch (UnauthorizedAccessException)
        {
            ModelState.AddModelError(string.Empty, "Email ou senha inválidos.");
            return View(vm);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "API indisponível. Tente novamente em instantes.");
            return View(vm);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    private async Task SignInAsync(Guid userId, string nome, string email, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, nome),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }

    private async Task StoreTokenAndSignIn(AuthResponse resp)
    {
        if (resp == null) throw new InvalidOperationException("Resposta de autenticação inválida");
        HttpContext.Session.SetString("ApiJwt", resp.Token ?? string.Empty);
        await SignInAsync(resp.UserId, resp.Nome, resp.Email, resp.Role);
    }

    public record AuthResponse(Guid UserId, string Nome, string Email, string Role, string Token, DateTime ExpiresAtUtc);
}
