using System;

namespace ProyectoRobotsEspeculativos.Utils
{
    public static class Logger
    {
        private static readonly object _lock = new();

        public static void Log(string mensaje)
        {
            lock (_lock)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {mensaje}");
                Console.ResetColor();
            }
        }

        public static void Titulo(string texto)
        {
            lock (_lock)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n=== {texto.ToUpper()} ===\n");
                Console.ResetColor();
            }
        }

        public static void Subtitulo(string texto)
        {
            lock (_lock)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n--- {texto} ---");
                Console.ResetColor();
            }
        }

        public static void RobotHeader(string nombreRobot)
        {
            lock (_lock)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n########## {nombreRobot.ToUpper()} ##########\n");
                Console.ResetColor();
            }
        }

        public static void RobotStep(string nombreRobot, string mensaje)
        {
            lock (_lock)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"[{nombreRobot}] {mensaje}");
                Console.ResetColor();
            }
        }
    }
}
