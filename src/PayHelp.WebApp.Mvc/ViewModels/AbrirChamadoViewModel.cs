using System.ComponentModel.DataAnnotations;

namespace PayHelp.WebApp.Mvc.ViewModels;

public class AbrirChamadoViewModel
{
    [Required(ErrorMessage = "Título é obrigatório")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descrição é obrigatória")]
    public string Descricao { get; set; } = string.Empty;
}
