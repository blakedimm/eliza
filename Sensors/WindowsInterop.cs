using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Eliza.Sensors
{
    public static class WindowsInterop
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        // Возвращает название текущего активного окна
        public static string GetActiveWindowTitle()
        {
            IntPtr handle = GetForegroundWindow();
            if (handle == IntPtr.Zero) return "Рабочий стол";

            StringBuilder sb = new StringBuilder(256);
            if (GetWindowText(handle, sb, 256) > 0)
            {
                return sb.ToString().Trim();
            }

            return "Неизвестное окно";
        }

        // Возвращает время в миллисекундах с момента последнего движения мышью или клика
        public static uint GetIdleTimeMs()
        {
            var lastInput = new LASTINPUTINFO();
            lastInput.cbSize = (uint)Marshal.SizeOf(lastInput);
            if (GetLastInputInfo(ref lastInput))
            {
                return (uint)Environment.TickCount - lastInput.dwTime;
            }
            return 0;
        }
    }
}