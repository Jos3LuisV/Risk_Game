namespace RiskGame.Modelos
{
    public class Territorio
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int Tropas { get; set; }
        public Jugador Propietario { get; set; }
        public Continente Continente { get; set; }

        public Territorio(int id, string nombre, int tropasIniciales)
        {
            Id = id;
            Nombre = nombre;
            Tropas = tropasIniciales;
            Propietario = null;
            Continente = null;
        }

        public void Reforzar(int cantidad)
        {
            Tropas += cantidad;
        }

        public void CambiarPropietario(Jugador nuevoPropietario)
        {
            if (Propietario != null)
            {
                Propietario.RemoverTerritorio(this);
            }

            Propietario = nuevoPropietario;
            if (nuevoPropietario != null)
            {
                nuevoPropietario.AgregarTerritorio(this);
            }
        }

        public bool PerteneceA(Jugador jugador)
        {
            return Propietario != null && Propietario.Id == jugador.Id;
        }
    }
}
