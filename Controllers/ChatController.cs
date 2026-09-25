using ApiCentralDocsWeb.Model.DTO;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Text.Json;

namespace ApiCentralDocsWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly HttpClient _httpClient;
        private readonly string _openRouterApiKey;

        public ChatController(
            IConfiguration config,
            HttpClient httpClient)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "A connection string 'DefaultConnection' não foi encontrada.");

            _httpClient = httpClient;

            _openRouterApiKey = config["OpenRouterApiKey"]
                ?? throw new InvalidOperationException(
                    "A chave 'OpenRouterApiKey' não foi encontrada.");
        }

        [HttpPost("{sessaoId}")]
        public async Task<IActionResult> SendMessage(Guid sessaoId, [FromBody] SendMessageRequest request)
        {
            using var connection = new SqlConnection(_connectionString);

            // ✅ NOVO: Garantir que a sessão existe antes de inserir mensagens
            var checkSessionSql = @"
                IF NOT EXISTS (SELECT 1 FROM SessoesChat WHERE Id = @SessaoId)
                BEGIN
                    INSERT INTO SessoesChat (Id, UserId, Titulo)
                    VALUES (@SessaoId, 'usuario-anonimo', 'Nova Conversa');
                END";

            await connection.ExecuteAsync(checkSessionSql, new { SessaoId = sessaoId });

            // 1. Salvar mensagem do usuário no banco
            var insertUserSql = @"
                INSERT INTO Mensagens (SessaoId, Role, Conteudo)
                VALUES (@SessaoId, 'user', @Conteudo);";

            await connection.ExecuteAsync(
                insertUserSql,
                new
                {
                    SessaoId = sessaoId,
                    Conteudo = request.UserMessage
                });

            // 2. Buscar histórico da sessão
            var historySql = @"
                SELECT TOP 20
                    Role,
                    Conteudo
                FROM Mensagens
                WHERE SessaoId = @SessaoId
                ORDER BY DataCriacao DESC;";

            var messagesFromDb = await connection.QueryAsync<ChatMessageDto>(
                historySql,
                new
                {
                    SessaoId = sessaoId
                });

            var messages = messagesFromDb
                .Reverse()
                .ToList();

            // Adicionar prompt do sistema
            messages.Insert(0, new ChatMessageDto
            {
                Role = "system",
                Conteudo = "Você é um assistente virtual útil e profissional da CentralDocs. Responda de forma clara, concisa e em português."
            });

            // 3. Montar o payload para a OpenRouter
            var openRouterPayload = new
            {
                model = "qwen/qwen-2.5-72b-instruct",
                messages = messages
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(openRouterPayload),
                Encoding.UTF8,
                "application/json");

            // 4. Configurar os cabeçalhos da OpenRouter
            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "https://openrouter.ai/api/v1/chat/completions");

            httpRequest.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer",
                    _openRouterApiKey);

            httpRequest.Headers.Add("HTTP-Referer", "https://seusite.com");
            httpRequest.Headers.Add("X-Title", "CentralDocs");
            httpRequest.Content = jsonContent;

            // 5. Enviar mensagem para a OpenRouter
            var response = await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode(
                    (int)response.StatusCode,
                    new
                    {
                        message = "Erro ao chamar a IA.",
                        details = error
                    });
            }

            // 6. Ler resposta da OpenRouter
            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            var aiResponseText = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(aiResponseText))
            {
                return StatusCode(500, new { message = "A IA retornou uma resposta vazia." });
            }

            // 7. Salvar resposta da IA no banco
            var insertAiSql = @"
                INSERT INTO Mensagens (SessaoId, Role, Conteudo)
                VALUES (@SessaoId, 'assistant', @Conteudo);";

            await connection.ExecuteAsync(
                insertAiSql,
                new
                {
                    SessaoId = sessaoId,
                    Conteudo = aiResponseText
                });

            // 8. Retornar resposta para o React Native
            return Ok(new
            {
                message = aiResponseText
            });
        }
    }
}