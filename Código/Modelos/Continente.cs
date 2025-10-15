using RiskGame.Estructuras;

namespace RiskGame.Modelos
{
    public class Continente
    {
        public string Nombre { get; set; }
        public int Bonus { get; set; }
        public ListaEnlazada<Territorio> Territorios { get; set; }

        public Continente(string nombre, int bonus)
        {
            Nombre = nombre;
            Bonus = bonus;
            Territorios = new ListaEnlazada<Territorio>();
        }

        public void AgregarTerritorio(Territorio territorio)
        {
            if (!Territorios.Contiene(territorio))
            {
                Territorios.Agregar(territorio);
                territorio.Continente = this;
            }
        }

        public bool EstaControladoPor(Jugador jugador)
        {
            for (int i = 0; i < Territorios.Count; i++)
            {
                var territorio = Territorios.Obtener(i);
                if (territorio.Propietario == null || territorio.Propietario.Id != jugador.Id)
                    return false;
            }
            return true;
        }

        public Jugador ObtenerPropietario()
        {
            if (Territorios.Count == 0) return null;

            Jugador primerPropietario = Territorios.Obtener(0).Propietario;

            for (int i = 1; i < Territorios.Count; i++)
            {
                if (Territorios.Obtener(i).Propietario != primerPropietario)
                    return null;
            }

            return primerPropietario;
        }
    }
}