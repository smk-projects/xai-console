using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using LLMApiConsole.Models;

class Program
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private static XAISettings? _xaiSettings;
    private static readonly List<object> _conversationHistory = new List<object>();

    static async Task Main(string[] args)
    {
        // 設定控制台編碼以支援中文
        try
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
        }
        catch (Exception)
        {
            // 在某些環境中可能無法設定編碼，忽略這個錯誤
        }
        
        // 載入設定檔
        LoadConfiguration();
        
        if (_xaiSettings == null || string.IsNullOrEmpty(_xaiSettings.ApiKey))
        {
            Console.WriteLine("錯誤：無法載入設定檔或 API 金鑰未設定");
            Console.WriteLine("請確認 appsettings.json 檔案存在且包含有效的 xAI API 金鑰");
            return;
        }

        // 設定 HTTP 用戶端標頭
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_xaiSettings.ApiKey}");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "LLMApiConsole/1.0");

        // 嘗試清除終端機畫面（在某些環境中可能會失敗）
        try
        {
            Console.Clear();
        }
        catch (IOException)
        {
            // 在某些環境中 Console.Clear() 可能不被支援，忽略這個錯誤
            Console.WriteLine("\n\n");
        }
        
        // 模型選擇
        string selectedModel = SelectModel();
        _xaiSettings.Model = selectedModel;
        
        Console.WriteLine($"=== xAI 對話助手 (使用模型: {selectedModel}) ===");
        Console.WriteLine("輸入 'exit' 或 'quit' 來結束程式");
        Console.WriteLine("輸入 'clear' 或 '重新開始' 來清除對話歷史\n");

        // 主要對話迴圈
        while (true)
        {
            // 1. 顯示輸入提示並等待使用者輸入
            Console.Write("請輸入訊息：\n");
            string? userInput = Console.ReadLine();

            if (string.IsNullOrEmpty(userInput))
                continue;

            if (userInput.ToLower() == "exit" || userInput.ToLower() == "quit")
            {
                Console.WriteLine("再見！");
                break;
            }

            if (userInput.ToLower() == "clear" || userInput.ToLower() == "重新開始")
            {
                _conversationHistory.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("對話歷史已清除，重新開始新的對話。");
                Console.ResetColor();
                Console.WriteLine();
                continue;
            }

            // 2. 清除 "請輸入訊息：" 並顯示使用者輸入（gray-600 顏色）
            try
            {
                // 回到行首並清除整行

                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write(new string(' ', Console.WindowWidth - 1));
                Console.SetCursorPosition(0, Console.CursorTop - 2);
                Console.Write(new string(' ', Console.WindowWidth - 2));                
                Console.SetCursorPosition(0, Console.CursorTop);
            }
            catch (IOException)
            {
                // 如果無法設定游標位置，忽略
            }

            Console.ForegroundColor = ConsoleColor.DarkGray; // gray-600 顏色
            Console.WriteLine($"\n{userInput}");
            Console.ResetColor();

            // 3. 顯示 "回應中..." 訊息（gray-300 非常淡的灰色）
            Console.ForegroundColor = ConsoleColor.Gray; // gray-300 淡灰色
            Console.Write("回應中...");
            Console.ResetColor();
            
            try
            {
                // 發送請求到 xAI API
                string response = await SendToXAI(userInput, selectedModel);
                
                // 4. 清除 "回應中..." 訊息
                try
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(new string(' ', "回應中...".Length));
                    Console.SetCursorPosition(0, Console.CursorTop);
                }
                catch (IOException)
                {
                    // 如果無法設定游標位置，直接換行
                    Console.WriteLine();
                }

                // 5. 顯示 AI 回應
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{response}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                // 清除 "回應中..." 訊息
                try
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(new string(' ', "回應中...".Length));
                    Console.SetCursorPosition(0, Console.CursorTop);
                }
                catch (IOException)
                {
                    // 如果無法設定游標位置，直接換行
                    Console.WriteLine();
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"錯誤：{ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine(); // 添加空行分隔
            // 6. 回到步驟 1，顯示新的 "請輸入訊息：" 提示
        }
    }

    /// <summary>
    /// 載入應用程式設定
    /// </summary>
    private static void LoadConfiguration()
    {
        try
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _xaiSettings = new XAISettings();
            configuration.GetSection("XAI").Bind(_xaiSettings);
            
            Console.WriteLine("設定檔載入成功");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"載入設定檔時發生錯誤：{ex.Message}");
        }
    }

    private static async Task<string> SendToXAI(string message, string? useModel = null)
    {
        if (_xaiSettings == null)
            throw new InvalidOperationException("xAI 設定未載入");

        if (string.IsNullOrEmpty(useModel))
            useModel = _xaiSettings.Model;

        // 如果這是第一次對話，加入系統提示
            if (_conversationHistory.Count == 0)
            {
                _conversationHistory.Add(new { role = "system", content = "請用繁體中文回應所有問題。使用台灣常用的詞彙和表達方式。" });
            }

        // 加入使用者訊息到對話歷史
        _conversationHistory.Add(new { role = "user", content = message });

        var requestBody = new
        {
            model = useModel,
            messages = _conversationHistory.ToArray(),
            max_tokens = _xaiSettings.MaxTokens,
            temperature = _xaiSettings.Temperature
        };

        string jsonContent = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync(_xaiSettings.ApiUrl, content);
        
        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"API 請求失敗：{response.StatusCode} - {errorContent}");
        }

        string responseContent = await response.Content.ReadAsStringAsync();
        
        var responseObj = JsonConvert.DeserializeObject<XAIResponse>(responseContent);
        
        if (responseObj?.Choices != null && responseObj.Choices.Length > 0)
        {
            string aiResponse = responseObj.Choices[0]?.Message?.Content ?? "AI 沒有回應內容";
            
            // 將 AI 回應加入對話歷史
            _conversationHistory.Add(new { role = "assistant", content = aiResponse });
            
            return aiResponse;
        }
        
        throw new Exception("無法解析 API 回應");
    }

    /// <summary>
    /// 讓使用者選擇要使用的模型
    /// </summary>
    /// <returns>選定的模型名稱</returns>
    private static string SelectModel()
    {
        while (true)
        {
            Console.WriteLine("\n請選擇使用的模型：");
            Console.WriteLine("(1) grok-3");
            Console.WriteLine("(2) grok-3-mini");
            Console.Write("請輸入選項 (1 或 2)：");
            
            string? choice = Console.ReadLine();
            
            switch (choice?.Trim())
            {
                case "1":
                    Console.WriteLine("已選擇：grok-3\n");
                    return "grok-3";
                case "2":
                    Console.WriteLine("已選擇：grok-3-mini\n");
                    return "grok-3-mini";
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("無效的選項，請輸入 1 或 2");
                    Console.ResetColor();
                    break;
            }
        }
    }
}
