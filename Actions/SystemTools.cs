using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Eliza.Actions
{
    public static class SystemTools
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDCHANGE = 0x02;

        public static void RunApplication(string appNameOrUrl)
        {
            try
            {
                if (appNameOrUrl.Equals("spotify", StringComparison.OrdinalIgnoreCase))
                {
                    try { Process.Start(new ProcessStartInfo { FileName = "spotify:", UseShellExecute = true }); }
                    catch { Process.Start(new ProcessStartInfo { FileName = "spotify", UseShellExecute = true }); }
                    return;
                }

                Process.Start(new ProcessStartInfo { FileName = appNameOrUrl, UseShellExecute = true });
            }
            catch { }
        }

        public static void OpenFolder(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = path, UseShellExecute = true });
                }
            }
            catch { }
        }

        public static void CreateDesktopNote(string text)
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string fileName = $"Элиза_Заметка_{new Random().Next(100, 999)}.txt";
                string fullPath = Path.Combine(desktopPath, fileName);
                
                File.WriteAllText(fullPath, text, Encoding.UTF8);
                Process.Start(new ProcessStartInfo { FileName = "notepad.exe", Arguments = fullPath, UseShellExecute = true });
            }
            catch { }
        }

        public static void SetWallpaper(string imagePath)
        {
            try
            {
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imagePath);
                if (File.Exists(fullPath))
                {
                    SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, fullPath, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
                }
            }
            catch { }
        }
    }
}