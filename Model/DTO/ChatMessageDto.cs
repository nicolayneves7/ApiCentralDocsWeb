using System.Text.Json.Serialization;

namespace ApiCentralDocsWeb.Model.DTO // <--- ATENÇÃO: Tem que ser igual ao "using" do seu Controller
{
    public class ChatMessageDto
    {
        // O nome em C# é "Role" (com R maiúsculo)
        // Mas o [JsonPropertyName] garante que será enviado como "role" (minúsculo) para a OpenRouter
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        // O nome em C# é "Conteudo" (com C maiúsculo)
        // Mas o [JsonPropertyName] garante que será enviado como "content" (minúsculo) para a OpenRouter
        [JsonPropertyName("content")]
        public string Conteudo { get; set; } = string.Empty;
    }
}