# 🎙 Eliza — Autonomous Desktop AI Copilot & Voice Assistant

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-13.0-239120?logo=c-sharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-0078D6?logo=windows&logoColor=white)]()
[![LLM Protocol](https://img.shields.io/badge/Protocol-Structured%20Tool%20Calling-orange)]()

> **Eliza** is a native Windows desktop autonomous AI assistant and system copilot built on **.NET 10**. It integrates local/remote LLM reasoning with a custom command protocol parser, speech synthesis, and low-level OS automation tools.

---

## 🏛 Architecture & Execution Flow

```
┌────────────────────────────────────────────────────────┐
│                      User Input                        │
│               (Voice / Text / Hotkeys)                 │
└───────────────────────────┬────────────────────────────┘
                            │
                            ▼
┌────────────────────────────────────────────────────────┐
│                      LlmClient                         │
│   • Context Orchestration    • SystemPrompt Grounding  │
│   • Streaming Response       • Tool-Use Formatting     │
└───────────────────────────┬────────────────────────────┘
                            │
                            ▼
┌────────────────────────────────────────────────────────┐
│                   ProtocolParser                       │
│    (Extracts structured action tags & payloads)        │
└─────────────┬────────────────────────────┬─────────────┘
              │                            │
              ▼                            ▼
┌───────────────────────────┐┌───────────────────────────┐
│      ActionExecutor       ││      VoiceSynthesizer     │
│  • Shell & Process Exec   ││  • Native TTS Engine      │
│  • UI & Window Automation ││  • Audio Feedback Queue   │
│  • System State Query     │└───────────────────────────┘
└─────────────┬─────────────┘
              │
              ▼
┌───────────────────────────┐
│        SystemTools        │
│   (Low-Level OS Interop)  │
└───────────────────────────┘
```

---

## 🚀 Key Features

* **Strict Action Protocol:** Custom DSL parser transforming natural language LLM outputs into verifiable, sandboxed OS commands.
* **Low-Level System Control (`SystemTools` & `ActionExecutor`):** Process orchestration, active window manipulation, volume management, and shell execution.
* **Voice Feedback Pipeline (`VoiceSynthesizer`):** Native asynchronous text-to-speech feedback pipeline.
* **Zero-Lag Async Client:** High-throughput streaming HTTP client for local engines (LM Studio, Ollama) and cloud LLM endpoints.

---

## 🛠 Tech Stack

* **Platform:** C# 13 / .NET 10 (Windows Desktop SDK)
* **APIs & Interop:** Win32 APIs, System.Speech / Media APIs
* **Networking:** System.Net.Http, System.Text.Json

---

## 🚀 Getting Started

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/)
* Windows 10 (Build 19041+) or Windows 11

```bash
# Clone repository
git clone [https://github.com/blakedimm/eliza.git](https://github.com/blakedimm/eliza.git)
cd eliza

# Build and run
dotnet restore
dotnet run -c Release
```

---

## 👨‍💻 Author
* **Developer:** [Blake](https://github.com/blakedimm)
* **Focus:** AI Agents, OS Automation & Cognitive Interfaces