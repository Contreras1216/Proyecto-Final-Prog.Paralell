using ProyectoRobotsEspeculativos.EstadoCompartido;

namespace ProyectoRobotsEspeculativos.Especulacion
{
    public class EscenarioResultado
    {
        public string Nombre { get; set; } = "";
        public double Riesgo { get; set; }
        public double Energia { get; set; }
        public double TiempoEstimadoMs { get; set; }
        public string Descripcion { get; set; } = "";

        public SnapshotRobotContext ContextoResultante { get; set; } = null!;
    }

    public static class EvaluadorEscenarios
    {
        /// <summary>
        /// Selecciona el mejor escenario (heurística simple: menor riesgo, luego menor tiempo).
        /// </summary>
        public static EscenarioResultado SeleccionarMejor(IList<EscenarioResultado> escenarios)
        {
            return escenarios
                .OrderBy(e => e.Riesgo)
                .ThenBy(e => e.TiempoEstimadoMs)
                .First();
        }
    }
}
