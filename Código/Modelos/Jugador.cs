using RiskGame.Estructuras;

namespace RiskGame.Modelos
{
    public class Jugador
    {
        public string Id { get; set; }
        public string Alias { get; set; }
        public string Color { get; set; }
        public int TropasDisponibles { get; set; }
        public ListaEnlazada<Territorio> Territorios { get; set; }
        public ListaEnlazada<Carta> Cartas { get; set; }
        public bool EsTurno { get; set; }

        public Jugador(string id, string alias, string color)
        {
            Id = id;
            Alias = alias;
            Color = color;
            TropasDisponibles = 0;
            Territorios = new ListaEnlazada<Territorio>();
            Cartas = new ListaEnlazada<Carta>();
            EsTurno = false;
        }

        public void AgregarTerritorio(Territorio territorio)
        {
            if (!Territorios.Contiene(territorio))
            {
                Territorios.Agregar(territorio);
                territorio.Propietario = this;
            }
        }

        public void RemoverTerritorio(Territorio territorio)
        {
            Territorios.Remover(territorio);
        }

        public void AgregarCarta(Carta carta)
        {
            Cartas.Agregar(carta);
        }

        public bool TieneTerritorio(Territorio territorio)
        {
            return Territorios.Contiene(territorio);
        }

        public int CalcularRefuerzos()
        {
            int refuerzos = Territorios.Count / 3;
            return refuerzos < 3 ? 3 : refuerzos;
        }

        public bool PuedeIntercambiarCartas()
        {
            // Lógica para verificar si tiene 3 cartas del mismo tipo o una de cada tipo
            int infanteria = 0, caballeria = 0, artilleria = 0;

            for (int i = 0; i < Cartas.Count; i++)
            {
                switch (Cartas.Obtener(i).Tipo)
                {
                    case "Infanteria": infanteria++; break;
                    case "Caballeria": caballeria++; break;
                    case "Artilleria": artilleria++; break;
                }
            }

            return (infanteria >= 3 || caballeria >= 3 || artilleria >= 3) ||
                   (infanteria >= 1 && caballeria >= 1 && artilleria >= 1);
        }
    }
}
