namespace LLMApiConsole.Models
{
    /// <summary>
    /// xAI 設定選項
    /// </summary>
    public class XAISettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int MaxTokens { get; set; }
        public double Temperature { get; set; }
    }
}
