using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Eliza.Core
{
    public class LlmClient
    {
        private readonly HttpClient _client;
        private readonly string _apiUrl = "http://localhost:1234/v1/chat/completions";
        private readonly string _modelName = "gemma4-12b-qat"; 

        public LlmClient()
        {
            _client = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
        }

        public async IAsyncEnumerable<string> SendMessageStreamAsync(List<object> messages)
        {
            var payload = new
            {
                model = _modelName,
                messages = messages,
                temperature = 0.6, // Чуть снизили для стабильности протокола
                max_tokens = 150,
                stream = true
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl) { Content = content };

            using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line.StartsWith("data: "))
                {
                    var data = line.Substring(6).Trim();
                    if (data == "[DONE]") break;

                    string chunk = ExtractContentFromJson(data);
                    if (!string.IsNullOrEmpty(chunk))
                    {
                        yield return chunk;
                    }
                }
            }
        }

        private string ExtractContentFromJson(string jsonChunk)
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonChunk);
                var choices = doc.RootElement.GetProperty("choices");
                if (choices.GetArrayLength() > 0)
                {
                    var delta = choices[0].GetProperty("delta");
                    if (delta.TryGetProperty("content", out var contentProp))
                    {
                        return contentProp.GetString() ?? "";
                    }
                }
            }
            catch
            {
                // Глотаем ошибки парсинга битых JSON-чанков
            }
            return "";
        }
    }
}