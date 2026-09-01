using Eliza.Core;
using System.Threading.Tasks;
using System.Windows;

namespace Eliza.Actions
{
    public class ActionExecutor
    {
        // Возвращает true, если получена команда на закрытие (999)
        public bool Execute(ParsedResponse response)
        {
            if (string.IsNullOrEmpty(response.ActionCode) || response.ActionCode == "000")
            {
                return false;
            }

            switch (response.ActionCode)
            {
                case "101":
                    // Запрос скриншота - логика обрабатывается выше в UI, здесь просто пропускаем
                    break;
                case "102":
                    SystemTools.RunApplication(response.ActionArg);
                    break;
                case "103":
                    SystemTools.OpenFolder(response.ActionArg);
                    break;
                case "104":
                    SystemTools.CreateDesktopNote(response.ActionArg);
                    break;
                case "105":
                    SystemTools.SetWallpaper(response.ActionArg);
                    break;
                case "999":
                    // Элиза обиделась и уходит
                    return true;
            }

            return false;
        }
    }
}