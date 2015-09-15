using System;

namespace ArkUpdater
{
    class Log
    {
        public static void LogToConsole(string message)
        {
            Console.WriteLine("[{0:HH:mm:ss}] {1}", DateTime.Now, message);
        }

        public static void LogErrorToConsole(string message)
        {
            ConsoleColor color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("[{0:HH:mm:ss}] {1}", DateTime.Now, message);
            Console.ForegroundColor = color;
        }
    }
}
