using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Eliza.Sensors
{
    public class Telemetry
    {
        private PerformanceCounter _cpuCounter;

        public Telemetry()
        {
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _cpuCounter.NextValue(); // Инициализация счетчика
            }
            catch { }
        }

        public async Task<(int CpuLoad, int GpuLoad)> GetSystemLoadAsync()
        {
            int cpu = 0;
            int gpu = 0;

            if (_cpuCounter != null)
            {
                try { cpu = (int)_cpuCounter.NextValue(); } catch { }
            }

            // Асинхронный вызов nvidia-smi для быстрой проверки загрузки видеокарты
            gpu = await Task.Run(() =>
            {
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "nvidia-smi",
                        Arguments = "--query-gpu=utilization.gpu --format=csv,noheader,nounits",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = Process.Start(startInfo);
                    if (process != null)
                    {
                        string output = process.StandardOutput.ReadToEnd().Trim();
                        if (int.TryParse(output, out int gpuVal)) return gpuVal;
                    }
                }
                catch { }
                
                return 0;
            });

            return (cpu, gpu);
        }
    }
}