namespace ProyectoRobotsEspeculativos.EstadoCompartido
{
    public class HealthStatus
    {
        public bool PuedeTrabajar { get; set; } = true;
        public string Estado { get; set; } = "OK";
        public double Temperatura { get; set; }
        public double Bateria { get; set; }

        public override string ToString()
        {
            return $"{Estado} (Temp={Temperatura}°C, Bat={Bateria}%)";
        }
    }
}
