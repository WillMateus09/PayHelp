using System.ComponentModel.DataAnnotations;

namespace PayHelp.WebApp.Mvc.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Selecione o tipo de usuário")]
    public string TipoUsuario { get; set; } = "Simples";

    [Required(ErrorMessage = "Número de inscrição é obrigatório")] 
    public string NumeroInscricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nome é obrigatório")] 
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail é obrigatório")] 
    [EmailAddress(ErrorMessage = "E-mail inválido")] 
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    [MinLength(6, ErrorMessage = "Senha deve ter no mínimo 6 caracteres")] 
    public string Senha { get; set; } = string.Empty;

    public string? Verificacao { get; set; }
}
