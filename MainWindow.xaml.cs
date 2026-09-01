using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using Eliza.Core;
using Eliza.Sensors;
using Eliza.Actions;
using Eliza.Memory;

namespace Eliza
{
    public partial class MainWindow : Window
    {
        // Модули архитектуры
        private readonly ElizaState _state;
        private readonly LlmClient _llm;
        private readonly ActionExecutor _actions;
        private readonly VoiceSynthesizer _voice;
        private readonly ScreenReader _screen;
        private readonly Telemetry _telemetry;

        // UI-переменные
        private DispatcherTimer _mainTimer;
        private DispatcherTimer _typewriterTimer;
        private Queue<char> _typewriterQueue = new Queue<char>();
        private ParsedResponse _pendingAction = null;

        public MainWindow()
        {
            InitializeComponent();

            // Инициализация модулей
            _state = new ElizaState();
            _llm = new LlmClient();
            _actions = new ActionExecutor();
            _voice = new VoiceSynthesizer();
            _screen = new ScreenReader();
            _telemetry = new Telemetry();

            // Настройка таймеров
            _mainTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _mainTimer.Tick += MainTimer_Tick;
            _mainTimer.Start();

            _typewriterTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
            _typewriterTimer.Tick += TypewriterTimer_Tick;
            _typewriterTimer.Start();

            this.Loaded += (s, e) => { RepositionToCorner(); UpdateAppearance(false); };
            this.SizeChanged += (s, e) => RepositionToCorner();
        }

        // ==========================================
        // ЛОГИКА ОБЩЕНИЯ С НЕЙРОСЕТЬЮ
        // ==========================================
        private async void InputField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string userText = InputField.Text.Trim();
                if (string.IsNullOrEmpty(userText)) return;

                InputField.Clear();
                _state.RegisterUserAction();
                Database.AddLog($"Артём: {userText}");

                await SendMessageAsync(userText);
            }
        }

        private async Task SendMessageAsync(string userText, bool forceScreenshot = false)
        {
            _state.IsThinking = true;
            ChatHistory.Text = "● ● ●";
            DialogBubble.Visibility = Visibility.Visible;
            _typewriterQueue.Clear();
            UpdateAppearance();

            // Сбор телеметрии
            var (cpu, gpu) = await _telemetry.GetSystemLoadAsync();
            string activeApp = WindowsInterop.GetActiveWindowTitle();
            string ocrText = "";

            if (forceScreenshot || userText.ToLower().Contains("посмотри") || userText.ToLower().Contains("экран"))
            {
                using var bmp = _screen.CaptureScreen();
                ocrText = await _screen.ExtractTextAsync(bmp);
            }

            string context = $"[СИСТЕМНЫЙ КОНТЕКСТ]\nАктивное окно: {activeApp}\nНагрузка CPU: {cpu}%, GPU: {gpu}%\nТекст на экране: {ocrText}";
            
            var messages = new List<object>
            {
                new { role = "system", content = SystemPrompt.BasePrompt + "\n\n" + context },
                new { role = "user", content = userText }
            };

            string accumulatedText = "";
            bool headerParsed = false;
            ParsedResponse currentResponse = new ParsedResponse();

            try
            {
                // Стриминг и парсинг "на лету"
                await foreach (var chunk in _llm.SendMessageStreamAsync(messages))
                {
                    accumulatedText += chunk;

                    // Как только модель выдала разделитель "|", мы парсим эмоцию и действие
                    if (!headerParsed && accumulatedText.Contains("|"))
                    {
                        headerParsed = true;
                        currentResponse = ProtocolParser.Parse(accumulatedText);
                        
                        _state.CurrentEmotion = currentResponse.Emotion;
                        UpdateAppearance(); // Лицо меняется до того, как текст допечатался!

                        // Отправляем в печатную машинку всё, что после "|"
                        string textPart = accumulatedText.Substring(accumulatedText.IndexOf('|') + 1).TrimStart();
                        foreach(var c in textPart) _typewriterQueue.Enqueue(c);
                        ChatHistory.Text = ""; // Убираем точки
                    }
                    else if (headerParsed)
                    {
                        foreach(var c in chunk) _typewriterQueue.Enqueue(c);
                    }
                }

                // Завершение генерации
                _state.IsThinking = false;
                UpdateAppearance();
                
                _voice.Speak(currentResponse.SpeechText);
                Database.AddLog($"Элиза: {currentResponse.SpeechText}");

                // Обработка действий (тулов)
                if (currentResponse.ActionCode != "000")
                {
                    HandleParsedAction(currentResponse);
                }
            }
            catch (Exception ex)
            {
                ChatHistory.Text = $"[Ошибка связи]: {ex.Message}";
                _state.IsThinking = false;
                UpdateAppearance();
            }
        }

        private void HandleParsedAction(ParsedResponse response)
        {
            if (response.ActionCode == "999")
            {
                Application.Current.Shutdown();
                return;
            }

            // Если действие требует подтверждения
            _pendingAction = response;
            ConfirmText.Text = $"Разрешить действие: {response.ActionCode} ({response.ActionArg})?";
            InputBorder.Visibility = Visibility.Collapsed;
            ConfirmationPanel.Visibility = Visibility.Visible;
        }

        // ==========================================
        // UI И СОБЫТИЯ
        // ==========================================
        private void AllowButton_Click(object sender, RoutedEventArgs e)
        {
            HideConfirmationPrompt();
            if (_pendingAction != null) _actions.Execute(_pendingAction);
            _pendingAction = null;
        }

        private void DenyButton_Click(object sender, RoutedEventArgs e)
        {
            HideConfirmationPrompt();
            _pendingAction = null;
        }

        private void HideConfirmationPrompt()
        {
            ConfirmationPanel.Visibility = Visibility.Collapsed;
            InputBorder.Visibility = Visibility.Visible;
            InputField.Focus();
        }

        private void TypewriterTimer_Tick(object sender, EventArgs e)
        {
            if (_typewriterQueue.Count > 0)
            {
                if (ChatHistory.Text == "● ● ●") ChatHistory.Text = "";
                int batchSize = _typewriterQueue.Count > 15 ? 2 : 1;
                for (int i = 0; i < batchSize && _typewriterQueue.Count > 0; i++) 
                    ChatHistory.Text += _typewriterQueue.Dequeue();
            }
        }

        private void MainTimer_Tick(object sender, EventArgs e)
        {
            if (_state.IsThinking) return;

            string activeApp = WindowsInterop.GetActiveWindowTitle();
            
            // Если активно не десктопное окно — прячемся в Чиби режим
            bool shouldBeChibi = !(activeApp.Contains("Progman") || activeApp.Contains("WorkerW"));
            if (_state.IsChibi != shouldBeChibi)
            {
                _state.IsChibi = shouldBeChibi;
                UpdateAppearance();
            }

            // Механика скуки
            uint idleTime = WindowsInterop.GetIdleTimeMs();
            if (idleTime > _state.CurrentIdleTimeoutMs)
            {
                _state.ResetIdleTimeout();
                _ = SendMessageAsync("[Система: Пользователь долго бездействует. Обрати на себя внимание!]");
            }
        }

        private void UpdateAppearance(bool animate = true)
        {
            string emotion = _state.CurrentEmotion.ToLower().Replace("e", ""); // E1 -> 1
            
            // Маппинг кодов эмоций на названия файлов
            string prefix = emotion switch
            {
                "1" => "happy",
                "2" => "wink",
                "3" => "sad",
                "4" => "shocked",
                "5" => "angry",
                "6" => "shy",
                "7" => "thinking",
                _ => "idle"
            };

            if (_state.IsThinking) prefix = "thinking";
            if (_state.IsChibi && !_state.IsThinking) prefix = "chibi"; // Если хочешь, чтобы в мини-режиме всегда была чиби

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sprites", $"madam_{prefix}.png");
            if (!File.Exists(path)) path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sprites", "madam_idle.png");

            if (File.Exists(path))
            {
                MadamSprite.Source = new BitmapImage(new Uri(path));
            }

            double targetWidth = _state.IsChibi ? 210 : 280;
            double targetHeight = _state.IsChibi ? 280 : 370;

            if (animate)
            {
                var duration = new Duration(TimeSpan.FromMilliseconds(250));
                MadamSprite.BeginAnimation(Image.WidthProperty, new DoubleAnimation(targetWidth, duration) { EasingFunction = new QuadraticEase() });
                MadamSprite.BeginAnimation(Image.HeightProperty, new DoubleAnimation(targetHeight, duration) { EasingFunction = new QuadraticEase() });
            }
            else
            {
                MadamSprite.Width = targetWidth;
                MadamSprite.Height = targetHeight;
            }
        }

        // ==========================================
        // БАЗОВЫЕ ФУНКЦИИ ОКНА
        // ==========================================
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void MadamSprite_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                DialogBubble.Visibility = DialogBubble.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void RepositionToCorner()
        {
            this.BeginAnimation(Window.LeftProperty, null);
            this.BeginAnimation(Window.TopProperty, null);
            this.UpdateLayout();

            var area = SystemParameters.WorkArea;
            this.Left = area.Right - this.ActualWidth - 20;
            this.Top = area.Bottom - this.ActualHeight - 10;
        }
    }
}