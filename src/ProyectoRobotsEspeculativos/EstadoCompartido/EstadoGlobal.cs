using System.Collections.Concurrent;

namespace ProyectoRobotsEspeculativos.EstadoCompartido
{
    public class EstadoGlobal
    {
        public ConcurrentDictionary<int, DatosDeRobot> EstadoPorRobot { get; } = new();
        public ConcurrentDictionary<int, Pieza> Piezas { get; } = new();
        public ConcurrentBag<string> LogEventos { get; } = new();

        public void RegistrarEstadoRobot(int idRobot, DatosDeRobot datos)
        {
            EstadoPorRobot.AddOrUpdate(idRobot, datos, (_, _) => datos);
        }

        public void Log(string mensaje)
        {
            var linea = $"[{DateTime.Now:HH:mm:ss}] {mensaje}";
            LogEventos.Add(linea);
        }
    }

    public class DatosDeRobot
    {
        public int IdRobot { get; set; }
        public string NombreRobot { get; set; } = "";
        public int? PiezaActualId { get; set; }

        public double Bateria { get; set; }
        public double Temperatura { get; set; }
        public string EtapaActual { get; set; } = "";
        public string UltimaAccion { get; set; } = "";
    }

    public enum EtapaProceso
    {
        CrearBase = 1,
        Rellenar = 2,
        Tapar = 3,
        Transportar = 4,
        Verificar = 5,
        Sellar = 6
    }

    public class Pieza
    {
        public int Id { get; }
        public EtapaProceso EtapaActual { get; set; } = EtapaProceso.CrearBase;
        public string Estado { get; set; } = "Inicial";
        public List<string> Historial { get; } = new();

        public Pieza(int id)
        {
            Id = id;
        }

        public Pieza ClonarShallow()
        {
            var copia = new Pieza(Id)
            {
                EtapaActual = EtapaActual,
                Estado = Estado
            };
            copia.Historial.AddRange(Historial);
            return copia;
        }

        public void AgregarHistorial(string mensaje)
        {
            Historial.Add($"[{DateTime.Now:HH:mm:ss}] {mensaje}");
        }

        public override string ToString()
        {
            return $"Pieza {Id} - Etapa: {EtapaActual} - Estado: {Estado}";
        }
    }

    public class SnapshotRobotContext
    {
        public DatosDeRobot EstadoRobot { get; set; } = new();
        public Pieza Pieza { get; set; } = null!;
    }
}
