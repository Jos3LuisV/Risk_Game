using RiskGame.Estructuras;

namespace RiskGame.Modelos
{
    public class Partida
    {
        public string Id { get; set; }
        public string Estado { get; set; } // "EnLobby", "EnCurso", "Finalizada"
        public ListaEnlazada<Jugador> Jugadores { get; set; }
        public Diccionario<int, Territorio> Territorios { get; set; }
        public ListaEnlazada<Continente> Continentes { get; set; }
        public Jugador JugadorActual { get; set; }
        public int Ronda { get; set; }

        public Partida(string id)
        {
            Id = id;
            Estado = "EnLobby";
            Jugadores = new ListaEnlazada<Jugador>();
            Territorios = new Diccionario<int, Territorio>();
            Continentes = new ListaEnlazada<Continente>();
            Ronda = 1;
        }

        public void AgregarJugador(Jugador jugador)
        {
            if (Estado == "EnLobby" && !Jugadores.Contiene(jugador))
            {
                Jugadores.Agregar(jugador);
            }
        }

        public void IniciarPartida()
        {
            if (Jugadores.Count >= 2 && Estado == "EnLobby")
            {
                Estado = "EnCurso";
                JugadorActual = Jugadores.Obtener(0);
                JugadorActual.EsTurno = true;
            }
        }

        public void SiguienteTurno()
        {
            if (Jugadores.Count == 0) return;

            int indiceActual = -1;
            for (int i = 0; i < Jugadores.Count; i++)
            {
                if (Jugadores.Obtener(i) == JugadorActual)
                {
                    indiceActual = i;
                    break;
                }
            }

            if (indiceActual != -1)
            {
                JugadorActual.EsTurno = false;
                int siguienteIndice = (indiceActual + 1) % Jugadores.Count;
                JugadorActual = Jugadores.Obtener(siguienteIndice);
                JugadorActual.EsTurno = true;
            }
        }

        public bool PartidaTerminada()
        {
            int jugadoresConTerritorios = 0;
            for (int i = 0; i < Jugadores.Count; i++)
            {
                if (Jugadores.Obtener(i).Territorios.Count > 0)
                {
                    jugadoresConTerritorios++;
                }
            }
            return jugadoresConTerritorios <= 1;
        }
    }
}
