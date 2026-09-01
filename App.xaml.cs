using System.Windows;
using Eliza.Memory;

namespace Eliza
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Database.Load(); // Поднимаем память из файла
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Database.Save(); // Сохраняем перед выходом
            base.OnExit(e);
        }
    }
}