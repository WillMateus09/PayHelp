using System;
using System.ComponentModel.DataAnnotations;

namespace PayHelp.WebApp.Mvc.Models
{
    /// <summary>
    /// ViewModel para marcação de chamado como resolvido pelo usuário
    /// </summary>
    public class MarcarResolvidoViewModel
    {
        public Guid TicketId { get; set; }

        public string? TituloTicket { get; set; }

        [Required(ErrorMessage = "O feedback é obrigatório")]
        [StringLength(2000, ErrorMessage = "O feedback deve ter no máximo 2000 caracteres")]
        [Display(Name = "Seu comentário")]
        public string FeedbackUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "Por favor, selecione uma nota de 1 a 5 estrelas")]
        [Range(1, 5, ErrorMessage = "A nota deve estar entre 1 e 5")]
        [Display(Name = "Avaliação")]
        public int NotaUsuario { get; set; }
    }
}
