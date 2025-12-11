using Xunit;
using ProyectoRobotsEspeculativos.Robots;
using ProyectoRobotsEspeculativos.Especulacion;
using ProyectoRobotsEspeculativos.EstadoCompartido;
using System.Threading;

public class MotorEspeculativoTests
{
    [Fact]
    public async Task MotorEspeculativo_DeberiaSeleccionarUnEscenarioValido()
    {
        var motor = new MotorEspeculativo();
        var robot = new RobotCrearBase(1, motor);

        var snapshot = new SnapshotRobotContext
        {
            EstadoRobot = new DatosDeRobot(),
            Pieza = new Pieza(1)
        };

        // Llamamos al método privado ObtenerEscenarios usando reflexión
        var escenarios = robot.GetType()
            .GetMethod("ObtenerEscenarios",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
            .Invoke(robot, new object[] { snapshot })
            as IEnumerable<Func<SnapshotRobotContext, EscenarioResultado>>;

        var mejor = await motor.EjecutarMejorEscenarioAsync(snapshot, escenarios, CancellationToken.None);

        Assert.NotNull(mejor);
        Assert.True(mejor.Riesgo >= 0);
        Assert.False(string.IsNullOrEmpty(mejor.Nombre));
    }
}
