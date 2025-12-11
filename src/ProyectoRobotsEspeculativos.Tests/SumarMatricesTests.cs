using Xunit;
using ProyectoRobotsEspeculativos.Paralelismo;

public class SumarMatricesTests
{
    [Fact]
    public void SumaSecuencial_y_Paralela_DanElMismoResultado()
    {
        int n = 100;

        // Crear matrices simples para validar
        double[,] A = new double[n, n];
        double[,] B = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                A[i, j] = i + j;
                B[i, j] = (i * 2) + (j * 2);
            }
        }

        var Cseq = SimdAndParallelDemos.SumarMatrizSecuencial(A, B);
        var Cpar = SimdAndParallelDemos.SumarMatrizParalela(A, B);

        // Revisar elementos críticos
        Assert.Equal(Cseq[0, 0], Cpar[0, 0]);
        Assert.Equal(Cseq[n - 1, n - 1], Cpar[n - 1, n - 1]);

        // Revisión completa (si pasa, es 100% correcto)
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                Assert.Equal(Cseq[i, j], Cpar[i, j]);
            }
        }
    }
}
