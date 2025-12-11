using System.Diagnostics;
using System.Numerics;

namespace ProyectoRobotsEspeculativos.Paralelismo
{
    public static class SimdAndParallelDemos
    {
        public static void DemoDotProduct()
        {
            int n = 100_000;
            var a = new double[n];
            var b = new double[n];

            var rnd = new Random(42);
            for (int i = 0; i < n; i++)
            {
                a[i] = rnd.NextDouble();
                b[i] = rnd.NextDouble();
            }

            var sw = Stopwatch.StartNew();
            var seq = DotProductSecuencial(a, b);
            sw.Stop();
            var tSeq = sw.ElapsedMilliseconds;

            sw.Restart();
            var simd = DotProductSimd(a, b);
            sw.Stop();
            var tSimd = sw.ElapsedMilliseconds;

            Console.WriteLine($"\n=== Producto Punto (SIMD vs Secuencial) ===");
            Console.WriteLine($"Secuencial: {seq:F4} | Tiempo: {tSeq} ms");
            Console.WriteLine($"SIMD:       {simd:F4} | Tiempo: {tSimd} ms");
        }

        public static double DotProductSecuencial(double[] a, double[] b)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
                sum += a[i] * b[i];
            return sum;
        }

        public static double DotProductSimd(double[] a, double[] b)
        {
            int length = a.Length;
            int simdSize = Vector<double>.Count;
            int i = 0;

            Vector<double> vsum = Vector<double>.Zero;

            for (; i <= length - simdSize; i += simdSize)
            {
                var va = new Vector<double>(a, i);
                var vb = new Vector<double>(b, i);
                vsum += va * vb;
            }

            double suma = 0;
            for (int j = 0; j < simdSize; j++)
                suma += vsum[j];

            for (; i < length; i++)
                suma += a[i] * b[i];

            return suma;
        }

        public static void DemoSumaMatrices()
        {
            int n = 300;
            var A = CrearMatriz(n, n, 1);
            var B = CrearMatriz(n, n, 2);

            var sw = Stopwatch.StartNew();
            var Cseq = SumarMatrizSecuencial(A, B);
            sw.Stop();
            var tSeq = sw.ElapsedMilliseconds;

            sw.Restart();
            var Cpar = SumarMatrizParalela(A, B);
            sw.Stop();
            var tPar = sw.ElapsedMilliseconds;

            Console.WriteLine($"\n=== Suma de Matrices en Paralelo ===");
            Console.WriteLine($"Tiempo Secuencial: {tSeq} ms");
            Console.WriteLine($"Tiempo Paralelo:   {tPar} ms");

            Console.WriteLine($"Ejemplo Cseq[0,0]={Cseq[0, 0]}, Cpar[0,0]={Cpar[0, 0]}");
        }

        private static double[,] CrearMatriz(int filas, int cols, double valorBase)
        {
            var m = new double[filas, cols];
            for (int i = 0; i < filas; i++)
                for (int j = 0; j < cols; j++)
                    m[i, j] = valorBase + i + j;
            return m;
        }

        public static double[,] SumarMatrizSecuencial(double[,] A, double[,] B)
        {
            int filas = A.GetLength(0);
            int cols = A.GetLength(1);
            var C = new double[filas, cols];

            for (int i = 0; i < filas; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    C[i, j] = A[i, j] + B[i, j];
                }
            }

            return C;
        }

        public static double[,] SumarMatrizParalela(double[,] A, double[,] B)
        {
            int filas = A.GetLength(0);
            int cols = A.GetLength(1);
            var C = new double[filas, cols];

            Parallel.For(0, filas, i =>
            {
                for (int j = 0; j < cols; j++)
                {
                    // NO hay dependencia entre elementos C[i,j], se puede paralelizar
                    C[i, j] = A[i, j] + B[i, j];
                }
            });

            return C;
        }

        public static void DemoSpeedup()
        {
            // Ejemplo simple de cálculo de speedup
            double tSecuencial = 1000; // ms (supuesto)
            double tParalelo = 250;    // ms (supuesto)
            int hilos = Environment.ProcessorCount;

            double speedup = tSecuencial / tParalelo;
            double eficiencia = speedup / hilos;

            Console.WriteLine($"\n=== Speedup y Eficiencia (Ejemplo) ===");
            Console.WriteLine($"Tiempo Secuencial: {tSecuencial} ms");
            Console.WriteLine($"Tiempo Paralelo:   {tParalelo} ms");
            Console.WriteLine($"Speedup:           {speedup:F2}x");
            Console.WriteLine($"Hilos (núcleos lógicos): {hilos}");
            Console.WriteLine($"Eficiencia:        {eficiencia:P2}");
        }
    }
}
