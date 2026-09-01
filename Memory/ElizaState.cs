using System;

namespace Eliza.Memory
{
    public class ElizaState
    {
        // Эмоциональный стейт
        public string CurrentEmotion { get; set; } = "E0";
        public bool IsOffended { get; set; } = false;
        public int OffendingTurns { get; set; } = 0; // Сколько раз юзер проигнорировал обиду
        
        // Режимы
        public bool IsMovieMode { get; set; } = false;
        public bool IsThinking { get; set; } = false;
        
        // Трекинг фокуса и ревности
        public string LastTrackedAppTitle { get; set; } = "";
        public bool IsGeminiEasterEggTriggered { get; set; } = false;

        // Таймеры скуки и простоя
        public int CurrentIdleTimeoutMs { get; set; } = 30000;
        public int TicksSinceLastActivity { get; set; } = 0;
        public int TotalIgnoreTicks { get; set; } = 0;

        public bool IsChibi { get; set; } = false;

        private Random _random = new Random();

        public ElizaState()
        {
            ResetIdleTimeout();
        }

        public void ResetIdleTimeout()
        {
            // Рандомизируем время, через которое Элиза заскучает (от 20 сек до 3 минут)
            CurrentIdleTimeoutMs = _random.Next(20000, 180000);
        }

        public void RegisterUserAction()
        {
            TicksSinceLastActivity = 0;
            TotalIgnoreTicks = 0;
        }
    }
}