using System;

namespace Eliza.Core
{
    public class ParsedResponse
    {
        public string Emotion { get; set; } = "E0";
        public string ActionCode { get; set; } = "000";
        public string ActionArg { get; set; } = "none";
        public string SpeechText { get; set; } = "";
    }

    public static class ProtocolParser
    {
        public static ParsedResponse Parse(string rawText)
        {
            var result = new ParsedResponse();
            if (string.IsNullOrWhiteSpace(rawText)) return result;

            // Ожидаемый формат: E1-102:spotify | Включаю!
            var parts = rawText.Split(new[] { "|" }, 2, StringSplitOptions.None);
            
            if (parts.Length > 1)
            {
                result.SpeechText = parts[1].Trim();
            }
            else
            {
                // Fallback: если модель не выдала разделитель, считаем всё текстом
                result.SpeechText = rawText.Trim();
                return result; 
            }

            var header = parts[0].Trim(); // Пример: "E1-102:spotify"
            
            try
            {
                if (header.Length >= 2) 
                    result.Emotion = header.Substring(0, 2);
                
                var dashIndex = header.IndexOf('-');
                var colonIndex = header.IndexOf(':');

                if (dashIndex != -1 && colonIndex != -1 && colonIndex > dashIndex)
                {
                    result.ActionCode = header.Substring(dashIndex + 1, colonIndex - dashIndex - 1).Trim();
                    result.ActionArg = header.Substring(colonIndex + 1).Trim();
                }
            }
            catch
            {
                // Если заголовок сгенерирован криво, глотаем ошибку. Элиза просто проговорит текст с дефолтной эмоцией.
            }

            return result;
        }
    }
}