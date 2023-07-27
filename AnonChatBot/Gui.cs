using System;
using System.Drawing;
using Console = Colorful.Console;

namespace AnonChatBot
{
    // I don't know when it will be ready
    // it's just unfinished code
    internal class Gui
    {
        public static string selectedLang = null;
        public static void MainGui()
        {
            Console.Clear();
            Console.WriteLine("Select the interface language (EN if you work on a dedicate server that does not support RU)");
            Console.WriteLine("[1] RU");
            Console.WriteLine("[2] EN");
            var langKey = Console.ReadKey();
            Console.Clear();
            if (langKey.Key == ConsoleKey.D1)
            {
                selectedLang = "RU";
                Console.WriteAscii("AnonChatBot", Color.FromArgb(244, 212, 255));
                Console.WriteLine("[1] Запустить спамер", Color.IndianRed);
                Console.WriteLine("[2] Настройка конфига", Color.IndianRed);
                Console.WriteLine("[3] Поддержать разработчика", Color.IndianRed);
                Console.WriteLine("Нажмите цифру на клавиатуре, чтобы выбрать пункт");
            }
            if (langKey.Key == ConsoleKey.D2)
            {
                selectedLang = "EN";
                Console.WriteAscii("AnonChatBot", Color.FromArgb(244, 212, 255));
                Console.WriteLine("[1] Launch a spammer", Color.IndianRed);
                Console.WriteLine("[2] Config setup", Color.IndianRed);
                Console.WriteLine("[3] Support the developer", Color.IndianRed);
                Console.WriteLine("Press a number on the keyboard to select an item");
            }
            var guiKey = Console.ReadKey();
            if (guiKey.Key == ConsoleKey.D1)
                // something
            if (guiKey.Key == ConsoleKey.D2)
                ConfigGui();
        }
        public static void ConfigGui()
        {
            Console.Clear();
        }
    }
}
