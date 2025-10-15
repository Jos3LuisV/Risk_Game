using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class GameServer
{
    private TcpListener tcpListener;
    private Thread listenThread;
    private bool isRunning = false;
    private int port;

    private TcpClient player1;
    private TcpClient player2;
    private NetworkStream stream1;
    private NetworkStream stream2;

    private Random random;
    private int[] distribucionTerritorios;
    private string aliasJugador1 = "Jugador1";
    private string aliasJugador2 = "Jugador2";
    private string jugadorConTurno = "NEUTRO";

    // ESTRUCTURAS PERSONALIZADAS
    private Diccionario<string, int> tropasPorJugador = new Diccionario<string, int>();
    private Diccionario<string, int> territoriosPorJugador = new Diccionario<string, int>();
    private Diccionario<int, int> tropasPorTerritorio = new Diccionario<int, int>();
    private Diccionario<int, string> propietariosTerritorios = new Diccionario<int, string>();
    private ListaEnlazada<int> territoriosNeutros = new ListaEnlazada<int>();

    // SISTEMA DE ATAQUES
    private Diccionario<int, ListaEnlazada<int>> adyacencias = new Diccionario<int, ListaEnlazada<int>>();
    private bool faseAtaque = false;
    private int territorioAtacanteSeleccionado = -1;
    private int cantidadTropasAtaque = 0;
    private string jugadorAtacante = "";

    public GameServer(int port)
    {
        this.port = port;
        this.random = new Random();
        this.distribucionTerritorios = new int[42];
        InicializarSistema();
        InicializarAdyacenciasCompletas();
        InicializarSistemaTarjetas();
    }

    //Selección de ataque
    private Diccionario<string, int> territorioAtacantePorJugador = new Diccionario<string, int>();
    private Diccionario<string, int> cantidadTropasAtaquePorJugador = new Diccionario<string, int>();

    /*
    //Escena Final
    private int territorioVictoria = 4;
    

    // VERIFICAR SI UN JUGADOR HA GANADO
    private void VerificarVictoria(string jugadorId)
    {
        // CONTAR TERRITORIOS DEL JUGADOR
        int territoriosConquistados = 0;

        for (int i = 0; i < 42; i++) // 42 territorios en total
        {
            if (propietariosTerritorios.ContieneClave(i) && propietariosTerritorios.Obtener(i) == jugadorId)
            {
                territoriosConquistados++;
            }
        }

        Console.WriteLine($"{jugadorId} tiene {territoriosConquistados}/{territorioVictoria} territorios");

        // VERIFICAR SI ALCANZÓ LA VICTORIA
        if (territoriosConquistados >= territorioVictoria)
        {
            Console.WriteLine($"¡{jugadorId} HA GANADO LA PARTIDA!");

            // ENVIAR MENSAJE DE VICTORIA A AMBOS JUGADORES
            EnviarMensajeVictoria(jugadorId, territoriosConquistados);

            // DETENER EL JUEGO
            isRunning = false;
        }
    }


    // ENVIAR MENSAJES DE VICTORIA
    private void EnviarMensajeVictoria(string jugadorGanador, int territoriosConquistados)
    {
        // PARA AMBOS JUGADORES (GANADOR Y PERDEDOR)
        SendToClient(player1, $"FIN_JUEGO:{jugadorGanador},{territoriosConquistados}");
        SendToClient(player2, $"FIN_JUEGO:{jugadorGanador},{territoriosConquistados}");

        Console.WriteLine($"Enviados mensajes de fin de juego");
    }
    */


    private void InicializarSistema()
    {
        Console.WriteLine(" Inicializando servidor Risk...");

        // Reiniciar contadores
        territoriosPorJugador.Agregar("JUGADOR1", 0);
        territoriosPorJugador.Agregar("JUGADOR2", 0);
        territoriosPorJugador.Agregar("NEUTRO", 0);

        // 40 TROPAS INICIALES
        tropasPorJugador.Agregar("JUGADOR1", 40);
        tropasPorJugador.Agregar("JUGADOR2", 40);
        tropasPorJugador.Agregar("NEUTRO", 40);

        // Inicializar todos los territorios con 1 tropa
        for (int i = 0; i < 42; i++)
        {
            tropasPorTerritorio.Agregar(i, 1);
        }
    }
    private void InicializarAdyacenciasCompletas()
    {
        Console.WriteLine(" Inicializando sistema de adyacencias completas...");

        // AMÉRICA DEL NORTE (0-8)
        AgregarConexion(0, 1); AgregarConexion(1, 2); AgregarConexion(2, 3);
        AgregarConexion(3, 4); AgregarConexion(4, 5); AgregarConexion(5, 6);
        AgregarConexion(6, 7); AgregarConexion(7, 8); AgregarConexion(0, 8);

        // OCEANÍA (9)
        AgregarConexion(9, 21); // Oceanía -> Asia (Sureste)

        // AMÉRICA DEL SUR (10-14)
        AgregarConexion(10, 11); AgregarConexion(11, 12); AgregarConexion(12, 13);
        AgregarConexion(13, 14); AgregarConexion(10, 14);
        AgregarConexion(4, 10);  // América Norte -> América Sur (Centroamérica)

        // ÁFRICA (15-20)
        AgregarConexion(15, 16); AgregarConexion(16, 17); AgregarConexion(17, 18);
        AgregarConexion(18, 19); AgregarConexion(19, 20); AgregarConexion(15, 20);

        // ASIA (21-23, 31-41)
        AgregarConexion(21, 22); AgregarConexion(22, 23); AgregarConexion(23, 31);
        AgregarConexion(31, 32); AgregarConexion(32, 33); AgregarConexion(33, 34);
        AgregarConexion(34, 35); AgregarConexion(35, 36); AgregarConexion(36, 37);
        AgregarConexion(37, 38); AgregarConexion(38, 39); AgregarConexion(39, 40);
        AgregarConexion(40, 41); AgregarConexion(21, 41);

        // EUROPA (24-30)
        AgregarConexion(24, 25); AgregarConexion(25, 26); AgregarConexion(26, 27);
        AgregarConexion(27, 28); AgregarConexion(28, 29); AgregarConexion(29, 30);
        AgregarConexion(24, 30);

        // CONEXIONES ENTRE CONTINENTES
        AgregarConexion(8, 24);   // América Norte -> Europa (Norte)
        AgregarConexion(15, 24);  // África -> Europa (Mediterráneo)
        AgregarConexion(20, 21);  // África -> Asia (Medio Oriente)
        AgregarConexion(30, 31);  // Europa -> Asia (Este)

        Console.WriteLine(" Sistema de adyacencias completas inicializado");
    }


    // SISTEMA SIMPLE DE TARJETAS
    private Diccionario<string, int[]> contadorTarjetasPorJugador = new Diccionario<string, int[]>();
    private int contadorIntercambiosGlobal = 0;
    private Random randomTarjetas = new Random();
    private int[] serieFibonacci = { 2, 3, 5, 8, 13, 21, 34, 55, 89, 144 };

    // INICIALIZAR SISTEMA DE TARJETAS
    private void InicializarSistemaTarjetas()
    {
        Console.WriteLine("Inicializando sistema simple de tarjetas...");

        // INICIALIZAR CONTADORES PARA CADA JUGADOR [Infanteria, Caballeria, Artilleria]
        contadorTarjetasPorJugador.Agregar("JUGADOR1", new int[3] { 0, 0, 0 });
        contadorTarjetasPorJugador.Agregar("JUGADOR2", new int[3] { 0, 0, 0 });
        contadorIntercambiosGlobal = 0;
    }

    // OTORGAR TARJETA POR CONQUISTA
    private void OtorgarTarjetaPorConquista(string jugadorId)
    {
        if (!contadorTarjetasPorJugador.ContieneClave(jugadorId))
            return;

        int[] tarjetasJugador = contadorTarjetasPorJugador.Obtener(jugadorId);

        // GENERAR TARJETA ALEATORIA (0=Infanteria, 1=Caballeria, 2=Artilleria)
        int tipoIndex = randomTarjetas.Next(0, 3);
        tarjetasJugador[tipoIndex]++;

        // ENVIAR AL JUGADOR
        SendToClient(GetClientByPlayerId(jugadorId), $"TARJETA_OBTENIDA:{tipoIndex}");

        string nombreTarjeta = ObtenerNombreTarjeta(tipoIndex);
        Console.WriteLine($"{jugadorId} recibió tarjeta: {nombreTarjeta}");

        // VERIFICAR SI TIENE TRÍO PARA INTERCAMBIO
        VerificarTrioTarjetas(jugadorId);
    }

    // OBTENER NOMBRE DE TARJETA
    private string ObtenerNombreTarjeta(int tipoIndex)
    {
        switch (tipoIndex)
        {
            case 0: return "Infantería";
            case 1: return "Caballería";
            case 2: return "Artillería";
            default: return "Desconocida";
        }
    }

    // VERIFICAR SI HAY TRÍO PARA INTERCAMBIO
    private void VerificarTrioTarjetas(string jugadorId)
    {
        int[] tarjetas = contadorTarjetasPorJugador.Obtener(jugadorId);

        // VERIFICAR TRÍOS VÁLIDOS
        bool tieneTrioDiferente = tarjetas[0] >= 1 && tarjetas[1] >= 1 && tarjetas[2] >= 1;
        bool tieneTrioIgual = tarjetas[0] >= 3 || tarjetas[1] >= 3 || tarjetas[2] >= 3;

        if (tieneTrioDiferente || tieneTrioIgual)
        {
            SendToClient(GetClientByPlayerId(jugadorId), "TARJETAS_TRIO_DISPONIBLE");
            Console.WriteLine($"{jugadorId} tiene trío disponible para intercambio");
        }
    }

    // INTERCAMBIAR TARJETAS
    private void IntercambiarTarjetas(string jugadorId)
    {
        if (!contadorTarjetasPorJugador.ContieneClave(jugadorId))
            return;

        int[] tarjetas = contadorTarjetasPorJugador.Obtener(jugadorId);

        // VERIFICAR QUE TIENE AL MENOS 3 TARJETAS EN TOTAL
        int totalTarjetas = tarjetas[0] + tarjetas[1] + tarjetas[2];
        if (totalTarjetas < 3)
        {
            SendToClient(GetClientByPlayerId(jugadorId), "ERROR:Necesitas al menos 3 tarjetas para intercambiar");
            return;
        }

        // VERIFICAR QUE TIENE TRÍO VÁLIDO
        bool tieneTrioDiferente = tarjetas[0] >= 1 && tarjetas[1] >= 1 && tarjetas[2] >= 1;
        bool tieneTrioIgual = tarjetas[0] >= 3 || tarjetas[1] >= 3 || tarjetas[2] >= 3;

        if (!tieneTrioDiferente && !tieneTrioIgual)
        {
            SendToClient(GetClientByPlayerId(jugadorId), "ERROR:No tienes un trío válido para intercambiar");
            return;
        }

        // ELIMINAR TARJETAS SEGÚN EL TRÍO
        if (tieneTrioDiferente)
        {
            // ELIMINAR 1 DE CADA TIPO
            tarjetas[0]--;
            tarjetas[1]--;
            tarjetas[2]--;
            Console.WriteLine($"{jugadorId} intercambió trío diferente");
        }
        else if (tieneTrioIgual)
        {
            // ELIMINAR 3 DEL MISMO TIPO
            if (tarjetas[0] >= 3)
            {
                tarjetas[0] -= 3;
                Console.WriteLine($"{jugadorId} intercambió trío de infantería");
            }
            else if (tarjetas[1] >= 3)
            {
                tarjetas[1] -= 3;
                Console.WriteLine($"{jugadorId} intercambió trío de caballería");
            }
            else if (tarjetas[2] >= 3)
            {
                tarjetas[2] -= 3;
                Console.WriteLine($"{jugadorId} intercambió trío de artillería");
            }
        }

        // CALCULAR REFUERZOS SEGÚN FIBONACCI
        int refuerzos = ObtenerRefuerzosPorIntercambio();
        contadorIntercambiosGlobal++;

        // OTORGAR REFUERZOS AL JUGADOR
        int tropasActuales = tropasPorJugador.Obtener(jugadorId);
        tropasPorJugador.Actualizar(jugadorId, tropasActuales + refuerzos);

        // ENVIAR RESULTADO AL JUGADOR
        SendToClient(GetClientByPlayerId(jugadorId), $"TARJETAS_INTERCAMBIADAS:{refuerzos}");
        SendToClient(GetClientByPlayerId(jugadorId), $"ACTUALIZAR_TARJETAS:{tarjetas[0]},{tarjetas[1]},{tarjetas[2]}");
        SendToClient(GetClientByPlayerId(jugadorId), $"TROPAS_DISPONIBLES:{tropasPorJugador.Obtener(jugadorId)}");

        Console.WriteLine($"{jugadorId} intercambió tarjetas por {refuerzos} tropas (Intercambio #{contadorIntercambiosGlobal})");
    }

    // OBTENER REFUERZOS POR FIBONACCI
    private int ObtenerRefuerzosPorIntercambio()
    {
        if (contadorIntercambiosGlobal < serieFibonacci.Length)
        {
            return serieFibonacci[contadorIntercambiosGlobal];
        }
        else
        {
            return serieFibonacci[serieFibonacci.Length - 1];
        }
    }

    private void AgregarConexion(int territorio1, int territorio2)
    {
        if (!adyacencias.ContieneClave(territorio1))
            adyacencias.Agregar(territorio1, new ListaEnlazada<int>());
        if (!adyacencias.ContieneClave(territorio2))
            adyacencias.Agregar(territorio2, new ListaEnlazada<int>());

        if (!adyacencias.Obtener(territorio1).Contiene(territorio2))
            adyacencias.Obtener(territorio1).Agregar(territorio2);
        if (!adyacencias.Obtener(territorio2).Contiene(territorio1))
            adyacencias.Obtener(territorio2).Agregar(territorio1);
    }
    public void Start()
    {
        isRunning = true;
        tcpListener = new TcpListener(IPAddress.Any, port);
        listenThread = new Thread(new ThreadStart(ListenForClients));
        listenThread.IsBackground = true;
        listenThread.Start();

        Console.WriteLine($" Servidor Risk iniciado en puerto {port}");
        Console.WriteLine("Esperando 2 jugadores...");
    }
    private void ListenForClients()
    {
        tcpListener.Start();

        while (isRunning)
        {
            try
            {
                player1 = tcpListener.AcceptTcpClient();
                stream1 = player1.GetStream();
                Console.WriteLine(" Jugador 1 conectado");
                SendToClient(player1, "ID:JUGADOR1");

                player2 = tcpListener.AcceptTcpClient();
                stream2 = player2.GetStream();
                Console.WriteLine(" Jugador 2 conectado");
                SendToClient(player2, "ID:JUGADOR2");

                Console.WriteLine(" Esperando aliases de los jugadores...");
                Thread.Sleep(1000);

                SendToAll($"JUGADOR1_ALIAS:{aliasJugador1}");
                SendToAll($"JUGADOR2_ALIAS:{aliasJugador2}");

                DistribuirTerritorios();
                IniciarFaseColocacion();

                Thread player1Thread = new Thread(() => HandleClient(player1, "JUGADOR1"));
                Thread player2Thread = new Thread(() => HandleClient(player2, "JUGADOR2"));
                player1Thread.Start();
                player2Thread.Start();

                SendToAll("INICIAR_JUEGO");
                Console.WriteLine("Partida Risk iniciada!");

            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error: {ex.Message}");
                if (isRunning) Thread.Sleep(1000);
            }
        }
    }
    // EN EL SERVIDOR - Agregar estas variables
    private bool faseDistribucion = true;
    private bool fasePrincipal = false;

    private void DistribuirTerritorios()
    {
        Console.WriteLine(" Distribuyendo 42 territorios entre 3 facciones...");

        int[] todosTerritoriosArray = new int[42];
        for (int i = 0; i < 42; i++)
        {
            todosTerritoriosArray[i] = i;
        }

        // array
        for (int i = 41; i > 0; i--)
        {
            int j = random.Next(i + 1);
            int temp = todosTerritoriosArray[i];
            todosTerritoriosArray[i] = todosTerritoriosArray[j];
            todosTerritoriosArray[j] = temp;
        }

        ListaEnlazada<int> todosTerritorios = new ListaEnlazada<int>();
        for (int i = 0; i < 42; i++)
        {
            todosTerritorios.Agregar(todosTerritoriosArray[i]);
        }
        // REINICIAR CONTADORES
        territoriosPorJugador.Actualizar("JUGADOR1", 0);
        territoriosPorJugador.Actualizar("JUGADOR2", 0);
        territoriosPorJugador.Actualizar("NEUTRO", 0);
        territoriosNeutros.Limpiar();
        propietariosTerritorios.Limpiar();

        tropasPorTerritorio.Limpiar();
        for (int i = 0; i < 42; i++)
        {
            tropasPorTerritorio.Agregar(i, 1); // TODOS EMPIEZAN CON 1 TROPA
        }

        for (int i = 0; i < 42; i++)
        {
            string propietario;
            if (i < 14)
            {
                propietario = "JUGADOR1";
                territoriosPorJugador.Actualizar("JUGADOR1", territoriosPorJugador.Obtener("JUGADOR1") + 1);
            }
            else if (i < 28)
            {
                propietario = "JUGADOR2";
                territoriosPorJugador.Actualizar("JUGADOR2", territoriosPorJugador.Obtener("JUGADOR2") + 1);
            }
            else
            {
                propietario = "NEUTRO";
                territoriosPorJugador.Actualizar("NEUTRO", territoriosPorJugador.Obtener("NEUTRO") + 1);
                territoriosNeutros.Agregar(todosTerritoriosArray[i]);
            }

            int territorioId = todosTerritoriosArray[i];
            propietariosTerritorios.Agregar(territorioId, propietario);

            // ENVIAR SIEMPRE 1 TROPA INICIAL
            string mensaje = $"TERRITORIO_ASIGNADO:{territorioId},{propietario},1";
            SendToAll(mensaje);

            Console.WriteLine($" Territorio {territorioId} -> {propietario} (1 tropa)");
        }

        Console.WriteLine($" Distribución completada. NEUTRO tiene {territoriosNeutros.Count} territorios");
    }

    // EN EL SERVIDOR - Modificar IniciarFaseColocacion para que JUGADORES tengan 26 tropas
    private void IniciarFaseColocacion()
    {
        Console.WriteLine(" Iniciando fase de colocación de tropas...");

        // TODOS RECIBEN 26 TROPAS: JUGADORES Y NEUTRO
        tropasPorJugador.Actualizar("JUGADOR1", 26);
        tropasPorJugador.Actualizar("JUGADOR2", 26);
        tropasPorJugador.Actualizar("NEUTRO", 26);

        Console.WriteLine($" Tropas para colocar:");
        Console.WriteLine($"   JUGADOR1: {tropasPorJugador.Obtener("JUGADOR1")} tropas");
        Console.WriteLine($"   JUGADOR2: {tropasPorJugador.Obtener("JUGADOR2")} tropas");
        Console.WriteLine($"   NEUTRO: {tropasPorJugador.Obtener("NEUTRO")} tropas");

        // ENVIAR 26 TROPAS A TODOS
        SendToClient(player1, "TROPAS_DISPONIBLES:26");
        SendToClient(player2, "TROPAS_DISPONIBLES:26");

        // INICIAR CON NEUTRO PARA QUE COLOQUE SUS TROPAS PRIMERO
        jugadorConTurno = "NEUTRO";
        SendToClient(player1, "TURNO:ESPERA");
        SendToClient(player2, "TURNO:ESPERA");
        SendToAll("FASE_COLOCACION:INICIADA");

        Console.WriteLine($" NEUTRO colocando sus 26 tropas...");

        // INICIAR COLOCACIÓN DEL NEUTRO
        ProcesarColocacionNeutro();
    }

    private void ProcesarTurnoNeutro()
    {
        if (tropasPorJugador.Obtener("NEUTRO") > 0 && territoriosNeutros.Count > 0)
        {
            int indiceTerritorio = random.Next(territoriosNeutros.Count);
            int territorioId = 0;

            for (int i = 0; i <= indiceTerritorio; i++)
            {
                territorioId = territoriosNeutros.Obtener(i);
            }

            tropasPorJugador.Actualizar("NEUTRO", tropasPorJugador.Obtener("NEUTRO") - 1);
            tropasPorTerritorio.Actualizar(territorioId, tropasPorTerritorio.Obtener(territorioId) + 1);

            string mensajeActualizacion = $"TROPAS_ACTUALIZADAS:{territorioId},NEUTRO,{tropasPorTerritorio.Obtener(territorioId)}";
            SendToAll(mensajeActualizacion);

            Console.WriteLine($" NEUTRO colocó 1 tropa en territorio {territorioId} | Total: {tropasPorTerritorio.Obtener(territorioId)}");

            Thread.Sleep(1000);
            SiguienteTurno();
        }
        else
        {
            Console.WriteLine(" NEUTRO ha terminado de colocar tropas");
            jugadorConTurno = "JUGADOR1";
            SendToClient(player1, "TURNO:ACTIVO");
            SendToClient(player2, "TURNO:ESPERA");
            SendToAll("FASE_COLOCACION:INICIADA");
        }
    }
    private void SiguienteTurno()
    {
        Console.WriteLine($" Cambiando turno... Jugador actual: {jugadorConTurno}");

        if (jugadorConTurno == "NEUTRO")
        {
            // EL NEUTRO YA COLOCÓ TODAS SUS TROPAS (se maneja en ProcesarColocacionNeutro)
            // Pasar directamente a JUGADOR1
            jugadorConTurno = "JUGADOR1";
            SendToClient(player1, "TURNO:ACTIVO");
            SendToClient(player2, "TURNO:ESPERA");
            Console.WriteLine($" Turno cambiado a JUGADOR1");
        }
        else if (jugadorConTurno == "JUGADOR1")
        {
            if (tropasPorJugador.Obtener("JUGADOR2") > 0)
            {
                jugadorConTurno = "JUGADOR2";
                SendToClient(player1, "TURNO:ESPERA");
                SendToClient(player2, "TURNO:ACTIVO");
                Console.WriteLine($" Turno cambiado a JUGADOR2");
            }
            else if (tropasPorJugador.Obtener("JUGADOR1") > 0)
            {
                // JUGADOR1 tiene más tropas, continuar con él
                Console.WriteLine($" JUGADOR1 continúa colocando tropas");
            }
            else
            {
                FinalizarFaseColocacion();
            }
        }
        else if (jugadorConTurno == "JUGADOR2")
        {
            if (tropasPorJugador.Obtener("JUGADOR1") > 0)
            {
                jugadorConTurno = "JUGADOR1";
                SendToClient(player1, "TURNO:ACTIVO");
                SendToClient(player2, "TURNO:ESPERA");
                Console.WriteLine($" Turno cambiado a JUGADOR1");
            }
            else if (tropasPorJugador.Obtener("JUGADOR2") > 0)
            {
                // JUGADOR2 tiene más tropas, continuar con él
                Console.WriteLine($" JUGADOR2 continúa colocando tropas");
            }
            else
            {
                FinalizarFaseColocacion();
            }
        }
    }

    // MÉTODO CORREGIDO - RESPETA LÍMITE DE 4 TROPAS
    private void ProcesarColocacionNeutro()
    {
        Console.WriteLine($" NEUTRO colocando tropas... Tropas restantes: {tropasPorJugador.Obtener("NEUTRO")}");

        // VERIFICAR SI TODOS LOS TERRITORIOS NEUTROS YA TIENEN 4 TROPAS
        bool todosLlenos = true;
        for (int i = 0; i < territoriosNeutros.Count; i++)
        {
            int territorioId = territoriosNeutros.Obtener(i);
            if (tropasPorTerritorio.Obtener(territorioId) < 4)
            {
                todosLlenos = false;
                break;
            }
        }

        if (todosLlenos && tropasPorJugador.Obtener("NEUTRO") > 0)
        {
            Console.WriteLine(" TODOS los territorios neutros tienen 4 tropas. NO se pueden colocar más.");
            tropasPorJugador.Actualizar("NEUTRO", 0); // FORZAR A 0 TROPAS RESTANTES
            Console.WriteLine(" NEUTRO terminó de colocar tropas (todos los territorios llenos)");
            SiguienteTurno();
            return;
        }
        if (tropasPorJugador.Obtener("NEUTRO") > 0 && territoriosNeutros.Count > 0)
        {
            // Buscar SOLO territorios que tengan MENOS de 4 tropas
            ListaEnlazada<int> territoriosDisponibles = new ListaEnlazada<int>();

            for (int i = 0; i < territoriosNeutros.Count; i++)
            {
                int territorioId = territoriosNeutros.Obtener(i);
                int tropasActuales = tropasPorTerritorio.Obtener(territorioId);

                // VERIFICACIÓN ESTRICTA: SOLO territorios con MENOS de 4 tropas
                if (tropasActuales < 4)
                {
                    territoriosDisponibles.Agregar(territorioId);
                    Console.WriteLine($"   Territorio {territorioId} disponible ({tropasActuales}/4 tropas)");
                }
            }

            if (territoriosDisponibles.Count > 0)
            {
                // Seleccionar un territorio aleatorio de los disponibles
                int indiceTerritorio = random.Next(territoriosDisponibles.Count);
                int territorioId = territoriosDisponibles.Obtener(indiceTerritorio);
                int tropasActuales = tropasPorTerritorio.Obtener(territorioId);

                // VERIFICACIÓN FINAL: Asegurar que no exceda el límite
                if (tropasActuales < 4)
                {
                    // Colocar 1 tropa
                    tropasPorJugador.Actualizar("NEUTRO", tropasPorJugador.Obtener("NEUTRO") - 1);
                    tropasPorTerritorio.Actualizar(territorioId, tropasActuales + 1);

                    string mensajeActualizacion = $"TROPAS_ACTUALIZADAS:{territorioId},NEUTRO,{tropasActuales + 1}";
                    SendToAll(mensajeActualizacion);

                    Console.WriteLine($" NEUTRO colocó 1 tropa en territorio {territorioId} | Total: {tropasActuales + 1}/4 | Restantes: {tropasPorJugador.Obtener("NEUTRO")}");

                    // Continuar colocando después de un breve delay
                    Thread.Sleep(300);
                    ProcesarColocacionNeutro();
                }
                else
                {
                    Console.WriteLine($" Territorio {territorioId} ya tiene {tropasActuales} tropas (límite alcanzado)");
                    ProcesarColocacionNeutro(); // Reintentar con otro territorio
                }
            }
            else
            {
                Console.WriteLine(" NEUTRO no tiene territorios disponibles (todos tienen 4 tropas)");
                tropasPorJugador.Actualizar("NEUTRO", 0); // FORZAR A 0
                Console.WriteLine(" NEUTRO terminó de colocar tropas");
                SiguienteTurno();
            }
        }
        else
        {
            Console.WriteLine(" NEUTRO terminó de colocar tropas");
            SiguienteTurno();
        }
    }

    // Todos los territorios asignados
    private void FinalizarFaseColocacion()
    {
        Console.WriteLine(" Fase de colocación completada!");

        // SOLO MARCAR QUE TERMINÓ LA FASE DE COLOCACIÓN INICIAL
        faseDistribucion = false;

        SendToAll("FASE_COLOCACION:COMPLETADA");

        // INICIAR CON JUGADOR1 - PERMITIR AMBAS ACCIONES DESDE EL INICIO
        jugadorConTurno = "JUGADOR1";
        SendToClient(player1, "TURNO:ACTIVO");
        SendToClient(player2, "TURNO:ESPERA");
        SendToAll("MODO_ATAQUE:DISPONIBLE");
        SendToAll("FASE_PRINCIPAL:INICIADA");

        // ENVIAR TROPAS DISPONIBLES ACTUALES
        int tropasJ1 = tropasPorJugador.Obtener("JUGADOR1");
        int tropasJ2 = tropasPorJugador.Obtener("JUGADOR2");

        SendToClient(player1, $"TROPAS_DISPONIBLES:{tropasJ1}");
        SendToClient(player2, $"TROPAS_DISPONIBLES:{tropasJ2}");

        Console.WriteLine($" Fase principal iniciada - Los jugadores pueden COLOCAR o ATACAR");
    }

    private void HandleClient(TcpClient client, string playerId)
    {
        byte[] buffer = new byte[4096];
        NetworkStream stream = client.GetStream();

        while (isRunning && client.Connected)
        {
            try
            {
                if (!stream.DataAvailable)
                {
                    Thread.Sleep(50);
                    continue;
                }

                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                Console.WriteLine($" {playerId} dice: {message}");

                if (message.StartsWith("ALIAS:"))
                {
                    ProcesarAlias(playerId, message);
                }
                else if (message.StartsWith("COLOCAR_TROPAS:"))
                {
                    ProcesarColocacionTropas(playerId, message);
                }
                else if (message == "INTERCAMBIAR_TARJETAS")
                {
                    IntercambiarTarjetas(playerId);
                }
                else if (message == "TERMINAR_TURNO")
                {
                    ProcesarFinTurno(playerId);
                }
                else if (message.StartsWith("SOLICITAR_ATACABLES:"))
                {
                    ProcesarSolicitudAtacables(playerId, message);
                }
                else if (message.StartsWith("SELECCIONAR_ATAQUE:"))
                {
                    ProcesarSeleccionAtaque(playerId, message);
                }
                else if (message.StartsWith("ATACAR:"))
                {
                    ProcesarAtaque(playerId, message);
                }
                else
                {
                    Console.WriteLine($" Comando no reconocido: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error con {playerId}: {ex.Message}");
                break;
            }
        }

        CleanupPlayerConnection(playerId, client);
    }

    // Fase para atacar
    private void ProcesarSolicitudAtacables(string playerId, string mensaje)
    {
        // VERIFICAR TURNO
        if (playerId != jugadorConTurno)
        {
            SendToClient(GetClientByPlayerId(playerId), "ERROR:No puedes atacar en este momento");
            return;
        }
        try
        {
            string[] partes = mensaje.Split(':');
            int territorioId = int.Parse(partes[1]);

            // Verificar que el territorio pertenece al jugador
            if (propietariosTerritorios.Obtener(territorioId) != playerId)
            {
                SendToClient(GetClientByPlayerId(playerId), "ERROR:Territorio no te pertenece");
                return;
            }
            // Verificar que tiene suficientes tropas para atacar
            int tropasEnTerritorio = tropasPorTerritorio.Obtener(territorioId);
            if (tropasEnTerritorio < 2)
            {
                SendToClient(GetClientByPlayerId(playerId), "ERROR:Necesitas al menos 2 tropas para atacar");
                return;
            }

            // Enviar territorios adyacentes que puede atacar
            ListaEnlazada<int> territoriosAtacables = ObtenerTerritoriosAtacables(territorioId, playerId);

            if (territoriosAtacables.Count == 0)
            {
                SendToClient(GetClientByPlayerId(playerId), "ERROR:No hay territorios enemigos adyacentes para atacar");
                return;
            }
            string mensajeTerritorios = "TERRITORIOS_ATACABLES:";
            for (int i = 0; i < territoriosAtacables.Count; i++)
            {
                mensajeTerritorios += territoriosAtacables.Obtener(i);
                if (i < territoriosAtacables.Count - 1) mensajeTerritorios += ",";
            }

            SendToClient(GetClientByPlayerId(playerId), mensajeTerritorios);
            Console.WriteLine($" {playerId} solicitó territorios atacables desde {territorioId} - Enviados: {territoriosAtacables.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error procesando solicitud de atacables: {ex.Message}");
        }
    }

    // Seleccionar territorio enemigo
    private void ProcesarSeleccionAtaque(string playerId, string mensaje)
    {
        Console.WriteLine($"SELECCIÓN ATAQUE: Jugador {playerId}, Mensaje: {mensaje}");

        if (playerId != jugadorConTurno)
        {
            SendToClient(GetClientByPlayerId(playerId), "ERROR:No es tu turno");
            Console.WriteLine($"{playerId} intentó seleccionar ataque fuera de turno");
            return;
        }
        try
        {
            string[] partes = mensaje.Split(':');
            string[] datos = partes[1].Split(',');

            int territorioId = int.Parse(datos[0]);
            int cantidadTropas = int.Parse(datos[1]);

            // Verificar que el territorio pertenece al jugador
            if (!propietariosTerritorios.ContieneClave(territorioId) || propietariosTerritorios.Obtener(territorioId) != playerId)
            {
                SendToClient(GetClientByPlayerId(playerId), "ERROR:Territorio no te pertenece");
                Console.WriteLine($"{playerId} intentó atacar desde territorio {territorioId} que no le pertenece");
                return;
            }

            // Verificar que tiene suficientes tropas para atacar
            int tropasEnTerritorio = tropasPorTerritorio.Obtener(territorioId);
            if (tropasEnTerritorio < 2)
            {
                SendToClient(GetClientByPlayerId(playerId), "ERROR:Necesitas al menos 2 tropas para atacar");
                Console.WriteLine($"{playerId} intentó atacar con {tropasEnTerritorio} tropas (mínimo 2)");
                return;
            }
            if (cantidadTropas < 1 || cantidadTropas > 3)
            {
                SendToClient(GetClientByPlayerId(playerId), "ERROR:Puedes atacar con 1, 2 o 3 tropas");
                Console.WriteLine($"{playerId} intentó atacar con {cantidadTropas} tropas (1-3 permitido)");
                return;
            }

            if (cantidadTropas >= tropasEnTerritorio)
            {
                SendToClient(GetClientByPlayerId(playerId), "ERROR:Debes dejar al menos 1 tropa en el territorio");
                Console.WriteLine($"{playerId} intentó atacar con {cantidadTropas} tropas pero solo tiene {tropasEnTerritorio}");
                return;
            }

            // GUARDAR SELECCIÓN POR JUGADOR
            if (territorioAtacantePorJugador.ContieneClave(playerId))
            {
                territorioAtacantePorJugador.Actualizar(playerId, territorioId);
            }
            else
            {
                territorioAtacantePorJugador.Agregar(playerId, territorioId);
            }

            if (cantidadTropasAtaquePorJugador.ContieneClave(playerId))
            {
                cantidadTropasAtaquePorJugador.Actualizar(playerId, cantidadTropas);
            }
            else
            {
                cantidadTropasAtaquePorJugador.Agregar(playerId, cantidadTropas);
            }

            // GUARDAR EN VARIABLES GLOBALES PARA COMPATIBILIDAD
            territorioAtacanteSeleccionado = territorioId;
            cantidadTropasAtaque = cantidadTropas;
            jugadorAtacante = playerId;

            Console.WriteLine($"{playerId} seleccionó ataque desde territorio {territorioId} con {cantidadTropas} tropas");

            // Enviar territorios adyacentes que puede atacar
            ListaEnlazada<int> territoriosAtacables = ObtenerTerritoriosAtacables(territorioId, playerId);

            if (territoriosAtacables.Count == 0)
            {
                SendToClient(GetClientByPlayerId(playerId), "ERROR:No hay territorios enemigos adyacentes para atacar");
                // Resetear selección
                LimpiarSeleccionAtaque(playerId);
                return;
            }

            string mensajeTerritorios = "TERRITORIOS_ATACABLES:";
            for (int i = 0; i < territoriosAtacables.Count; i++)
            {
                mensajeTerritorios += territoriosAtacables.Obtener(i);
                if (i < territoriosAtacables.Count - 1) mensajeTerritorios += ",";
            }

            SendToClient(GetClientByPlayerId(playerId), mensajeTerritorios);
            Console.WriteLine($"Enviados {territoriosAtacables.Count} territorios atacables a {playerId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error procesando selección de ataque: {ex.Message}");
            SendToClient(GetClientByPlayerId(playerId), "ERROR:Error en selección de ataque");
        }
    }

    // ObtenerTerritoriosAtacables para verificar adyacencia
    private ListaEnlazada<int> ObtenerTerritoriosAtacables(int territorioOrigen, string jugadorAtacante)
    {
        ListaEnlazada<int> territoriosAtacables = new ListaEnlazada<int>();

        if (!adyacencias.ContieneClave(territorioOrigen))
        {
            Console.WriteLine($" Territorio {territorioOrigen} no tiene adyacencias configuradas");
            return territoriosAtacables;
        }

        var adyacentes = adyacencias.Obtener(territorioOrigen);
        Console.WriteLine($" Territorio {territorioOrigen} tiene {adyacentes.Count} territorios adyacentes");

        for (int i = 0; i < adyacentes.Count; i++)
        {
            int territorioAdyacente = adyacentes.Obtener(i);

            // Verificar que existe el territorio
            if (!propietariosTerritorios.ContieneClave(territorioAdyacente))
            {
                Console.WriteLine($" Territorio adyacente {territorioAdyacente} no existe en propietarios");
                continue;
            }
            string propietario = propietariosTerritorios.Obtener(territorioAdyacente);

            // Solo territorios que NO sean del jugador atacante
            if (propietario != jugadorAtacante)
            {
                territoriosAtacables.Agregar(territorioAdyacente);
                Console.WriteLine($" Territorio {territorioAdyacente} es atacable (Propietario: {propietario})");
            }
            else
            {
                Console.WriteLine($" Territorio {territorioAdyacente} no es atacable (Propietario: {propietario} = Atacante)");
            }
        }

        Console.WriteLine($" Total territorios atacables desde {territorioOrigen}: {territoriosAtacables.Count}");
        return territoriosAtacables;
    }

    // ProcesarAtaque para usar selección por jugador
    private void ProcesarAtaque(string playerId, string mensaje)
    {
        Console.WriteLine($"PROCESAR ATAQUE: Jugador {playerId}, Mensaje: {mensaje}");

        // Verificar que es el turno del jugador
        if (playerId != jugadorConTurno)
        {
            SendToClient(GetClientByPlayerId(playerId), "ERROR:No es tu turno");
            Console.WriteLine($"{playerId} intentó atacar fuera de turno");
            return;
        }

        // SELECCIÓN USANDO EL SISTEMA POR JUGADOR
        if (!territorioAtacantePorJugador.ContieneClave(playerId) || !cantidadTropasAtaquePorJugador.ContieneClave(playerId))
        {
            SendToClient(GetClientByPlayerId(playerId), "ERROR:No hay selección de ataque activa");
            Console.WriteLine($"{playerId} intentó atacar sin selección activa de territorio");
            return;
        }

        // OBTENER SELECCIÓN DEL JUGADOR
        int territorioAtacante = territorioAtacantePorJugador.Obtener(playerId);
        int tropasAtaque = cantidadTropasAtaquePorJugador.Obtener(playerId);

        try
        {
            string[] partes = mensaje.Split(':');
            int territorioDefensorId = int.Parse(partes[1]);

            Console.WriteLine($"Procesando ataque: {territorioAtacante} -> {territorioDefensorId} con {tropasAtaque} tropas");

            // ✅ REGLA: Verificar que el territorio defensor es adyacente y enemigo
            if (!EsTerritorioAtacable(territorioAtacante, territorioDefensorId, playerId))
            {
                SendToClient(GetClientByPlayerId(playerId), "ERROR:Territorio no atacable");
                Console.WriteLine($"Territorio {territorioDefensorId} no es atacable desde {territorioAtacante}");
                return;
            }

            // Verificar que el territorio atacante aún tiene suficientes tropas
            int tropasActualesAtacante = tropasPorTerritorio.Obtener(territorioAtacante);
            if (tropasActualesAtacante < 2)
            {
                SendToClient(GetClientByPlayerId(playerId), "ERROR:El territorio atacante ya no tiene suficientes tropas");
                Console.WriteLine($"Territorio {territorioAtacante} ahora tiene {tropasActualesAtacante} tropas (mínimo 2)");
                LimpiarSeleccionAtaque(playerId);
                return;
            }
            if (tropasAtaque >= tropasActualesAtacante)
            {
                SendToClient(GetClientByPlayerId(playerId), "ERROR:No puedes usar más tropas de las disponibles");
                Console.WriteLine($"Intentó usar {tropasAtaque} tropas pero solo hay {tropasActualesAtacante}");
                LimpiarSeleccionAtaque(playerId);
                return;
            }

            // Realizar combate
            RealizarCombate(territorioAtacante, territorioDefensorId, tropasAtaque, playerId);

            // SOLO 1 ATAQUE POR TURNO - Cambiar turno automáticamente después del ataque
            CambiarTurno(playerId);

            // LIMPIAR SELECCIÓN DESPUÉS DEL ATAQUE
            LimpiarSeleccionAtaque(playerId);

            Console.WriteLine($"Ataque completado - Turno cambiado");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error procesando ataque: {ex.Message}");
            SendToClient(GetClientByPlayerId(playerId), "ERROR:Error en el ataque");
            LimpiarSeleccionAtaque(playerId);
        }
    }

    // Método para limpiar selección de ataque
    private void LimpiarSeleccionAtaque(string playerId)
    {
        // Limpiar sistema por jugador
        if (territorioAtacantePorJugador.ContieneClave(playerId))
        {
            territorioAtacantePorJugador.Remover(playerId);
        }

        if (cantidadTropasAtaquePorJugador.ContieneClave(playerId))
        {
            cantidadTropasAtaquePorJugador.Remover(playerId);
        }

        // Limpiar variables globales (para compatibilidad)
        territorioAtacanteSeleccionado = -1;
        cantidadTropasAtaque = 0;
        jugadorAtacante = "";

        Console.WriteLine($"Selección de ataque limpiada para {playerId}");
    }

    // CambiarTurno para limpiar selecciones
    private void CambiarTurno(string jugadorActual)
    {
        // LIMPIAR SELECCIÓN DEL JUGADOR ACTUAL ANTES DE CAMBIAR TURNO
        LimpiarSeleccionAtaque(jugadorActual);

        // SOLO CAMBIAR ENTRE JUGADOR1 Y JUGADOR2
        string nuevoTurno = (jugadorActual == "JUGADOR1") ? "JUGADOR2" : "JUGADOR1";
        jugadorConTurno = nuevoTurno;

        // Enviar cambio de turno a los clientes
        SendToClient(player1, $"TURNO:{(nuevoTurno == "JUGADOR1" ? "ACTIVO" : "ESPERA")}");
        SendToClient(player2, $"TURNO:{(nuevoTurno == "JUGADOR2" ? "ACTIVO" : "ESPERA")}");

        // ENVIAR TROPAS DISPONIBLES AL NUEVO JUGADOR
        int tropasDisponibles = tropasPorJugador.Obtener(nuevoTurno);
        SendToClient(GetClientByPlayerId(nuevoTurno), $"TROPAS_DISPONIBLES:{tropasDisponibles}");

        Console.WriteLine($"Turno cambiado a {nuevoTurno} - Tropas disponibles: {tropasDisponibles}");
    }

    // Verificar adyacencia en EsTerritorioAtacable
    private bool EsTerritorioAtacable(int territorioOrigen, int territorioDestino, string jugadorAtacante)
    {
        // Verificar que el territorio origen existe y tiene adyacencias
        if (!adyacencias.ContieneClave(territorioOrigen))
        {
            Console.WriteLine($" Territorio {territorioOrigen} no tiene adyacencias");
            return false;
        }

        // Verificar que el territorio destino existe
        if (!propietariosTerritorios.ContieneClave(territorioDestino))
        {
            Console.WriteLine($" Territorio destino {territorioDestino} no existe");
            return false;
        }
        var adyacentes = adyacencias.Obtener(territorioOrigen);

        // Verificar que son territorios adyacentes conectados
        if (!adyacentes.Contiene(territorioDestino))
        {
            Console.WriteLine($"Territorio {territorioDestino} no es adyacente a {territorioOrigen}");
            return false;
        }
        string propietarioDestino = propietariosTerritorios.Obtener(territorioDestino);

        // Solo se puede atacar territorios enemigos
        bool esAtacable = propietarioDestino != jugadorAtacante;
        Console.WriteLine($" Verificación atacable: {territorioOrigen} -> {territorioDestino} = {esAtacable} (Propietario: {propietarioDestino})");
        return esAtacable;
    }

    // RealizarCombate para nuevo sistema de dados
    private void RealizarCombate(int territorioAtacante, int territorioDefensor, int tropasAtacante, string jugadorAtacante)
    {
        string jugadorDefensor = propietariosTerritorios.Obtener(territorioDefensor);
        int tropasEnAtacante = tropasPorTerritorio.Obtener(territorioAtacante);
        int tropasEnDefensor = tropasPorTerritorio.Obtener(territorioDefensor);

        Console.WriteLine($"COMBATE: {jugadorAtacante} ataca {jugadorDefensor}");
        Console.WriteLine($"Atacante: Territorio {territorioAtacante} con {tropasEnAtacante} tropas");
        Console.WriteLine($"Defensor: Territorio {territorioDefensor} con {tropasEnDefensor} tropas");
        Console.WriteLine($"Tropas usadas en ataque: {tropasAtacante}");

        // SISTEMA DE DADOS - SIN ORDENAR
        // ATACANTE: Puede usar 1, 2 o 3 dados
        int dadosAtacanteUsar = Math.Min(3, tropasAtacante);

        // DEFENSOR: Puede usar 1, 2 o 3 dados (Dependiendo de las tropas de su territorio)
        int dadosDefensorUsar = Math.Min(3, tropasEnDefensor);

        // LANZAR DADOS ALEATORIOS
        int[] dadosAtacante = LanzarDados(dadosAtacanteUsar);
        int[] dadosDefensor = LanzarDados(dadosDefensorUsar);

        Console.WriteLine($"Dados Atacante ({dadosAtacanteUsar}): [{string.Join(",", dadosAtacante)}]");
        Console.WriteLine($"Dados Defensor ({dadosDefensorUsar}): [{string.Join(",", dadosDefensor)}]");

        // COMPARAR DADOS EN EL ORDEN ORIGINAL
        int perdidasAtacante = 0;
        int perdidasDefensor = 0;

        int comparaciones = Math.Min(dadosAtacante.Length, dadosDefensor.Length);
        for (int i = 0; i < comparaciones; i++)
        {
            if (dadosAtacante[i] > dadosDefensor[i])
            {
                // ATACANTE GANA: defensor pierde 1 tropa
                perdidasDefensor++;
                Console.WriteLine($" Dado {i + 1}: {dadosAtacante[i]} > {dadosDefensor[i]} → DEFENSOR pierde 1 tropa");
            }
            else
            {
                // DEFENSOR GANA O EMPATA: atacante pierde 1 tropa
                perdidasAtacante++;
                Console.WriteLine($" Dado {i + 1}: {dadosAtacante[i]} <= {dadosDefensor[i]} → ATACANTE pierde 1 tropa");
            }
        }

        Console.WriteLine($"RESULTADO: Atacante pierde {perdidasAtacante}, Defensor pierde {perdidasDefensor}");

        // ✅ APLICAR PÉRDIDAS
        int nuevasTropasAtacante = tropasEnAtacante - perdidasAtacante;
        int nuevasTropasDefensor = tropasEnDefensor - perdidasDefensor;

        // Asegurar que no queden tropas negativas
        if (nuevasTropasAtacante < 0) nuevasTropasAtacante = 0;
        if (nuevasTropasDefensor < 0) nuevasTropasDefensor = 0;

        tropasPorTerritorio.Actualizar(territorioAtacante, nuevasTropasAtacante);
        tropasPorTerritorio.Actualizar(territorioDefensor, nuevasTropasDefensor);

        // Enviar actualizaciones a los clientes
        SendToAll($"TROPAS_ACTUALIZADAS:{territorioAtacante},{jugadorAtacante},{nuevasTropasAtacante}");
        SendToAll($"TROPAS_ACTUALIZADAS:{territorioDefensor},{jugadorDefensor},{nuevasTropasDefensor}");

        // VERIFICAR SI EL DEFENSOR PERDIÓ EL TERRITORIO
        if (nuevasTropasDefensor <= 0)
        {
            ConquistarTerritorio(territorioAtacante, territorioDefensor, jugadorAtacante, tropasAtacante);
        }

        // ENVIAR RESULTADO DEL COMBATE CON DADOS ORIGINALES
        string resultadoCombate = $"RESULTADO_COMBATE:{territorioAtacante},{territorioDefensor}," +
                                 $"{perdidasAtacante},{perdidasDefensor}," +
                                 $"{string.Join(",", dadosAtacante)},{string.Join(",", dadosDefensor)}";
        SendToAll(resultadoCombate);
    }

    // LanzarDados para el ramdon
    private int[] LanzarDados(int cantidad)
    {
        int[] dados = new int[cantidad];
        for (int i = 0; i < cantidad; i++)
        {
            dados[i] = random.Next(1, 7); // Dados de 1 a 6
        }
        return dados;
    }

    // ConquistarTerritorio para movimiento de tropas
    private void ConquistarTerritorio(int territorioAtacante, int territorioConquistado, string nuevoPropietario, int tropasUsadasEnAtaque)
    {
        Console.WriteLine($" CONQUISTA: {nuevoPropietario} conquistó territorio {territorioConquistado}!");

        // 1. CAMBIAR PROPIETARIO
        string antiguoPropietario = propietariosTerritorios.Obtener(territorioConquistado);
        propietariosTerritorios.Actualizar(territorioConquistado, nuevoPropietario);

        // 2. ACTUALIZAR CONTADORES DE TERRITORIOS
        territoriosPorJugador.Actualizar(antiguoPropietario, territoriosPorJugador.Obtener(antiguoPropietario) - 1);
        territoriosPorJugador.Actualizar(nuevoPropietario, territoriosPorJugador.Obtener(nuevoPropietario) + 1);

        // 3. MOVER TROPAS - LAS USADAS EN EL ATAQUE
        int tropasEnAtacante = tropasPorTerritorio.Obtener(territorioAtacante);
        int tropasParaMover = Math.Max(tropasUsadasEnAtaque, 1); 

        // Verificar que no se muevan más tropas de las disponibles
        int maximoPermitido = tropasEnAtacante - 1;
        tropasParaMover = Math.Min(tropasParaMover, maximoPermitido);

        // 4. ACTUALIZAR TROPAS EN AMBOS TERRITORIOS
        tropasPorTerritorio.Actualizar(territorioAtacante, tropasEnAtacante - tropasParaMover);
        tropasPorTerritorio.Actualizar(territorioConquistado, tropasParaMover);

        // 5. ACTUALIZAR LISTA DE TERRITORIOS NEUTROS SI ES NECESARIO
        if (antiguoPropietario == "NEUTRO")
        {
            territoriosNeutros.Remover(territorioConquistado);
        }

        // 6. ENVIAR ACTUALIZACIONES A LOS CLIENTES
        SendToAll($"TERRITORIO_CONQUISTADO:{territorioConquistado},{nuevoPropietario},{tropasParaMover}");
        SendToAll($"TROPAS_ACTUALIZADAS:{territorioAtacante},{nuevoPropietario},{tropasPorTerritorio.Obtener(territorioAtacante)}");
        SendToAll($"TROPAS_ACTUALIZADAS:{territorioConquistado},{nuevoPropietario},{tropasParaMover}");

        Console.WriteLine($"  Movidas {tropasParaMover} tropas a territorio {territorioConquistado}");
        Console.WriteLine($"  Territorio {territorioAtacante} ahora tiene {tropasPorTerritorio.Obtener(territorioAtacante)} tropas");
        Console.WriteLine($"  Territorio {territorioConquistado} ahora tiene {tropasParaMover} tropas");
        OtorgarTarjetaPorConquista(nuevoPropietario);

        /*
        // VERIFICAR VICTORIA DESPUÉS DE LA CONQUISTA
        VerificarVictoria(nuevoPropietario);
        */
    }

    private void ProcesarAlias(string playerId, string mensaje)
    {
        string alias = mensaje.Substring(6);
        if (playerId == "JUGADOR1")
        {
            aliasJugador1 = alias;
            SendToAll($"JUGADOR1_ALIAS:{aliasJugador1}");
        }
        else
        {
            aliasJugador2 = alias;
            SendToAll($"JUGADOR2_ALIAS:{aliasJugador2}");
        }
        Console.WriteLine($" {playerId} se llama: {alias}");
    }

    // ProcesarColocacionTropas para mantener 1 acción por turno
    private void ProcesarColocacionTropas(string playerId, string mensaje)
    {
        Console.WriteLine($" COLOCACIÓN: Jugador {playerId}, Mensaje: {mensaje}");

        if (playerId != jugadorConTurno)
        {
            Console.WriteLine($"{playerId} intentó colocar tropas fuera de turno");
            SendToClient(GetClientByPlayerId(playerId), "ERROR:No es tu turno");
            return;
        }
        try
        {
            string[] partes = mensaje.Split(':');
            string[] datos = partes[1].Split(',');

            int territorioId = int.Parse(datos[0]);
            int cantidad = int.Parse(datos[1]);

            // SOLO PERMITIR COLOCAR 1 TROPA POR TURNO
            if (cantidad != 1)
            {
                Console.WriteLine($" {playerId} intentó colocar {cantidad} tropas (solo 1 permitida)");
                SendToClient(GetClientByPlayerId(playerId), "ERROR:Solo puedes colocar 1 tropa por turno");
                return;
            }

            int tropasActuales = tropasPorJugador.Obtener(playerId);
            if (tropasActuales < 1)
            {
                Console.WriteLine($" {playerId} no tiene tropas disponibles");
                SendToClient(GetClientByPlayerId(playerId), "ERROR:No tienes tropas disponibles");
                return;
            }

            string propietarioActual = propietariosTerritorios.Obtener(territorioId);
            Console.WriteLine($"DEBUG: Territorio {territorioId} pertenece a '{propietarioActual}'");

            if (propietarioActual == playerId)
            {
                // VERIFICAR LÍMITE DE 4 TROPAS POR TERRITORIO
                int tropasActualesEnTerritorio = tropasPorTerritorio.Obtener(territorioId);
                if (tropasActualesEnTerritorio >= 4)
                {
                    Console.WriteLine($" {playerId} intentó colocar en territorio lleno: {territorioId}");
                    SendToClient(GetClientByPlayerId(playerId), "ERROR:Este territorio ya tiene 4 tropas");
                    return;
                }

                // COLOCAR 1 TROPA
                tropasPorJugador.Actualizar(playerId, tropasActuales - 1);
                tropasPorTerritorio.Actualizar(territorioId, tropasActualesEnTerritorio + 1);

                string mensajeActualizacion = $"TROPAS_ACTUALIZADAS:{territorioId},{playerId},{tropasPorTerritorio.Obtener(territorioId)}";
                SendToAll(mensajeActualizacion);

                // ENVIAR NUEVAS TROPAS DISPONIBLES
                int nuevasTropasDisponibles = tropasPorJugador.Obtener(playerId);
                SendToClient(GetClientByPlayerId(playerId), $"TROPAS_DISPONIBLES:{nuevasTropasDisponibles}");

                Console.WriteLine($" {playerId} colocó 1 tropa en territorio {territorioId} | Total: {tropasPorTerritorio.Obtener(territorioId)} | Restantes: {nuevasTropasDisponibles}");

                // 1 ACCIÓN POR TURNO - Cambiar turno después de colocar
                CambiarTurno(playerId);
            }
            else
            {
                Console.WriteLine($" {playerId} intentó colocar en territorio de {propietarioActual}: {territorioId}");
                SendToClient(GetClientByPlayerId(playerId), "ERROR:Territorio no te pertenece");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error procesando colocación: {ex.Message}");
        }
    }

    private void ProcesarFinTurno(string playerId)
    {
        if (playerId != jugadorConTurno)
        {
            Console.WriteLine($"{playerId} intentó terminar turno fuera de turno");
            SendToClient(GetClientByPlayerId(playerId), "ERROR:No es tu turno");
            return;
        }

        // EL NEUTRO NUNCA DEBERÍA PODER TERMINAR TURNO (NO ATACA)
        if (playerId == "NEUTRO")
        {
            Console.WriteLine($"NEUTRO intentó terminar turno - no debería pasar");
            return;
        }

        // Solo JUGADOR1 y JUGADOR2 pueden cambiar turnos
        string nuevoTurno = (playerId == "JUGADOR1") ? "JUGADOR2" : "JUGADOR1";
        jugadorConTurno = nuevoTurno;

        SendToClient(player1, $"TURNO:{(nuevoTurno == "JUGADOR1" ? "ACTIVO" : "ESPERA")}");
        SendToClient(player2, $"TURNO:{(nuevoTurno == "JUGADOR2" ? "ACTIVO" : "ESPERA")}");

        // ENVIAR 0 TROPAS SOLO A JUGADORES (INDICA MODO ATAQUE)
        SendToClient(GetClientByPlayerId(nuevoTurno), $"TROPAS_DISPONIBLES:0");

        Console.WriteLine($"Turno cambiado a {nuevoTurno} - Modo Ataque");
    }

    private TcpClient GetClientByPlayerId(string playerId)
    {
        return playerId == "JUGADOR1" ? player1 : player2;
    }

    private void SendToClient(TcpClient client, string message)
    {
        try
        {
            if (client == null || !client.Connected) return;

            NetworkStream stream = client.GetStream();
            byte[] data = Encoding.UTF8.GetBytes(message + "\n");
            stream.Write(data, 0, data.Length);

            Console.WriteLine($"Enviado a {GetPlayerIdByClient(client)}: {message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error enviando: {ex.Message}");
        }
    }

    private string GetPlayerIdByClient(TcpClient client)
    {
        if (client == player1) return "JUGADOR1";
        if (client == player2) return "JUGADOR2";
        return "DESCONOCIDO";
    }

    private void SendToAll(string message)
    {
        SendToClient(player1, message);
        SendToClient(player2, message);
    }

    private void CleanupPlayerConnection(string playerId, TcpClient client)
    {
        try
        {
            client?.Close();
            if (playerId == "JUGADOR1") player1 = null;
            else if (playerId == "JUGADOR2") player2 = null;
            Console.WriteLine($" {playerId} desconectado");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error limpiando {playerId}: {ex.Message}");
        }
    }

    public void Stop()
    {
        isRunning = false;
        tcpListener?.Stop();
        player1?.Close();
        player2?.Close();
        Console.WriteLine("Servidor detenido");
    }

    public static void Main(string[] args)
    {
        GameServer server = new GameServer(7777);
        server.Start();
        Console.WriteLine("Presiona 'Q' para detener el servidor...");
        while (Console.ReadKey().Key != ConsoleKey.Q) { }
        server.Stop();
    }
}