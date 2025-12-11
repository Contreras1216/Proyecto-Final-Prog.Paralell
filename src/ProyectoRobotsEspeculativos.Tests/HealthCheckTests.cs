using Xunit;
using ProyectoRobotsEspeculativos.Robots;
using ProyectoRobotsEspeculativos.Especulacion;
using ProyectoRobotsEspeculativos.EstadoCompartido;

public class HealthCheckTests
{
    [Fact]
    public void Robot_NoDebeTrabajar_Cuando_TemperaturaEsCritica()
    {
        var motor = new MotorEspeculativo();
        var robot = new RobotCrearBase(1, motor);

        var datos = new DatosDeRobot
        {
            Temperatura = 95, // crítica
            Bateria = 80
        };

        var metodo = robot
            .GetType()
            .GetMethod(
                "VerificarEstadoRobot",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance
            );

        var resultado = (HealthStatus)metodo.Invoke(robot, new object[] { datos });

        Assert.False(resultado.PuedeTrabajar);
        Assert.Contains("CRÍTICA", resultado.Estado);
    }


    [Fact]
    public void Robot_DebeTrabajar_Cuando_TodoEstaNormal()
    {
        var motor = new MotorEspeculativo();
        var robot = new RobotCrearBase(1, motor);

        var datos = new DatosDeRobot
        {
            Temperatura = 40,
            Bateria = 70
        };

        var metodo = robot
            .GetType()
            .GetMethod(
                "VerificarEstadoRobot",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance
            );

        var resultado = (HealthStatus)metodo.Invoke(robot, new object[] { datos });

        Assert.True(resultado.PuedeTrabajar);
    }
}
