using ProyectoRobotsEspeculativos.EstadoCompartido;
using ProyectoRobotsEspeculativos.Utils;

namespace ProyectoRobotsEspeculativos.Especulacion
{
    public class MotorEspeculativo
    {
        public async Task<EscenarioResultado> EjecutarMejorEscenarioAsync(
            SnapshotRobotContext snapshotBase,
            IEnumerable<Func<SnapshotRobotContext, EscenarioResultado>> generadores,
            CancellationToken ct)
        {
            var tareas = new List<Task<EscenarioResultado>>();

            foreach (var gen in generadores)
            {
                var snap = Clonar(snapshotBase);

                tareas.Add(Task.Run(() =>
                {
                    ct.ThrowIfCancellationRequested();
                    return gen(snap);
                }, ct));
            }

            var resultados = await Task.WhenAll(tareas);

            Logger.Subtitulo("Resultado de Especulación");

            foreach (var r in resultados)
                Logger.Log($"Escenario '{r.Nombre}' → Riesgo={r.Riesgo}, Tiempo={r.TiempoEstimadoMs} ms");

            return EvaluadorEscenarios.SeleccionarMejor(resultados);
        }

        private SnapshotRobotContext Clonar(SnapshotRobotContext o)
        {
            return new SnapshotRobotContext
            {
                EstadoRobot = new DatosDeRobot
                {
                    IdRobot = o.EstadoRobot.IdRobot,
                    NombreRobot = o.EstadoRobot.NombreRobot,
                    PiezaActualId = o.EstadoRobot.PiezaActualId,
                    Bateria = o.EstadoRobot.Bateria,
                    Temperatura = o.EstadoRobot.Temperatura,
                    EtapaActual = o.EstadoRobot.EtapaActual,
                    UltimaAccion = o.EstadoRobot.UltimaAccion
                },
                Pieza = o.Pieza.ClonarShallow()
            };
        }
    }
}
