using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RiskGame.Estructuras;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI textoEstado;
    public TextMeshProUGUI textoTropasDisponibles;
    public TextMeshProUGUI textoJugadorActual;
    public TextMeshProUGUI textoOponente;
    public GameObject panelTurno;

    [Header("Modo de Acción")]
    public Button botonModoColocar;
    public Button botonModoAtacar;
    public TextMeshProUGUI textoModoActual;

    [Header("Botón Ajustes")]
    public Button botonAjustes;

    [Header("UI Jugadores")]
    public GameObject panelInfoJugadores;
    public TextMeshProUGUI textoNombreJugador1;
    public TextMeshProUGUI textoNombreJugador2;
    public TextMeshProUGUI textoTropasJugador1;
    public TextMeshProUGUI textoTropasJugador2;
    public TextMeshProUGUI textoTurnoJugador1;
    public TextMeshProUGUI textoTurnoJugador2;
    public Image fondoJugador1;
    public Image fondoJugador2;
    public Image bordeTurnoJugador1;
    public Image bordeTurnoJugador2;

    [Header("Sistema de Continentes")]
    public GameObject panelContinentes;
    public Button botonTodos;
    public Button botonAmericaNorte;
    public Button botonAmericaSur;
    public Button botonEuropa;
    public Button botonAfrica;
    public Button botonAsia;
    public Button botonOceania;
    public TextMeshProUGUI textoContinenteActual;

    [Header("Territorios")]
    public Button[] botonesTerritorios = new Button[42];
    public GameObject contenedorTerritorios;

    [Header("UI Ataque")]
    public GameObject panelAtaque;
    public TextMeshProUGUI textoTerritorioAtacante;
    public TextMeshProUGUI textoTropasAtaque;
    public Button botonAtacar1;
    public Button botonAtacar2;
    public Button botonAtacar3;
    public Button botonCancelarAtaque;
    public GameObject panelTerritoriosAtacables;
    public Transform contenidoTerritoriosAtacables;
    public GameObject prefabBotonTerritorioAtacable;

    [Header("UI Combate")]
    public GameObject panelCombate;
    public TextMeshProUGUI textoResultadoCombate;
    public TextMeshProUGUI textoDadosAtacante;
    public TextMeshProUGUI textoDadosDefensor;
    public TextMeshProUGUI textoPerdidasAtacante;
    public TextMeshProUGUI textoPerdidasDefensor;
    public Button botonCerrarCombate;

    [Header("Sistema de Tarjetas")]
    public TextMeshProUGUI textoInfanteria;
    public TextMeshProUGUI textoCaballeria;
    public TextMeshProUGUI textoArtilleria;
    public Button botonIntercambiar;

    private int infanteria = 0;
    private int caballeria = 0;
    private int artilleria = 0;
    private bool tieneTrioDisponible = false;

    [Header("Colores")]
    public Color colorJugador1 = new Color(0.2f, 0.4f, 1.0f, 1.0f);
    public Color colorJugador2 = new Color(1.0f, 0.3f, 0.3f, 1.0f);
    public Color colorTurnoActivo = new Color(1f, 0.9f, 0.2f, 1f);
    public Color colorTurnoInactivo = new Color(0.3f, 0.3f, 0.3f, 0.3f);

    [Header("Configuración")]
    public int maxTropasPorTerritorio = 4;

    // Sistema de continentes
    private Diccionario<string, ListaEnlazada<int>> continentes = new Diccionario<string, ListaEnlazada<int>>();
    private string continenteActual = "TODOS";

    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;
    private bool isConnected = false;

    // Estructuras personalizadas
    private Diccionario<int, Territory> territorios = new Diccionario<int, Territory>();
    private Cola<string> messageQueue = new Cola<string>();
    private readonly object queueLock = new object();

    // Datos del juego
    private string miJugadorId = "SIN_ID";
    private string oponenteId = "SIN_ID";
    private int tropasDisponibles = 0;
    private bool esMiTurno = false;

    // Sistema de ataques
    private int territorioAtacanteSeleccionado = -1;
    private int cantidadTropasAtaque = 0;
    private ListaEnlazada<int> territoriosAtacables = new ListaEnlazada<int>();

    // Alias de jugadores
    private string aliasJugador1 = "Jugador1";
    private string aliasJugador2 = "Jugador2";

    // Instancia singleton
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        Debug.Log(" Iniciando Risk GameManager...");
        InicializarContinentes();
        InicializarJuego();
        ConectarAlServidor();
        GestionarAudio();
    }

    // OBTENER EL NOMBRE INTERNO DEL CONTINENTE
    public string ObtenerNombreContinenteInterno(int territorioId)
    {
        return NombresContinentes.ObtenerNombreContinente(territorioId);
    }

    // SISTEMA DE TARJETAS
private void ConfigurarSistemaTarjetas()
{
    if (botonIntercambiar != null)
    {
        botonIntercambiar.onClick.RemoveAllListeners();
        botonIntercambiar.onClick.AddListener(IntercambiarTarjetas);
    }
    ActualizarUITarjetas();
}

// ACTUALIZAR UI DE TARJETAS
private void ActualizarUITarjetas()
{
    if (textoInfanteria != null)
        textoInfanteria.text = $"Infantería: {infanteria}";
    
    if (textoCaballeria != null)
        textoCaballeria.text = $"Caballería: {caballeria}";
    
    if (textoArtilleria != null)
        textoArtilleria.text = $"Artillería: {artilleria}";
    
    // ACTUALIZAR BOTÓN DE INTERCAMBIO
    if (botonIntercambiar != null)
    {
        bool puedeIntercambiar = (infanteria >= 1 && caballeria >= 1 && artilleria >= 1) ||
                                (infanteria >= 3 || caballeria >= 3 || artilleria >= 3);
        
        botonIntercambiar.interactable = puedeIntercambiar;
    }
}

// PROCESAR TARJETA OBTENIDA
private void ProcesarTarjetaObtenida(string datos)
{
    try
    {
        int tipoIndex = int.Parse(datos);
        
        switch(tipoIndex)
        {
            case 0: 
                infanteria++;
                textoEstado.text = "Obtuviste tarjeta: Infantería";
                break;
            case 1: 
                caballeria++;
                textoEstado.text = "Obtuviste tarjeta: Caballería";
                break;
            case 2: 
                artilleria++;
                textoEstado.text = "Obtuviste tarjeta: Artillería";
                break;
        }
        
        ActualizarUITarjetas();
        Debug.Log($"Tarjeta obtenida - Infantería: {infanteria}, Caballería: {caballeria}, Artillería: {artilleria}");
    }
    catch (Exception ex)
    {
        Debug.LogError($"Error procesando tarjeta: {ex.Message}");
    }
}

// ACTUALIZAR TARJETAS DESDE SERVIDOR
private void ActualizarTarjetasDesdeServidor(string datos)
{
    try
    {
        string[] valores = datos.Split(',');
        infanteria = int.Parse(valores[0]);
        caballeria = int.Parse(valores[1]);
        artilleria = int.Parse(valores[2]);
        
        ActualizarUITarjetas();
        Debug.Log($"Tarjetas actualizadas - I:{infanteria}, C:{caballeria}, A:{artilleria}");
    }
    catch (Exception ex)
    {
        Debug.LogError($"Error actualizando tarjetas: {ex.Message}");
    }
}

// INTERCAMBIAR TARJETAS
private void IntercambiarTarjetas()
{
    // VERIFICAR LOCALMENTE QUE TIENE TRÍO VÁLIDO
    bool tieneTrioDiferente = infanteria >= 1 && caballeria >= 1 && artilleria >= 1;
    bool tieneTrioIgual = infanteria >= 3 || caballeria >= 3 || artilleria >= 3;
    
    if (!tieneTrioDiferente && !tieneTrioIgual)
    {
        textoEstado.text = "No tienes un trío válido para intercambiar";
        return;
    }
    EnviarAlServidor("INTERCAMBIAR_TARJETAS");
    textoEstado.text = "Intercambiando tarjetas...";
}

// PROCESAR INTERCAMBIO COMPLETADO
private void ProcesarIntercambioCompletado(string datos)
{
    if (int.TryParse(datos, out int refuerzos))
    {
        textoEstado.text = $"¡Intercambio exitoso! +{refuerzos} tropas";
    }
}

    // INICIALIZAR SISTEMA DE CONTINENTES
    private void InicializarContinentes()
    {
        Debug.Log(" Inicializando sistema de continentes personalizado...");

        // Crear diccionario de continentes
        continentes.Agregar("TODOS", new ListaEnlazada<int>());
        continentes.Agregar("AMERICA_NORTE", new ListaEnlazada<int>());
        continentes.Agregar("AMERICA_SUR", new ListaEnlazada<int>());
        continentes.Agregar("EUROPA", new ListaEnlazada<int>());
        continentes.Agregar("AFRICA", new ListaEnlazada<int>());
        continentes.Agregar("ASIA", new ListaEnlazada<int>());
        continentes.Agregar("OCEANIA", new ListaEnlazada<int>());

        // AMÉRICA DEL NORTE: Botones 0,1,2,3,4,5,6,7,8
        int[] americaNorte = { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        foreach (int territorioId in americaNorte)
        {
            continentes.Obtener("AMERICA_NORTE").Agregar(territorioId);
        }

        // OCEANÍA: Botón 9
        int[] oceania = { 9 };
        foreach (int territorioId in oceania)
        {
            continentes.Obtener("OCEANIA").Agregar(territorioId);
        }

        // AMÉRICA DEL SUR: Botones 10,11,12,13,14
        int[] americaSur = { 10, 11, 12, 13, 14 };
        foreach (int territorioId in americaSur)
        {
            continentes.Obtener("AMERICA_SUR").Agregar(territorioId);
        }

        // ÁFRICA: Botones 15,16,17,18,19,20
        int[] africa = { 15, 16, 17, 18, 19, 20 };
        foreach (int territorioId in africa)
        {
            continentes.Obtener("AFRICA").Agregar(territorioId);
        }

        // EUROPA: Botones 24,25,26,27,28,29,30
        int[] europa = { 24, 25, 26, 27, 28, 29, 30 };
        foreach (int territorioId in europa)
        {
            continentes.Obtener("EUROPA").Agregar(territorioId);
        }

        // ASIA: Botones 21,22,23,31,32,33,34,35,36,37,38,39,40,41
        int[] asia = { 21, 22, 23, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41 };
        foreach (int territorioId in asia)
        {
            continentes.Obtener("ASIA").Agregar(territorioId);
        }

        // Todos los territorios (0-41)
        for (int i = 0; i < 42; i++)
        {
            continentes.Obtener("TODOS").Agregar(i);
        }

        Debug.Log(" Continentes personalizados inicializados:");
        Debug.Log($"   América Norte: {continentes.Obtener("AMERICA_NORTE").Count} territorios (0-8)");
        Debug.Log($"   Oceanía: {continentes.Obtener("OCEANIA").Count} territorios (9)");
        Debug.Log($"   América Sur: {continentes.Obtener("AMERICA_SUR").Count} territorios (10-14)");
        Debug.Log($"   África: {continentes.Obtener("AFRICA").Count} territorios (15-20)");
        Debug.Log($"   Europa: {continentes.Obtener("EUROPA").Count} territorios (24-30)");
        Debug.Log($"   Asia: {continentes.Obtener("ASIA").Count} territorios (21-23,31-41)");
    }

    // CONFIGURAR BOTONES DE CONTINENTES
    private void ConfigurarBotonesContinentes()
    {
        if (botonTodos != null)
        {
            botonTodos.onClick.RemoveAllListeners();
            botonTodos.onClick.AddListener(() => MostrarContinente("TODOS"));
        }

        if (botonAmericaNorte != null)
        {
            botonAmericaNorte.onClick.RemoveAllListeners();
            botonAmericaNorte.onClick.AddListener(() => MostrarContinente("AMERICA_NORTE"));
        }

        if (botonAmericaSur != null)
        {
            botonAmericaSur.onClick.RemoveAllListeners();
            botonAmericaSur.onClick.AddListener(() => MostrarContinente("AMERICA_SUR"));
        }

        if (botonEuropa != null)
        {
            botonEuropa.onClick.RemoveAllListeners();
            botonEuropa.onClick.AddListener(() => MostrarContinente("EUROPA"));
        }

        if (botonAfrica != null)
        {
            botonAfrica.onClick.RemoveAllListeners();
            botonAfrica.onClick.AddListener(() => MostrarContinente("AFRICA"));
        }

        if (botonAsia != null)
        {
            botonAsia.onClick.RemoveAllListeners();
            botonAsia.onClick.AddListener(() => MostrarContinente("ASIA"));
        }

        if (botonOceania != null)
        {
            botonOceania.onClick.RemoveAllListeners();
            botonOceania.onClick.AddListener(() => MostrarContinente("OCEANIA"));
        }

        // Mostrar todos los territorios por defecto
        MostrarContinente("TODOS");
    }

    // CONFIGURAR BOTONES DE ATAQUE
    private void ConfigurarBotonesAtaque()
    {
        if (botonAtacar1 != null)
        {
            botonAtacar1.onClick.RemoveAllListeners();
            botonAtacar1.onClick.AddListener(() => SeleccionarTropasAtaque(1));
        }

        if (botonAtacar2 != null)
        {
            botonAtacar2.onClick.RemoveAllListeners();
            botonAtacar2.onClick.AddListener(() => SeleccionarTropasAtaque(2));
        }

        if (botonAtacar3 != null)
        {
            botonAtacar3.onClick.RemoveAllListeners();
            botonAtacar3.onClick.AddListener(() => SeleccionarTropasAtaque(3));
        }
    }

    // SeleccionarTropasAtaque para iniciar el timeout
    private void SeleccionarTropasAtaque(int cantidad)
    {
        if (territorioAtacanteSeleccionado == -1)
        {
            textoEstado.text = "Primero selecciona un territorio PROPIO para atacar";
            return;
        }

        var territorio = territorios.Obtener(territorioAtacanteSeleccionado);
        int tropasEnTerritorio = territorio.GetTropas();
        int maximoPermitido = Math.Min(3, tropasEnTerritorio - 1);

        if (cantidad < 1 || cantidad > maximoPermitido)
        {
            textoEstado.text = $"Puedes atacar con 1 a {maximoPermitido} tropas";
            return;
        }

        // SELECCIONAR CANTIDAD DE TROPAS
        cantidadTropasAtaque = cantidad;

        // ENVIAR AL SERVIDOR LA SELECCIÓN DE ATAQUE
        Debug.Log($"Enviando SELECCIONAR_ATAQUE: {territorioAtacanteSeleccionado},{cantidad}");
        EnviarAlServidor($"SELECCIONAR_ATAQUE:{territorioAtacanteSeleccionado},{cantidad}");

        textoEstado.text = $"{cantidad} tropas seleccionadas. Esperando territorios atacables...";

        // INICIAR TIMEOUT PARA SELECCIÓN
        StartCoroutine(TimeoutSeleccionAtaque());
    }

    // Método de debug para verificar estado
    private void DebugEstadoAtaque()
    {
        Debug.Log($"DEBUG ATAQUE:");
        Debug.Log($"Estado: {estadoActual}");
        Debug.Log($"Territorio Atacante: {territorioAtacanteSeleccionado}");
        Debug.Log($"Cantidad Tropas: {cantidadTropasAtaque}");
        Debug.Log($"Territorios Atacables: {territoriosAtacables.Count}");

        if (territorioAtacanteSeleccionado != -1 && territorios.ContieneClave(territorioAtacanteSeleccionado))
        {
            var territorio = territorios.Obtener(territorioAtacanteSeleccionado);
            Debug.Log($"Tropas en territorio: {territorio.GetTropas()}");
            Debug.Log($"Propietario: {territorio.GetPropietario()}");
        }
    }

    // MostrarPanelAtaque para solo habilitar/deshabilitar
    private void MostrarPanelAtaque(int territorioId)
    {
        var territorio = territorios.Obtener(territorioId);
        int tropasEnTerritorio = territorio.GetTropas();

        // CALCULAR MÁXIMO PERMITIDO
        int maxTropasAtaque = Math.Min(3, tropasEnTerritorio - 1);

        // HABILITAR BOTONES SEGÚN TROPAS DISPONIBLES
        if (botonAtacar1 != null)
        {
            bool puedeAtacarCon1 = maxTropasAtaque >= 1;
            botonAtacar1.interactable = puedeAtacarCon1;
        }
        if (botonAtacar2 != null)
        {
            bool puedeAtacarCon2 = maxTropasAtaque >= 2;
            botonAtacar2.interactable = puedeAtacarCon2;
        }
        if (botonAtacar3 != null)
        {
            bool puedeAtacarCon3 = maxTropasAtaque >= 3;
            botonAtacar3.interactable = puedeAtacarCon3;
        }

        textoEstado.text = $"Territorio {territorioId} seleccionado ({tropasEnTerritorio} tropas). Elige tropas para atacar:";

        Debug.Log($"Botones de ataque habilitados - Territorio {territorioId} con {tropasEnTerritorio} tropas - Máximo: {maxTropasAtaque}");
    }

    // OcultarPanelAtaque para resetear botones
    private void OcultarPanelAtaque()
    {
        if (panelAtaque != null)
        {
            panelAtaque.SetActive(false);
        }
        if (panelTerritoriosAtacables != null) panelTerritoriosAtacables.SetActive(false);
        territorioAtacanteSeleccionado = -1;
        cantidadTropasAtaque = 0;
    }

    // MOSTRAR TERRITORIOS ATACABLES
    private void MostrarTerritoriosAtacables(ListaEnlazada<int> territoriosAtacables)
    {
        this.territoriosAtacables = territoriosAtacables;

        if (panelTerritoriosAtacables != null && contenidoTerritoriosAtacables != null)
        {
            // Limpiar botones anteriores
            foreach (Transform child in contenidoTerritoriosAtacables)
            {
                Destroy(child.gameObject);
            }

            panelTerritoriosAtacables.SetActive(true);

            // Crear botones para cada territorio atacable
            for (int i = 0; i < territoriosAtacables.Count; i++)
            {
                int territorioId = territoriosAtacables.Obtener(i);
                CrearBotonTerritorioAtacable(territorioId);
            }

            Debug.Log($" Mostrados {territoriosAtacables.Count} territorios atacables en el panel");
        }
    }

    // BOTÓN TERRITORIO ATACABLE
    private void CrearBotonTerritorioAtacable(int territorioId)
    {
        if (prefabBotonTerritorioAtacable == null) return;

        GameObject botonObj = Instantiate(prefabBotonTerritorioAtacable, contenidoTerritoriosAtacables);
        Button boton = botonObj.GetComponent<Button>();
        TextMeshProUGUI texto = botonObj.GetComponentInChildren<TextMeshProUGUI>();

        if (texto != null)
        {
            texto.text = $"Territorio {territorioId}";
        }

        boton.onClick.AddListener(() => AtacarTerritorio(territorioId));
    }

    // ATACAR TERRITORIO
    private void AtacarTerritorio(int territorioDefensorId)
    {
        if (territorioAtacanteSeleccionado == -1 || cantidadTropasAtaque == 0)
        {
            textoEstado.text = " Primero selecciona un territorio y tropas para atacar";
            return;
        }

        // Verificar que el territorio defensor es enemigo
        var territorioDefensor = territorios.Obtener(territorioDefensorId);
        if (territorioDefensor.GetPropietario() == miJugadorId || territorioDefensor.GetPropietario() == "NEUTRO")
        {
            textoEstado.text = " Solo puedes atacar territorios enemigos";
            return;
        }

        EnviarAlServidor($"ATACAR:{territorioDefensorId}");
        OcultarPanelAtaque();
        textoEstado.text = $" Atacando territorio {territorioDefensorId}...";

        Debug.Log($" Enviando ataque: {territorioAtacanteSeleccionado} -> {territorioDefensorId} con {cantidadTropasAtaque} tropas");
    }
    // MostrarResultadoCombate para mostrar dados en orden original
    private void MostrarResultadoCombate(int territorioAtacante, int territorioDefensor,
                                       int perdidasAtacante, int perdidasDefensor,
                                       string dadosAtacanteStr, string dadosDefensorStr)
    {
        if (panelCombate != null)
        {
            panelCombate.SetActive(true);

            // PROCESAR DADOS EN ORDEN ORIGINAL
            string[] dadosAtacanteArray = dadosAtacanteStr.Split(',');
            string[] dadosDefensorArray = dadosDefensorStr.Split(',');

            StringBuilder comparacion = new StringBuilder();

            // COMPARAR DADO POR DADO EN ORDEN ORIGINAL
            int comparaciones = Math.Min(dadosAtacanteArray.Length, dadosDefensorArray.Length);
            for (int i = 0; i < comparaciones; i++)
            {
                int dadoAtacante = int.Parse(dadosAtacanteArray[i]);
                int dadoDefensor = int.Parse(dadosDefensorArray[i]);

                string resultado;
                if (dadoAtacante > dadoDefensor)
                {
                    resultado = $"<color=red>DEFENSOR pierde 1 tropa</color>";
                }
                else
                {
                    resultado = $"<color=blue>ATACANTE pierde 1 tropa</color>";
                }

                comparacion.AppendLine($"Dado {i + 1}: {dadoAtacante} vs {dadoDefensor} → {resultado}");
            }

            textoResultadoCombate.text = $"Combate: Territorio {territorioAtacante} vs Territorio {territorioDefensor}";
            textoDadosAtacante.text = $"Dados Atacante ({dadosAtacanteArray.Length}): {dadosAtacanteStr}";
            textoDadosDefensor.text = $"Dados Defensor ({dadosDefensorArray.Length}): {dadosDefensorStr}";
            textoPerdidasAtacante.text = $"Tropas perdidas Atacante: {perdidasAtacante}";
            textoPerdidasDefensor.text = $"Tropas perdidas Defensor: {perdidasDefensor}";

            // DETALLE DE COMPARACIÓN
            if (textoResultadoCombate != null)
            {
                textoResultadoCombate.text += $"\n\n{comparacion.ToString()}";
            }
            Debug.Log($"Combate mostrado - Dados originales: Atacante[{dadosAtacanteStr}] Defensor[{dadosDefensorStr}]");
        }
    }

    // CERRAR PANEL COMBATE
    private void CerrarPanelCombate()
    {
        if (panelCombate != null)
        {
            panelCombate.SetActive(false);
            Debug.Log(" Panel de combate cerrado");
        }
    }
    //Música
    private void ConfigurarBotonAjustes()
    {
        if (botonAjustes != null)
        {
            botonAjustes.onClick.RemoveAllListeners();
            botonAjustes.onClick.AddListener(IrAAjustes);
        }
    }
    private void IrAAjustes()
    {
        SceneManager.LoadScene("Ajustes");
    }
    private void GestionarAudio()
    {
        if (AudioManager.Instance == null)
        {
            GameObject audioManagerObj = new GameObject("AudioManager");
            audioManagerObj.AddComponent<AudioManager>();
        }
    }

    // MOSTRAR TERRITORIOS DE UN CONTINENTE
    private void MostrarContinente(string continente)
    {
        continenteActual = continente;
        OcultarTodosLosTerritorios();

        if (continentes.ContieneClave(continente))
        {
            var territoriosContinente = continentes.Obtener(continente);

            for (int i = 0; i < territoriosContinente.Count; i++)
            {
                int territorioId = territoriosContinente.Obtener(i);
                if (territorioId < botonesTerritorios.Length && botonesTerritorios[territorioId] != null)
                {
                    botonesTerritorios[territorioId].gameObject.SetActive(true);
                }
            }
        }

        if (textoContinenteActual != null)
        {
            string nombreContinente = ObtenerNombreContinente(continente);
            textoContinenteActual.text = $"Continente: {nombreContinente}";
        }

        Debug.Log($" Mostrando continente: {continente} ({continentes.Obtener(continente).Count} territorios)");
    }

    // OCULTAR TODOS LOS TERRITORIOS
    private void OcultarTodosLosTerritorios()
    {
        for (int i = 0; i < botonesTerritorios.Length; i++)
        {
            if (botonesTerritorios[i] != null)
            {
                botonesTerritorios[i].gameObject.SetActive(false);
            }
        }
    }

    // OBTENER NOMBRE DEL CONTINENTE
    private string ObtenerNombreContinente(string continenteId)
    {
        switch (continenteId)
        {
            case "TODOS": return " Todos los Territorios";
            case "AMERICA_NORTE": return " América del Norte";
            case "AMERICA_SUR": return " América del Sur";
            case "EUROPA": return " Europa";
            case "AFRICA": return " África";
            case "ASIA": return " Asia";
            case "OCEANIA": return " Oceanía";
            default: return continenteId;
        }
    }
    private void ConectarAlServidor()
    {
        try
        {
            client = new TcpClient();
            client.Connect("127.0.0.1", 7777);
            stream = client.GetStream();
            isConnected = true;

            receiveThread = new Thread(new ThreadStart(RecibirMensajes));
            receiveThread.IsBackground = true;
            receiveThread.Start();

            textoEstado.text = " Conectado al servidor";

            string miRol = PlayerPrefs.GetString("MiRol", "JUGADOR1");
            string miAlias = miRol == "JUGADOR1" ?
                PlayerPrefs.GetString("Jugador1", "Jugador1") :
                PlayerPrefs.GetString("Jugador2", "Jugador2");

            EnviarAlServidor($"ALIAS:{miAlias}");
            oponenteId = miRol == "JUGADOR1" ? "JUGADOR2" : "JUGADOR1";
            ActualizarUIJugadores();

        }
        catch (Exception ex)
        {
            textoEstado.text = " Error de conexión";
            Debug.LogError($"Error de conexión: {ex.Message}");
        }
    }
    private void RecibirMensajes()
    {
        byte[] buffer = new byte[4096];
        StringBuilder messageBuilder = new StringBuilder();

        while (isConnected && client.Connected)
        {
            try
            {
                if (stream.DataAvailable)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        messageBuilder.Append(receivedData);

                        string completeData = messageBuilder.ToString();
                        string[] messages = completeData.Split('\n');

                        for (int i = 0; i < messages.Length - 1; i++)
                        {
                            string message = messages[i].Trim();
                            if (!string.IsNullOrEmpty(message))
                            {
                                lock (queueLock)
                                {
                                    messageQueue.Enqueue(message);
                                }
                            }
                        }

                        messageBuilder.Clear();
                        messageBuilder.Append(messages[messages.Length - 1]);
                    }
                }
                Thread.Sleep(50);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error en recepción: {ex.Message}");
                break;
            }
        }
    }
    void Update()
    {
        ProcesarMensajesEnCola();
        ActualizarUI();
    }

    private void ProcesarMensajesEnCola()
    {
        lock (queueLock)
        {
            while (!messageQueue.EstaVacia)
            {
                string message = messageQueue.Dequeue();
                ProcesarMensajeServidor(message);
            }
        }
    }

    private void ProcesarMensajeServidor(string message)
    {
        try
        {
            string[] partes = message.Split(':');
            if (partes.Length < 2) return;

            string comando = partes[0];
            string datos = partes[1];

            switch (comando)
            {
                case "ID":
                    miJugadorId = datos;
                    ActualizarNombresJugadoresConColor();
                    Debug.Log($"Identificado como: {miJugadorId}");
                    break;

                case "JUGADOR1_ALIAS":
                    aliasJugador1 = datos;
                    ActualizarNombresJugadoresConColor();
                    Debug.Log($" Alias Jugador1: {aliasJugador1}");
                    break;

                case "FIN_JUEGO":
                ProcesarFinJuego(datos);
                break;

                case "JUGADOR2_ALIAS":
                    aliasJugador2 = datos;
                    ActualizarNombresJugadoresConColor();
                    Debug.Log($"Alias Jugador2: {aliasJugador2}");
                    break;

                case "TERRITORIO_ASIGNADO":
                    ProcesarTerritorioAsignado(datos);
                    break;

                case "TROPAS_DISPONIBLES":
                    if (int.TryParse(datos, out int nuevasTropas))
                    {
                        tropasDisponibles = nuevasTropas;
                        ActualizarUI();
                        Debug.Log($"Tropas disponibles actualizadas: {tropasDisponibles}");
                        
                    }
                    break;

                case "TARJETA_OBTENIDA":
                    ProcesarTarjetaObtenida(datos);
                    break;

                case "TARJETAS_TRIO_DISPONIBLE":
                    tieneTrioDisponible = true;
                    textoEstado.text = "🎴 ¡Tienes un trío de tarjetas disponible para intercambiar!";
                    ActualizarUITarjetas();
                    break;

                case "TARJETAS_INTERCAMBIADAS":
                    ProcesarIntercambioCompletado(datos);
                    break;

                case "ACTUALIZAR_TARJETAS":
                    ActualizarTarjetasDesdeServidor(datos);
                    break;

                case "TURNO":
                    esMiTurno = (datos == "ACTIVO");
                    ActualizarIndicadoresTurno();
                    ActualizarUI();

                    if (esMiTurno)
                    {
                        // AL INICIAR TURNO, SUGERIR MODO COLOCAR SI HAY TROPAS
                        if (tropasDisponibles > 0)
                        {
                            CambiarAModoColocar();
                        }
                        else
                        {
                            CambiarAModoAtacar();
                        }
                    }
                    Debug.Log($" Estado de turno: {(esMiTurno ? "ACTIVO" : "ESPERA")}");
                    break;

                case "TROPAS_ACTUALIZADAS":
                    ProcesarTropasActualizadas(datos);
                    break;

                case "FASE_COLOCACION":
                    if (datos == "COMPLETADA")
                    {
                        textoEstado.text = " Fase inicial completada - Puedes COLOCAR o ATACAR";
                        Debug.Log("Fase de colocación inicial completada");
                    }
                    break;

                case "FASE_PRINCIPAL":
                    if (datos == "INICIADA")
                    {
                        textoEstado.text = " ¡Fase principal iniciada! - Usa los botones para COLOCAR o ATACAR";
                        Debug.Log("Fase principal iniciada - Modos disponibles");
                    }
                    break;

                case "MODO_ATAQUE":
                    if (datos == "DISPONIBLE")
                    {
                        textoEstado.text = " Modo ataque disponible - Usa el botón ATACAR cuando quieras";
                        Debug.Log("Modo ataque disponible");
                    }
                    break;

                case "TERRITORIOS_ATACABLES":
                    ProcesarTerritoriosAtacables(datos);
                    break;

                case "RESULTADO_COMBATE":
                    ProcesarResultadoCombate(datos);

                    // Cerrar automáticamente el panel después de 3 segundos
                    StartCoroutine(CerrarCombateAutomaticamente());
                    break;

                case "TERRITORIO_CONQUISTADO":
                    ProcesarTerritorioConquistado(datos);
                    break;

                case "ERROR":
                    textoEstado.text = $"Error: {datos}";
                    Debug.LogError($"Error del servidor: {datos}");

                    // RESETEAR ESTADO EN CASO DE ERROR
                    if (datos.Contains("atacar") || datos.Contains("Ataque"))
                    {
                        territorioAtacanteSeleccionado = -1;
                        cantidadTropasAtaque = 0;
                        estadoActual = EstadoJuego.SeleccionandoAtacante;
                        OcultarPanelAtaque();
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($" Error procesando mensaje: {ex.Message}");
        }
    }

    // ProcesarTerritoriosAtacables para verificar selección
    private void ProcesarTerritoriosAtacables(string datos)
    {
        // VERIFICAR QUE TODAVÍA TENEMOS UNA SELECCIÓN ACTIVA
        if (territorioAtacanteSeleccionado == -1 || cantidadTropasAtaque == 0)
        {
            Debug.LogError("Procesando territorios atacables pero no hay selección activa");
            return;
        }

        string[] territoriosArray = datos.Split(',');
        territoriosAtacables.Limpiar();

        foreach (string territorioStr in territoriosArray)
        {
            if (int.TryParse(territorioStr, out int territorioId))
            {
                territoriosAtacables.Agregar(territorioId);
            }
        }

        Debug.Log($"Recibidos {territoriosAtacables.Count} territorios atacables desde el servidor");

        if (territoriosAtacables.Count > 0)
        {
            textoEstado.text = $"{territoriosAtacables.Count} territorios enemigos disponibles. Haz clic en uno para atacar";
            estadoActual = EstadoJuego.SeleccionandoDefensor;
        }
        else
        {
            textoEstado.text = "No hay territorios enemigos adyacentes para atacar";
            // Resetear selección
            territorioAtacanteSeleccionado = -1;
            cantidadTropasAtaque = 0;
            estadoActual = EstadoJuego.SeleccionandoAtacante;
        }
    }

    private IEnumerator TimeoutSeleccionAtaque()
    {
        yield return new WaitForSeconds(10f); // 10 segundos de timeout

        if (estadoActual == EstadoJuego.SeleccionandoDefensor && cantidadTropasAtaque > 0)
        {
            Debug.LogWarning("Timeout en selección de ataque - Limpiando selección");
            textoEstado.text = "Tiempo agotado para seleccionar ataque";

            territorioAtacanteSeleccionado = -1;
            cantidadTropasAtaque = 0;
            estadoActual = EstadoJuego.SeleccionandoAtacante;
            DeshabilitarBotonesAtaque();
        }
    }

    // ProcesarResultadoCombate si es necesario
    private void ProcesarResultadoCombate(string datos)
    {
        string[] valores = datos.Split(',');
        if (valores.Length >= 6)
        {
            int territorioAtacante = int.Parse(valores[0]);
            int territorioDefensor = int.Parse(valores[1]);
            int perdidasAtacante = int.Parse(valores[2]);
            int perdidasDefensor = int.Parse(valores[3]);
            string dadosAtacante = valores[4];
            string dadosDefensor = valores[5];

            MostrarResultadoCombate(territorioAtacante, territorioDefensor,
                                  perdidasAtacante, perdidasDefensor,
                                  dadosAtacante, dadosDefensor);

            // ACTUALIZAR ESTADO CON RESULTADO
            textoEstado.text = $"Combate: Atacante perdió {perdidasAtacante}, Defensor perdió {perdidasDefensor}";
        }
    }

    // PROCESAR TERRITORIO CONQUISTADO EN EL CLIENTE
    private void ProcesarTerritorioConquistado(string datos)
    {
        string[] valores = datos.Split(',');
        if (valores.Length >= 3)
        {
            int territorioId = int.Parse(valores[0]);
            string nuevoPropietario = valores[1];
            int tropasMovidas = int.Parse(valores[2]);

            if (territorios.ContieneClave(territorioId))
            {
                var territorio = territorios.Obtener(territorioId);
                territorio.ActualizarPropietario(nuevoPropietario, tropasMovidas);

                Debug.Log($" Territorio {territorioId} conquistado por {nuevoPropietario} con {tropasMovidas} tropas movidas");

                // ACTUALIZAR ESTADO CON INFORMACIÓN DE LA CONQUISTA
                textoEstado.text = $"¡Territorio {territorioId} conquistado! Se movieron {tropasMovidas} tropas";
            }
        }
    }

    private void ActualizarNombresJugadoresConColor()
    {
        if (textoNombreJugador1 != null)
        {
            string colorHex1 = ColorUtility.ToHtmlStringRGB(colorJugador1);
            string nombreConColor1 = $"<color=#{colorHex1}>{aliasJugador1}</color>";

            if (miJugadorId == "JUGADOR1")
            {
                nombreConColor1 += " <color=#FFFF00>(TÚ)</color>";
            }

            textoNombreJugador1.text = nombreConColor1;
        }

        if (textoNombreJugador2 != null)
        {
            string colorHex2 = ColorUtility.ToHtmlStringRGB(colorJugador2);
            string nombreConColor2 = $"<color=#{colorHex2}>{aliasJugador2}</color>";

            if (miJugadorId == "JUGADOR2")
            {
                nombreConColor2 += " <color=#FFFF00>(TÚ)</color>";
            }

            textoNombreJugador2.text = nombreConColor2;
        }

        if (textoJugadorActual != null)
        {
            string miAlias = miJugadorId == "JUGADOR1" ? aliasJugador1 : aliasJugador2;
            string miColorHex = miJugadorId == "JUGADOR1" ?
                ColorUtility.ToHtmlStringRGB(colorJugador1) :
                ColorUtility.ToHtmlStringRGB(colorJugador2);
            textoJugadorActual.text = $"Tú: <color=#{miColorHex}>{miAlias}</color>";
        }

        if (textoOponente != null)
        {
            string oponenteAlias = oponenteId == "JUGADOR1" ? aliasJugador1 : aliasJugador2;
            string oponenteColorHex = oponenteId == "JUGADOR1" ?
                ColorUtility.ToHtmlStringRGB(colorJugador1) :
                ColorUtility.ToHtmlStringRGB(colorJugador2);
            textoOponente.text = $"Oponente: <color=#{oponenteColorHex}>{oponenteAlias}</color>";
        }
    }
    private void ActualizarIndicadoresTurno()
    {
        if (bordeTurnoJugador1 != null)
        {
            bool esTurnoJugador1 = (esMiTurno && miJugadorId == "JUGADOR1") || (!esMiTurno && miJugadorId == "JUGADOR2");
            bordeTurnoJugador1.color = esTurnoJugador1 ? colorTurnoActivo : colorTurnoInactivo;
        }

        if (bordeTurnoJugador2 != null)
        {
            bool esTurnoJugador2 = (esMiTurno && miJugadorId == "JUGADOR2") || (!esMiTurno && miJugadorId == "JUGADOR1");
            bordeTurnoJugador2.color = esTurnoJugador2 ? colorTurnoActivo : colorTurnoInactivo;
        }

        if (textoTurnoJugador1 != null)
        {
            bool esTurnoJugador1 = (esMiTurno && miJugadorId == "JUGADOR1") || (!esMiTurno && miJugadorId == "JUGADOR2");
            textoTurnoJugador1.text = esTurnoJugador1 ? " TURNO ACTIVO" : "ESPERANDO";
            textoTurnoJugador1.color = esTurnoJugador1 ? Color.yellow : new Color(0.7f, 0.7f, 0.7f);
        }

        if (textoTurnoJugador2 != null)
        {
            bool esTurnoJugador2 = (esMiTurno && miJugadorId == "JUGADOR2") || (!esMiTurno && miJugadorId == "JUGADOR1");
            textoTurnoJugador2.text = esTurnoJugador2 ? "TURNO ACTIVO" : "ESPERANDO";
            textoTurnoJugador2.color = esTurnoJugador2 ? Color.yellow : new Color(0.7f, 0.7f, 0.7f);
        }
    }
    private void ActualizarUIJugadores()
    {
        if (fondoJugador1 != null)
        {
            fondoJugador1.color = new Color(colorJugador1.r, colorJugador1.g, colorJugador1.b, 0.3f);
        }

        if (fondoJugador2 != null)
        {
            fondoJugador2.color = new Color(colorJugador2.r, colorJugador2.g, colorJugador2.b, 0.3f);
        }
        ActualizarNombresJugadoresConColor();
        ActualizarIndicadoresTurno();
    }
    private void ProcesarTerritorioAsignado(string datos)
    {
        try
        {
            string[] valores = datos.Split(',');
            if (valores.Length >= 3)
            {
                int territorioId = int.Parse(valores[0]);
                string propietario = valores[1];
                int tropas = int.Parse(valores[2]);

                if (territorios.ContieneClave(territorioId))
                {
                    var territorio = territorios.Obtener(territorioId);
                    territorio.ActualizarPropietario(propietario, tropas);

                    string nombreContinente = NombresContinentes.ObtenerNombreContinente(territorioId);
                    Debug.Log($"Territorio {territorioId} ({nombreContinente}) asignado a {propietario} con {tropas} tropas");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error procesando territorio asignado: {ex.Message}");
        }
    }
    private void ProcesarTropasActualizadas(string datos)
    {
        try
        {
            string[] valores = datos.Split(',');
            if (valores.Length >= 3)
            {
                int territorioId = int.Parse(valores[0]);
                string propietario = valores[1];
                int nuevasTropas = int.Parse(valores[2]);

                if (territorios.ContieneClave(territorioId))
                {
                    var territorio = territorios.Obtener(territorioId);
                    territorio.ActualizarTropas(nuevasTropas);

                    string nombreContinente = NombresContinentes.ObtenerNombreContinente(territorioId);
                    Debug.Log($"Territorio {territorioId} ({nombreContinente}) actualizado: {nuevasTropas} tropas");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error procesando tropas actualizadas: {ex.Message}");
        }
    }

    // ActualizarUI para reflejar 1 acción por turno
    private void ActualizarUI()
    {
        textoTropasDisponibles.text = $"Tropas: {tropasDisponibles}";

        if (textoTropasJugador1 != null)
        {
            textoTropasJugador1.text = $"Tropas: {(miJugadorId == "JUGADOR1" ? tropasDisponibles.ToString() : "?")}";
        }

        if (textoTropasJugador2 != null)
        {
            textoTropasJugador2.text = $"Tropas: {(miJugadorId == "JUGADOR2" ? tropasDisponibles.ToString() : "?")}";
        }

        // ACTUALIZAR BOTONES - SOLO DURANTE EL TURNO
        if (botonModoColocar != null)
            botonModoColocar.interactable = esMiTurno && (estadoActual != EstadoJuego.Colocando);
        if (botonModoAtacar != null)
            botonModoAtacar.interactable = esMiTurno && (estadoActual != EstadoJuego.SeleccionandoAtacante && estadoActual != EstadoJuego.SeleccionandoDefensor);

        // ACTUALIZAR ESTADO SEGÚN REGLA DE 1 ACCIÓN POR TURNO
        if (esMiTurno)
        {
            switch (estadoActual)
            {
                case EstadoJuego.Colocando:
                    if (tropasDisponibles > 0)
                    {
                        textoEstado.text = $"TU TURNO - Coloca 1 tropa en territorio PROPIO";
                    }
                    else
                    {
                        textoEstado.text = " No tienes tropas para colocar - Cambia a MODO ATACAR";
                    }
                    break;

                case EstadoJuego.SeleccionandoAtacante:
                    textoEstado.text = " TU TURNO - Selecciona territorio PROPIO con 2+ tropas para ATACAR";
                    break;

                case EstadoJuego.SeleccionandoDefensor:
                    if (cantidadTropasAtaque == 0)
                    {
                        textoEstado.text = $" Territorio {territorioAtacanteSeleccionado} seleccionado - Elige 1-3 tropas para ATACAR";
                    }
                    else
                    {
                        textoEstado.text = $" {cantidadTropasAtaque} tropas seleccionadas - Haz clic en territorio ENEMIGO adyacente";
                    }
                    break;
            }
        }
        else
        {
            textoEstado.text = " Esperando turno del oponente...";
        }
        if (esMiTurno && estadoActual == EstadoJuego.SeleccionandoAtacante)
        {
            textoEstado.text = "Selecciona un territorio PROPIO con 2+ tropas";
        }
        else if (esMiTurno && estadoActual == EstadoJuego.SeleccionandoDefensor)
        {
            if (cantidadTropasAtaque == 0)
            {
                textoEstado.text = $"Territorio {territorioAtacanteSeleccionado} - Elige tropas para atacar";
            }
            else
            {
                textoEstado.text = $"{cantidadTropasAtaque} tropas seleccionadas - Selecciona ENEMIGO";
            }
        }
    }

    // PROCESAR FIN DEL JUEGO
    private void ProcesarFinJuego(string datos)
    {
        try
        {
            string[] valores = datos.Split(',');
            string jugadorGanador = valores[0];
            int territoriosConquistados = int.Parse(valores[1]);

            // GUARDAR DATOS PARA LA ESCENA FINAL
            PlayerPrefs.SetString("JugadorGanador", jugadorGanador);
            PlayerPrefs.SetInt("TerritoriosConquistados", territoriosConquistados);
            PlayerPrefs.Save();

            // CARGAR ESCENA DE FIN DE JUEGO
            UnityEngine.SceneManagement.SceneManager.LoadScene("FinJuego");

            Debug.Log($"Fin del juego: {jugadorGanador} ganó con {territoriosConquistados} territorios");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error procesando fin del juego: {ex.Message}");
        }
    }

    private void EnviarAlServidor(string message)
    {
        if (!isConnected || stream == null)
        {
            Debug.LogWarning("No conectado para enviar mensaje");
            return;
        }
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message + "\n");
            stream.Write(data, 0, data.Length);
            Debug.Log($"Enviado al servidor: {message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error enviando: {ex.Message}");
        }
    }

    // InicializarJuego para deshabilitar botones de ataque al inicio
    private void InicializarJuego()
    {
        textoEstado.text = "Conectando...";
        textoTropasDisponibles.text = "Tropas disponibles: 0";
        textoJugadorActual.text = "Jugador: ...";

        InicializarTerritorios();
        ConfigurarBotonesContinentes();
        ConfigurarBotonesAtaque();
        ConfigurarBotonesModo();
        ConfigurarBotonAjustes();
        ConfigurarSistemaTarjetas();

        if (panelInfoJugadores != null) panelInfoJugadores.SetActive(true);
        if (panelContinentes != null) panelContinentes.SetActive(true);

        // DESHABILITAR BOTONES DE ATAQUE AL INICIO
        DeshabilitarBotonesAtaque();

        // INICIAR EN MODO COLOCACIÓN POR DEFECTO
        CambiarAModoColocar();

        ActualizarUIJugadores();
        Debug.Log("Juego inicializado - Botones de ataque deshabilitados");
    }

    // DeshabilitarBotonesAtaque para solo deshabilitar
    private void DeshabilitarBotonesAtaque()
    {
        if (botonAtacar1 != null)
        {
            botonAtacar1.interactable = false;
        }

        if (botonAtacar2 != null)
        {
            botonAtacar2.interactable = false;
        }

        if (botonAtacar3 != null)
        {
            botonAtacar3.interactable = false;
        }
        Debug.Log("Botones de ataque deshabilitados");
    }

    private void InicializarTerritorios()
    {
        for (int i = 0; i < botonesTerritorios.Length; i++)
        {
            if (botonesTerritorios[i] == null) continue;

            int territorioId = i;
            Territory territorio = botonesTerritorios[i].GetComponent<Territory>();
            if (territorio == null)
            {
                territorio = botonesTerritorios[i].gameObject.AddComponent<Territory>();
            }
            territorio.Inicializar(territorioId);
            territorios.Agregar(territorioId, territorio);

            botonesTerritorios[i].onClick.RemoveAllListeners();
            botonesTerritorios[i].onClick.AddListener(() => OnTerritorioClickeado(territorioId));
        }
        Debug.Log($"Inicializados {territorios.Count} territorios");
    }

    // Modificar OnTerritorioClickeado para manejar mejor los modos
    public void OnTerritorioClickeado(int territorioId)
    {
        Debug.Log($" Clic en territorio {territorioId} - Estado: {estadoActual} - Tropas disponibles: {tropasDisponibles}");
        DebugEstadoAtaque();

        if (!esMiTurno)
        {
            textoEstado.text = " ¡No es tu turno!";
            return;
        }

        if (!isConnected)
        {
            textoEstado.text = " ¡Sin conexión!";
            return;
        }

        if (!territorios.ContieneClave(territorioId))
            return;

        var territorio = territorios.Obtener(territorioId);

        switch (estadoActual)
        {
            case EstadoJuego.Colocando:
                ProcesarClicModoColocar(territorioId, territorio);
                break;

            case EstadoJuego.SeleccionandoAtacante:
                ProcesarClicSeleccionarAtacante(territorioId, territorio);
                break;

            case EstadoJuego.SeleccionandoDefensor:
                ProcesarClicSeleccionarDefensor(territorioId, territorio);
                break;
        }
    }

    // ProcesarClicModoColocar para resetear después de colocar
    private void ProcesarClicModoColocar(int territorioId, Territory territorio)
    {
        if (territorio.GetPropietario() != miJugadorId)
        {
            textoEstado.text = " ¡Solo puedes colocar en territorios propios!";
            return;
        }

        if (territorio.GetTropas() >= maxTropasPorTerritorio)
        {
            textoEstado.text = $" ¡Este territorio ya tiene {maxTropasPorTerritorio} tropas!";
            return;
        }

        if (tropasDisponibles < 1)
        {
            textoEstado.text = " ¡No tienes tropas disponibles para colocar!";
            return;
        }

        // COLOCAR 1 TROPA - EL SERVIDRO CAMBIARÁ EL TURNO AUTOMÁTICAMENTE
        EnviarAlServidor($"COLOCAR_TROPAS:{territorioId},1");
        textoEstado.text = $" Colocando 1 tropa en territorio {territorioId}... Cambiando turno...";

        Debug.Log($" Colocando 1 tropa en territorio {territorioId} - Turno cambiará automáticamente");
    }

    // ProcesarClicSeleccionarAtacante para habilitar botones
    private void ProcesarClicSeleccionarAtacante(int territorioId, Territory territorio)
    {
        // VERIFICAR QUE EL TERRITORIO ES PROPIO
        if (territorio.GetPropietario() != miJugadorId)
        {
            textoEstado.text = "¡Solo puedes atacar desde territorios propios!";
            return;
        }

        // VERIFICAR QUE TIENE AL MENOS 2 TROPAS
        int tropasEnTerritorio = territorio.GetTropas();
        if (tropasEnTerritorio < 2)
        {
            textoEstado.text = "Necesitas al menos 2 tropas para atacar";
            return;
        }

        // CALCULAR MÁXIMO PERMITIDO
        int maxTropasAtaque = Math.Min(3, tropasEnTerritorio - 1);
        if (maxTropasAtaque < 1)
        {
            textoEstado.text = "No puedes atacar (necesitas dejar al menos 1 tropa)";
            return;
        }

        // SELECCIONAR TERRITORIO ATACANTE Y HABILITAR BOTONES
        territorioAtacanteSeleccionado = territorioId;
        estadoActual = EstadoJuego.SeleccionandoDefensor;

        // HABILITAR BOTONES SEGÚN TROPAS DISPONIBLES
        MostrarPanelAtaque(territorioId);

        textoEstado.text = $"Territorio {territorioId} seleccionado. Elige 1-{maxTropasAtaque} tropas para atacar";
        Debug.Log($"Territorio atacante: {territorioId} con {tropasEnTerritorio} tropas - Máximo: {maxTropasAtaque}");
    }


    // ProcesarClicSeleccionarDefensor para eliminar referencia a colores
    private void ProcesarClicSeleccionarDefensor(int territorioId, Territory territorio)
    {
        if (territorio.GetPropietario() == miJugadorId)
        {
            textoEstado.text = "No puedes atacar tus propios territorios";
            return;
        }

        if (cantidadTropasAtaque == 0)
        {
            textoEstado.text = "Primero selecciona cuántas tropas usar para atacar";
            return;
        }

        // VERIFICAR QUE EL TERRITORIO ES ATACABLE
        bool esAtacable = false;
        for (int i = 0; i < territoriosAtacables.Count; i++)
        {
            if (territoriosAtacables.Obtener(i) == territorioId)
            {
                esAtacable = true;
                break;
            }
        }

        if (!esAtacable)
        {
            textoEstado.text = "Este territorio no es atacable desde tu posición";
            return;
        }

        // EJECUTAR ATAQUE
        Debug.Log($"Atacando: {territorioAtacanteSeleccionado} -> {territorioId} con {cantidadTropasAtaque} tropas");
        EnviarAlServidor($"ATACAR:{territorioId}");

        textoEstado.text = $"Atacando territorio {territorioId}... Cambiando turno...";

        // DESHABILITAR BOTONES DESPUÉS DEL ATAQUE
        DeshabilitarBotonesAtaque();

        // Resetear estado
        territorioAtacanteSeleccionado = -1;
        cantidadTropasAtaque = 0;
        estadoActual = EstadoJuego.Colocando; // Volver a modo colocar por defecto
    }

    // corutina para manejar el timing
    private System.Collections.IEnumerator EnviarAtaqueDespuesDeSeleccion(int territorioDefensorId)
    {
        Debug.Log($" Esperando para enviar ataque a {territorioDefensorId}...");

        // Esperar un frame para que el servidor procese la selección
        yield return new WaitForSeconds(0.1f);

        // ENVIAR COMANDO DE ATAQUE
        Debug.Log($" Enviando ATACAR: {territorioDefensorId}");
        EnviarAlServidor($"ATACAR:{territorioDefensorId}");

        textoEstado.text = $" Atacando territorio {territorioDefensorId}...";

        // RESETEAR ESTADO DESPUÉS DEL ATAQUE
        territorioAtacanteSeleccionado = -1;
        cantidadTropasAtaque = 0;
        estadoActual = EstadoJuego.Colocando;
        OcultarPanelAtaque();
    }

    // Para cerrar panel de combate automáticamente
    private System.Collections.IEnumerator CerrarCombateAutomaticamente()
    {
        yield return new WaitForSeconds(5f); // Esperar 5 segundos
        CerrarPanelCombate();
    }

    // Estados del juego
    private enum EstadoJuego { Colocando, SeleccionandoAtacante, SeleccionandoDefensor }
    private EstadoJuego estadoActual = EstadoJuego.Colocando;

    private bool modoColocacion = true;

    // CONFIGURAR BOTONES DE MODO
    private void ConfigurarBotonesModo()
    {
        if (botonModoColocar != null)
        {
            botonModoColocar.onClick.RemoveAllListeners();
            botonModoColocar.onClick.AddListener(() => CambiarAModoColocar());
        }

        if (botonModoAtacar != null)
        {
            botonModoAtacar.onClick.RemoveAllListeners();
            botonModoAtacar.onClick.AddListener(() => CambiarAModoAtacar());
        }
    }

    // CambiarAModoColocar para deshabilitar botones de ataque
    private void CambiarAModoColocar()
    {
        estadoActual = EstadoJuego.Colocando;
        territorioAtacanteSeleccionado = -1;
        cantidadTropasAtaque = 0;

        if (textoModoActual != null)
            textoModoActual.text = "MODO: COLOCAR TROPAS";

        // Actualizar estado de botones de modo
        if (botonModoColocar != null) botonModoColocar.interactable = false;
        if (botonModoAtacar != null) botonModoAtacar.interactable = true;

        // DESHABILITAR BOTONES DE ATAQUE AL CAMBIAR A MODO COLOCAR
        DeshabilitarBotonesAtaque();

        textoEstado.text = "MODO COLOCAR: Haz clic en territorio PROPIO para añadir 1 tropa";

        Debug.Log("Cambiado a MODO COLOCAR - Botones de ataque deshabilitados");
    }

    // CambiarAModoAtacar para deshabilitar botones al cambiar a modo atacar
    private void CambiarAModoAtacar()
    {
        estadoActual = EstadoJuego.SeleccionandoAtacante;
        territorioAtacanteSeleccionado = -1;
        cantidadTropasAtaque = 0;

        if (textoModoActual != null)
            textoModoActual.text = "MODO: ATACAR";

        // Actualizar estado de botones de modo
        if (botonModoColocar != null) botonModoColocar.interactable = true;
        if (botonModoAtacar != null) botonModoAtacar.interactable = false;

        // DESHABILITAR BOTONES DE ATAQUE HASTA QUE SE SELECCIONE UN TERRITORIO
        DeshabilitarBotonesAtaque();
        textoEstado.text = "MODO ATACAR: Selecciona un territorio PROPIO con 2+ tropas";
        Debug.Log("Cambiado a MODO ATACAR - Botones de ataque deshabilitados");
    }

    void OnDestroy()
    {
        isConnected = false;
        stream?.Close();
        client?.Close();

        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
        Debug.Log("GameManager destruido - conexiones cerradas");
    }
}