using ProyectoRobotsEspeculativos.EstadoCompartido;
using ProyectoRobotsEspeculativos.Robots;
using ProyectoRobotsEspeculativos.Paralelismo;
using ProyectoRobotsEspeculativos.Utils;   // NECESARIO PARA MetricsExporter
using ProyectoRobotsEspeculativos.Especulacion;

namespace ProyectoRobotsEspeculativos
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Logger.Log("Sistema de Robots con Descomposición Especulativa");

            // ==== DEMO de patrones con Task y CancellationToken ====
            await DemoPatronesTasksAsync();

            // ==== CancellationToken general del sistema ====
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            // ==== Crear estado global ====
            var estado = new EstadoGlobal();
            var motor = new MotorEspeculativo();

            // ==== Crear robots ====
            var r1 = new RobotCrearBase(1, motor);
            var r2 = new RobotRellenar(2, motor);
            var r3 = new RobotTapar(3, motor);
            var r4 = new RobotTransportar(4, motor);
            var r5 = new RobotVerificar(5, motor);
            var r6 = new RobotSellar(6, motor);

            var robotsPipeline = new IRobot[]
            {
                r1, r2, r3, r4, r5, r6
            };

            // ===============================================================
            // ==== PREGUNTAR AL USUARIO CUÁNTOS PRODUCTOS DESEA PREPARAR ====
            // ===============================================================

            Logger.Titulo("Configuración Inicial");

            int cantidadPiezas = 0;
            while (cantidadPiezas <= 0)
            {
                Console.Write("¿Cuántos productos deseas preparar? -> ");
                string? input = Console.ReadLine();

                if (int.TryParse(input, out cantidadPiezas) && cantidadPiezas > 0)
                {
                    Logger.Log($"Se crearán {cantidadPiezas} productos.");
                    break;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Número inválido. Debe ser un entero mayor que 0.\n");
                    Console.ResetColor();
                }
            }

            // ==== Crear piezas según la cantidad ingresada ====
            for (int i = 1; i <= cantidadPiezas; i++)
            {
                var pieza = new Pieza(i);
                estado.Piezas.TryAdd(pieza.Id, pieza);
            }

            Logger.Log($"Piezas generadas correctamente: {cantidadPiezas}");

            // ==== Ejecutar pipeline para cada pieza en paralelo (MIMD) ====
            var tareasPiezas = estado.Piezas.Values
                .Select(pieza => EjecutarPipelineParaPiezaAsync(pieza, robotsPipeline, estado, token))
                .ToArray();

            await Task.WhenAll(tareasPiezas);

            Logger.Log("Pipeline de todas las piezas finalizado.");

            // ==== DEMOS DE SIMD, MATRICES Y SPEEDUP ====
            SimdAndParallelDemos.DemoDotProduct();
            SimdAndParallelDemos.DemoSumaMatrices();
            SimdAndParallelDemos.DemoSpeedup();

            // ======================================================
            // ==== 🔥 GENERAR MÉTRICAS EN /metrics AUTOMÁTICAMENTE ====
            // ======================================================

            Logger.Titulo("Generando métricas en /metrics ...");

            MetricsExporter.Benchmark(
                "Producto Punto Secuencial (Benchmark)",
                () => SimdAndParallelDemos.DotProductSecuencial(new double[100000], new double[100000]),
                "dot_secuencial.txt"
            );

            MetricsExporter.Benchmark(
                "Producto Punto SIMD (Benchmark)",
                () => SimdAndParallelDemos.DotProductSimd(new double[100000], new double[100000]),
                "dot_simd.txt"
            );

            MetricsExporter.Benchmark(
                "Suma Matrices Paralela vs Secuencial (Benchmark)",
                () => SimdAndParallelDemos.DemoSumaMatrices(),
                "suma_matrices.txt"
            );

            MetricsExporter.Export("speedup.txt",
                "=== Speedup y Eficiencia ===\n" +
                $"Speedup teórico tomado del ejemplo: 4x\n" +
                $"Eficiencia estimada: 33%\n" +
                $"Fecha: {DateTime.Now}\n"
            );

            Logger.Log("Métricas generadas correctamente.");

            // ==== MOSTRAR LOG FINAL ====
            Logger.Log("Ejecución completa. Presiona una tecla para ver el log final...");
            Console.ReadKey();

            Console.WriteLine("\n=== LOG DE EVENTOS ===");
            foreach (var linea in estado.LogEventos)
            {
                Console.WriteLine(linea);
            }

            Console.WriteLine("\nFin. Presiona cualquier tecla para salir.");
            Console.ReadKey();
        }

        /// <summary>
        /// Ejecuta el pipeline completo (R1..R6) para una pieza.
        /// </summary>
        private static async Task EjecutarPipelineParaPiezaAsync(
            Pieza pieza,
            IRobot[] robots,
            EstadoGlobal estado,
            CancellationToken ct)
        {
            Logger.Titulo($"Procesando pieza {pieza.Id}");

            foreach (var robot in robots)
            {
                if (ct.IsCancellationRequested)
                {
                    Logger.Log($"[Pieza {pieza.Id}] Cancelado antes de robot {robot.Nombre}");
                    return;
                }

                await robot.ProcesarAsync(pieza, estado, ct);
            }
        }

        /// <summary>
        /// Demostración de todos los patrones de Task.
        /// </summary>
        private static async Task DemoPatronesTasksAsync()
        {
            Logger.Titulo("Demo de Patrones TASK / async / CancellationToken");

            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            // Task.Run
            var t1 = Task.Run(() =>
            {
                Logger.Log("[Demo] Task.Run ejecutándose...");
                Thread.Sleep(200);
            }, token);

            // new Task + Start
            var t2 = new Task(() =>
            {
                Logger.Log("[Demo] new Task() + Start ejecutándose...");
                Thread.Sleep(150);
            }, token);
            t2.Start();

            // Task.Factory.StartNew + AttachedToParent
            var parent = Task.Factory.StartNew(() =>
            {
                Logger.Log("[Demo] Parent Task iniciada...");

                var child = Task.Factory.StartNew(() =>
                {
                    Logger.Log("[Demo] Child Task con AttachedToParent ejecutándose...");
                    Thread.Sleep(100);
                }, token, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);

            }, token);

            // ContinueWith OnlyOnRanToCompletion
            var continuacion = parent.ContinueWith(t =>
            {
                Logger.Log("[Demo] ContinueWith OnlyOnRanToCompletion ejecutado.");
            }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);

            await Task.WhenAll(t1, t2, continuacion);

            Logger.Log("Fin de demo de patrones Task.");
        }
    }
}
