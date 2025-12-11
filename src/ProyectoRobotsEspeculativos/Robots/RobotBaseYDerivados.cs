using ProyectoRobotsEspeculativos.EstadoCompartido;
using ProyectoRobotsEspeculativos.Especulacion;
using ProyectoRobotsEspeculativos.Utils;

namespace ProyectoRobotsEspeculativos.Robots
{
    // ======================================================
    // ROBOT BASE
    // ======================================================
    public abstract class RobotBase : IRobot
    {
        public int Id { get; }
        public string Nombre { get; }

        protected readonly MotorEspeculativo Motor;

        protected RobotBase(int id, string nombre, MotorEspeculativo motor)
        {
            Id = id;
            Nombre = nombre;
            Motor = motor;
        }

        // ===========================
        // HEALTH CHECK
        // ===========================
        protected HealthStatus VerificarEstadoRobot(DatosDeRobot datos)
        {
            var h = new HealthStatus
            {
                Temperatura = datos.Temperatura,
                Bateria = datos.Bateria,
                Estado = "OK",
                PuedeTrabajar = true
            };

            if (datos.Temperatura > 70)
            {
                h.Estado = "TEMPERATURA CRÍTICA";
                h.PuedeTrabajar = false;
                return h;
            }
            if (datos.Temperatura > 55)
            {
                h.Estado = "TEMPERATURA ALTA";
            }

            if (datos.Bateria < 10)
            {
                h.Estado = "BATERÍA CRÍTICA";
                h.PuedeTrabajar = false;
                return h;
            }
            if (datos.Bateria < 25)
            {
                h.Estado = "BATERÍA BAJA";
            }

            return h;
        }

        // ==========================================
        // PROCESAMIENTO PRINCIPAL
        // ==========================================
        public async Task ProcesarAsync(Pieza pieza, EstadoGlobal estado, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            Logger.RobotHeader(Nombre);
            Logger.RobotStep(Nombre, $"Iniciando procesamiento de pieza {pieza.Id}");

            // Crear snapshot
            var snapshot = new SnapshotRobotContext
            {
                EstadoRobot = new DatosDeRobot
                {
                    IdRobot = Id,
                    NombreRobot = Nombre,
                    PiezaActualId = pieza.Id,
                    Bateria = 100 - Id * 5,
                    Temperatura = 30 + Id * 2,
                    EtapaActual = pieza.EtapaActual.ToString(),
                    UltimaAccion = "Inicio"
                },
                Pieza = pieza.ClonarShallow()
            };

            // Verificar salud
            var health = VerificarEstadoRobot(snapshot.EstadoRobot);
            Logger.RobotStep(Nombre, $"Estado del robot: {health.Estado} (Temp={health.Temperatura}°C, Batt={health.Bateria}%)");

            if (!health.PuedeTrabajar)
            {
                Logger.RobotStep(Nombre, $"Robot NO puede trabajar → {health.Estado}");
                pieza.AgregarHistorial($"Robot {Nombre} detenido: {health.Estado}");
                return;
            }

            // Obtener escenarios especulativos
            var escenarios = ObtenerEscenarios(snapshot);

            var mejor = await Motor.EjecutarMejorEscenarioAsync(snapshot, escenarios, ct);

            AplicarResultadoAEstadoReal(mejor, pieza, estado);

            Logger.RobotStep(Nombre, $"Finalizó pieza {pieza.Id} con escenario '{mejor.Nombre}'.");
        }

        protected abstract IEnumerable<Func<SnapshotRobotContext, EscenarioResultado>> ObtenerEscenarios(SnapshotRobotContext snap);

        protected virtual void AplicarResultadoAEstadoReal(EscenarioResultado mejor, Pieza pieza, EstadoGlobal estado)
        {
            pieza.EtapaActual = mejor.ContextoResultante.Pieza.EtapaActual;
            pieza.Estado = mejor.ContextoResultante.Pieza.Estado;

            foreach (var h in mejor.ContextoResultante.Pieza.Historial)
                pieza.Historial.Add(h);

            pieza.AgregarHistorial($"[{mejor.Nombre}] {mejor.Descripcion}");

            estado.RegistrarEstadoRobot(Id, mejor.ContextoResultante.EstadoRobot);
        }
    }

    // ======================================================
    // ROBOT 1 — CREAR BASE
    // ======================================================
    public class RobotCrearBase : RobotBase
    {
        public RobotCrearBase(int id, MotorEspeculativo motor)
            : base(id, "Robot Crear Base", motor) { }

        protected override IEnumerable<Func<SnapshotRobotContext, EscenarioResultado>> ObtenerEscenarios(SnapshotRobotContext s)
        {
            yield return snap =>
            {
                snap.Pieza.Estado = "Base creada lentamente";
                snap.Pieza.EtapaActual = EtapaProceso.CrearBase;
                snap.Pieza.AgregarHistorial("Creación lenta de base.");
                snap.EstadoRobot.UltimaAccion = "Creación lenta";

                return new EscenarioResultado
                {
                    Nombre = "Creación lenta",
                    Riesgo = 0.2,
                    Energia = 10,
                    TiempoEstimadoMs = 130,
                    ContextoResultante = snap,
                    Descripcion = "Base generada con precisión."
                };
            };

            yield return snap =>
            {
                snap.Pieza.Estado = "Base creada rápido";
                snap.Pieza.EtapaActual = EtapaProceso.CrearBase;
                snap.Pieza.AgregarHistorial("Creación rápida de base.");
                snap.EstadoRobot.UltimaAccion = "Creación rápida";
                snap.EstadoRobot.Temperatura += 10;

                return new EscenarioResultado
                {
                    Nombre = "Creación rápida",
                    Riesgo = 0.55,
                    Energia = 22,
                    TiempoEstimadoMs = 60,
                    ContextoResultante = snap,
                    Descripcion = "Rápido pero más riesgo por calor."
                };
            };
        }
    }

    // ======================================================
    // ROBOT 2 — RELLENAR BASE
    // ======================================================
    public class RobotRellenar : RobotBase
    {
        public RobotRellenar(int id, MotorEspeculativo motor)
            : base(id, "Robot Rellenar Base", motor) { }

        protected override IEnumerable<Func<SnapshotRobotContext, EscenarioResultado>> ObtenerEscenarios(SnapshotRobotContext s)
        {
            yield return snap =>
            {
                snap.Pieza.Estado = "Base rellenada";
                snap.Pieza.EtapaActual = EtapaProceso.Rellenar;
                snap.Pieza.AgregarHistorial("Llenado normal.");

                snap.EstadoRobot.UltimaAccion = "Llenado normal";

                return new EscenarioResultado
                {
                    Nombre = "Llenado normal",
                    Riesgo = 0.3,
                    Energia = 8,
                    TiempoEstimadoMs = 80,
                    ContextoResultante = snap,
                    Descripcion = "Llenado estándar."
                };
            };
        }
    }

    // ======================================================
    // ROBOT 3 — TAPAR BASE
    // ======================================================
    public class RobotTapar : RobotBase
    {
        public RobotTapar(int id, MotorEspeculativo motor)
            : base(id, "Robot Tapar Base", motor) { }

        protected override IEnumerable<Func<SnapshotRobotContext, EscenarioResultado>> ObtenerEscenarios(SnapshotRobotContext s)
        {
            yield return snap =>
            {
                snap.Pieza.Estado = "Base tapada";
                snap.Pieza.EtapaActual = EtapaProceso.Tapar;
                snap.Pieza.AgregarHistorial("Base tapada estándar.");

                snap.EstadoRobot.UltimaAccion = "Sellado estándar";

                return new EscenarioResultado
                {
                    Nombre = "Sellado estándar",
                    Riesgo = 0.25,
                    Energia = 8,
                    TiempoEstimadoMs = 70,
                    ContextoResultante = snap,
                    Descripcion = "Sellado normal."
                };
            };
        }
    }

    // ======================================================
    // ROBOT 4 — TRANSPORTAR
    // ======================================================
    public class RobotTransportar : RobotBase
    {
        public RobotTransportar(int id, MotorEspeculativo motor)
            : base(id, "Robot Transportar", motor) { }

        protected override IEnumerable<Func<SnapshotRobotContext, EscenarioResultado>> ObtenerEscenarios(SnapshotRobotContext s)
        {
            yield return snap =>
            {
                snap.Pieza.Estado = "Transportada";
                snap.Pieza.EtapaActual = EtapaProceso.Transportar;
                snap.Pieza.AgregarHistorial("Transportada a estación.");

                snap.EstadoRobot.UltimaAccion = "Ruta directa";

                return new EscenarioResultado
                {
                    Nombre = "Ruta directa",
                    Riesgo = 0.4,
                    Energia = 12,
                    TiempoEstimadoMs = 80,
                    ContextoResultante = snap,
                    Descripcion = "Ruta rápida."
                };
            };
        }
    }

    // ======================================================
    // ROBOT 5 — VERIFICAR
    // ======================================================
    public class RobotVerificar : RobotBase
    {
        public RobotVerificar(int id, MotorEspeculativo motor)
            : base(id, "Robot Verificar", motor) { }

        protected override IEnumerable<Func<SnapshotRobotContext, EscenarioResultado>> ObtenerEscenarios(SnapshotRobotContext s)
        {
            yield return snap =>
            {
                snap.Pieza.Estado = "Verificada";
                snap.Pieza.EtapaActual = EtapaProceso.Verificar;
                snap.Pieza.AgregarHistorial("Verificación estándar.");

                snap.EstadoRobot.UltimaAccion = "Verificación";

                return new EscenarioResultado
                {
                    Nombre = "Verificación estándar",
                    Riesgo = 0.2,
                    Energia = 6,
                    TiempoEstimadoMs = 100,
                    ContextoResultante = snap,
                    Descripcion = "Verificación básica."
                };
            };
        }
    }

    // ======================================================
    // ROBOT 6 — SELLAR CAJA
    // ======================================================
    public class RobotSellar : RobotBase
    {
        public RobotSellar(int id, MotorEspeculativo motor)
            : base(id, "Robot Sellar Caja", motor) { }

        protected override IEnumerable<Func<SnapshotRobotContext, EscenarioResultado>> ObtenerEscenarios(SnapshotRobotContext s)
        {
            yield return snap =>
            {
                snap.Pieza.Estado = "Caja sellada";
                snap.Pieza.EtapaActual = EtapaProceso.Sellar;
                snap.Pieza.AgregarHistorial("Caja sellada final.");

                snap.EstadoRobot.UltimaAccion = "Sellado final";

                return new EscenarioResultado
                {
                    Nombre = "Sellado final",
                    Riesgo = 0.1,
                    Energia = 10,
                    TiempoEstimadoMs = 115,
                    ContextoResultante = snap,
                    Descripcion = "Sellado preciso."
                };
            };
        }
    }
}
