# LLM API 終端機對話應用程式

這是一個使用 xAI API 進行自然語言對話的 C# 終端機應用程式。

## 專案結構

```
LLMApiConsole/
├── Models/
│   ├── XAIModels.cs        # xAI API 資料模型
│   └── XAISettings.cs      # 設定類別
├── Program.cs              # 主程式檔案
├── appsettings.json        # 應用程式設定檔
├── LLMApiConsole.csproj   # 專案設定檔
└── README.md              # 說明文件
```

## 使用方式

### 1. 設定 API 金鑰
編輯 `appsettings.json` 檔案，將您的 xAI API 金鑰填入：

```json
{
  "XAI": {
    "ApiKey": "your-xai-api-key-here",
    "ApiUrl": "https://api.x.ai/v1/chat/completions",
    "Model": "grok-beta",
    "MaxTokens": 1000,
    "Temperature": 0.7
  }
}
```

### 2. 建置專案
```bash
cd LLMApiConsole
dotnet restore
dotnet build
```

### 3. 執行應用程式
```bash
dotnet run
```

### 4. 開始對話
- 輸入您的訊息並按 Enter
- 等待 AI 回應
- 輸入 `exit` 或 `quit` 結束程式

## 系統需求

- .NET 9.0 或更高版本
- 有效的 xAI API 金鑰
- 網路連線

## 依賴套件

- Microsoft.Extensions.Configuration 8.0.0
- Microsoft.Extensions.Configuration.Json 8.0.0
- Microsoft.Extensions.Configuration.Binder 8.0.0
- Newtonsoft.Json 13.0.3

## API 整合說明

本應用程式使用 xAI 的 Chat Completions API：
- 端點：`https://api.x.ai/v1/chat/completions`
- 模型：`grok-beta`
- 最大令牌數：1000
- 溫度：0.7

## 錯誤處理

應用程式包含完整的錯誤處理機制：
- API 請求失敗處理
- 網路連線問題處理
- JSON 解析錯誤處理
- 無效 API 金鑰處理

## 安全性注意事項

- API 金鑰僅存儲在記憶體中，不會寫入檔案
- 建議使用環境變數或安全的金鑰管理系統來存儲 API 金鑰
