using ProyectoRobotsEspeculativos.EstadoCompartido;
using System.Threading;
using System.Threading.Tasks;

namespace ProyectoRobotsEspeculativos.Robots
{
    public interface IRobot
    {
        int Id { get; }
        string Nombre { get; }

        Task ProcesarAsync(Pieza pieza, EstadoGlobal estado, CancellationToken ct);
    }
}
