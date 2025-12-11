using Xunit;
using ProyectoRobotsEspeculativos.Paralelismo;

public class SimdTests
{
    [Fact]
    public void ProductoPunto_SIMD_igual_que_Secuencial()
    {
        // Preparamos dos arreglos iguales
        int n = 10_000;
        double[] a = new double[n];
        double[] b = new double[n];

        var rnd = new Random(42);
        for (int i = 0; i < n; i++)
        {
            a[i] = rnd.NextDouble();
            b[i] = rnd.NextDouble();
        }

        // Ejecutamos ambas versiones
        double secuencial = SimdAndParallelDemos.DotProductSecuencial(a, b);
        double simd = SimdAndParallelDemos.DotProductSimd(a, b);

        // Verificamos que ambos resultados sean iguales con una tolerancia mínima
        Assert.Equal(secuencial, simd, 4);
    }
}
