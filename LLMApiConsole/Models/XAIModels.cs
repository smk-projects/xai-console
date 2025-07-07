using Newtonsoft.Json;

namespace LLMApiConsole.Models
{
    /// <summary>
    /// xAI API 回應的資料結構
    /// </summary>
    public class XAIResponse
    {
        [JsonProperty("choices")]
        public Choice[]? Choices { get; set; }
    }

    /// <summary>
    /// xAI API 選擇項目
    /// </summary>
    public class Choice
    {
        [JsonProperty("message")]
        public Message? Message { get; set; }
    }

    /// <summary>
    /// xAI API 訊息內容
    /// </summary>
    public class Message
    {
        [JsonProperty("content")]
        public string? Content { get; set; }
    }
}
