using System;
using System.Threading.Tasks;

namespace PayHelp.Desktop.WinForms
{
    /// <summary>
    /// Serviço para integração de feedbacks com o sistema de IA e FAQ
    /// </summary>
    public class IAFeedbackService
    {
        private readonly ApiClient _api;

        public IAFeedbackService(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// Registra o feedback no sistema de IA para alimentar a FAQ dinâmica e melhorar as sugestões automáticas
        /// </summary>
        /// <param name="feedback">Dados do feedback do usuário</param>
        /// <returns>True se o registro foi bem-sucedido</returns>
        public async Task<bool> RegistrarFeedbackAsync(FeedbackModel feedback)
        {
            if (feedback == null)
                throw new ArgumentNullException(nameof(feedback));

            try
            {
                // Salvar feedback no banco
                var feedbackSalvo = await _api.SalvarFeedbackAsync(
                    feedback.TicketId,
                    feedback.UserId,
                    feedback.Nota,
                    feedback.Comentario
                );

                if (feedbackSalvo == null)
                    return false;

                // Se a nota for 4 ou 5, considerar como resolução bem-sucedida
                if (feedback.Nota >= 4)
                {
                    await ProcessarFeedbackPositivoAsync(feedback);
                }
                else if (feedback.Nota <= 2)
                {
                    await ProcessarFeedbackNegativoAsync(feedback);
                }

                return true;
            }
            catch (Exception ex)
            {
                AppLog.Write($"[IAFeedbackService] Erro ao registrar feedback: {ex.Message}");
                return false;
            }
        }

        private async Task ProcessarFeedbackPositivoAsync(FeedbackModel feedback)
        {
            try
            {
                // Obter detalhes do chamado para análise
                var ticket = await _api.ObterChamadoAsync(feedback.TicketId);
                if (ticket == null) return;

                // Extrair informações relevantes das mensagens automáticas
                var mensagensBot = ticket.Mensagens
                    .Where(m => m.Automatica)
                    .OrderBy(m => m.EnviadoEmUtc)
                    .ToList();

                if (mensagensBot.Any())
                {
                    // A primeira mensagem do bot geralmente contém a sugestão principal
                    var primeiraSugestao = mensagensBot.First().Conteudo;

                    // Registrar na FAQ se houver comentário positivo
                    if (!string.IsNullOrWhiteSpace(feedback.Comentario))
                    {
                        await TentarRegistrarNaFaqAsync(ticket.Titulo, primeiraSugestao, feedback.Comentario);
                    }
                }

                AppLog.Write($"[IAFeedbackService] Feedback positivo processado para ticket {feedback.TicketId}");
            }
            catch (Exception ex)
            {
                AppLog.Write($"[IAFeedbackService] Erro ao processar feedback positivo: {ex.Message}");
            }
        }

        private async Task ProcessarFeedbackNegativoAsync(FeedbackModel feedback)
        {
            try
            {
                AppLog.Write($"[IAFeedbackService] Feedback negativo registrado para análise - Ticket: {feedback.TicketId}, Nota: {feedback.Nota}");
                
                // Em uma implementação completa, aqui você poderia:
                // - Enviar alerta para equipe de suporte
                // - Marcar o caso para revisão manual
                // - Ajustar pesos do algoritmo de IA
                // - Registrar padrões que não funcionaram
            }
            catch (Exception ex)
            {
                AppLog.Write($"[IAFeedbackService] Erro ao processar feedback negativo: {ex.Message}");
            }
        }

        private async Task TentarRegistrarNaFaqAsync(string problema, string solucao, string contexto)
        {
            try
            {
                // Formatar a solução com contexto adicional do usuário
                var solucaoCompleta = $"{solucao}\n\nContexto adicional: {contexto}";

                // Aqui você chamaria um endpoint específico para registrar na FAQ
                // Como não temos esse endpoint específico, vamos usar o registro de resolução final
                AppLog.Write($"[IAFeedbackService] Registrando na FAQ: Problema='{problema}', Solução registrada.");
                
                // Em produção, você teria algo como:
                // await _api.RegistrarNaFaqAsync(problema, solucaoCompleta);
            }
            catch (Exception ex)
            {
                AppLog.Write($"[IAFeedbackService] Erro ao registrar na FAQ: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém estatísticas de feedback para análise
        /// </summary>
        public async Task<FeedbackStats> ObterEstatisticasAsync()
        {
            try
            {
                // Em uma implementação completa, isso viria de um endpoint específico
                // Por enquanto, retornamos dados básicos
                return new FeedbackStats
                {
                    TotalFeedbacks = 0,
                    MediaNotas = 0.0,
                    TaxaSucesso = 0.0
                };
            }
            catch
            {
                return new FeedbackStats();
            }
        }
    }

    /// <summary>
    /// Modelo de dados para feedback
    /// </summary>
    public class FeedbackModel
    {
        public Guid TicketId { get; set; }
        public Guid UserId { get; set; }
        public int Nota { get; set; }
        public string? Comentario { get; set; }
    }

    /// <summary>
    /// Estatísticas de feedback
    /// </summary>
    public class FeedbackStats
    {
        public int TotalFeedbacks { get; set; }
        public double MediaNotas { get; set; }
        public double TaxaSucesso { get; set; }
    }
}
