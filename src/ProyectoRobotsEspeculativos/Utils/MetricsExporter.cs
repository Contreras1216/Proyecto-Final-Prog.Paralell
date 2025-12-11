using System;
using System.Diagnostics;
using System.IO;

namespace ProyectoRobotsEspeculativos.Utils
{
    public static class MetricsExporter
    {
        // Ruta absoluta hacia la carpeta "metrics" junto a la solución .sln
        private static readonly string metricsPath = Path.Combine(
            Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName,
            "metrics"
        );

        public static void Export(string fileName, string content)
        {
            try
            {
                if (!Directory.Exists(metricsPath))
                    Directory.CreateDirectory(metricsPath);

                string filePath = Path.Combine(metricsPath, fileName);

                File.WriteAllText(filePath, content);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[OK] Métrica guardada en: {filePath}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] No se pudo guardar la métrica: " + ex.Message);
                Console.ResetColor();
            }
        }

        public static void Benchmark(string title, Action action, string outputFile)
        {
            var sw = Stopwatch.StartNew();
            action();
            sw.Stop();

            string result =
                $"=== {title} ==={Environment.NewLine}" +
                $"Tiempo: {sw.ElapsedMilliseconds} ms{Environment.NewLine}" +
                $"Fecha: {DateTime.Now}{Environment.NewLine}";

            Export(outputFile, result);
        }
    }
}
