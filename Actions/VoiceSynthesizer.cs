using System;

namespace Eliza.Actions
{
    public class VoiceSynthesizer
    {
        private dynamic _sapiVoice;

        public VoiceSynthesizer()
        {
            InitializeSapi();
        }

        private void InitializeSapi()
        {
            try
            {
                Type type = Type.GetTypeFromProgID("SAPI.SpVoice");
                if (type != null)
                {
                    _sapiVoice = Activator.CreateInstance(type);
                    var voices = _sapiVoice?.GetVoices();
                    dynamic targetVoice = null;

                    if (voices != null)
                    {
                        for (int i = 0; i < voices.Count; i++)
                        {
                            var voice = voices.Item(i);
                            string name = voice.GetDescription().ToLower();

                            if (name.Contains("aigul") || name.Contains("айгуль") || 
                                name.Contains("baya") || name.Contains("бая"))
                            {
                                targetVoice = voice;
                                break;
                            }
                            
                            // Фолбэк на любой русский голос, кроме Ксении
                            if ((name.Contains("russian") || name.Contains("ru-")) && !name.Contains("kseniya"))
                            {
                                if (targetVoice == null) targetVoice = voice;
                            }
                        }
                    }

                    if (targetVoice != null && _sapiVoice != null)
                    {
                        _sapiVoice.Voice = targetVoice;
                        _sapiVoice.Rate = 2; // Ускоренный темп речи
                    }
                }
            }
            catch { }
        }

        public void Speak(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || _sapiVoice == null) return;
            
            // Удаляем возможные остатки тегов, если модель все-таки сглючила
            string cleanText = text.Replace("❤️", "").Replace("<3", "").Trim();
            
            try 
            { 
                _sapiVoice.Speak(cleanText, 3); // 3 = асинхронное чтение + сброс предыдущей очереди
            } 
            catch { }
        }
    }
}