using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    [Header("Conexión")]
    public string serverIP = "127.0.0.1";
    public int serverPort = 7777;

    [Header("UI References")]
    public TMP_InputField aliasInputField;
    public Button crearPartidaBtn;
    public Button unirsePartidaBtn;
    public TMP_Text estadoTexto;
    public TMP_Text jugador1Texto;
    public TMP_Text jugador2Texto;

    private TcpClient tcpClient;
    private NetworkStream networkStream;
    private Thread receiveThread;
    private bool isConnected = false;
    private string miAlias = "";
    private string aliasJugador1 = "Esperando...";
    private string aliasJugador2 = "Esperando...";
    private string miRol = "";

    void Start()
    {
        // Limpiar PlayerPrefs para nueva sesión
        PlayerPrefs.DeleteKey("MiRol");
        PlayerPrefs.DeleteKey("Jugador1");
        PlayerPrefs.DeleteKey("Jugador2");

        // Configurar UI
        if (crearPartidaBtn != null) crearPartidaBtn.onClick.AddListener(CrearPartida);
        if (unirsePartidaBtn != null) unirsePartidaBtn.onClick.AddListener(UnirseAPartida);

        ActualizarUI();
        estadoTexto.text = "Ingresa tu alias para comenzar";
    }

    public void CrearPartida()
    {
        if (string.IsNullOrEmpty(aliasInputField.text))
        {
            estadoTexto.text = " ¡Debes ingresar un alias!";
            return;
        }

        miAlias = aliasInputField.text.Trim();
        estadoTexto.text = " Iniciando servidor...";
        DeshabilitarBotones();

        ConectarAlServidor();
    }

    public void UnirseAPartida()
    {
        if (string.IsNullOrEmpty(aliasInputField.text))
        {
            estadoTexto.text = " ¡Debes ingresar un alias!";
            return;
        }

        miAlias = aliasInputField.text.Trim();
        estadoTexto.text = " Conectando al servidor...";
        DeshabilitarBotones();
        ConectarAlServidor();
    }

    private void ConectarAlServidor()
    {
        try
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(serverIP, serverPort);
            networkStream = tcpClient.GetStream();
            isConnected = true;

            estadoTexto.text = " Conectado al servidor!";

            receiveThread = new Thread(new ThreadStart(RecibirMensajes));
            receiveThread.IsBackground = true;
            receiveThread.Start();

            // Enviar alias inmediatamente
            EnviarMensaje($"ALIAS:{miAlias}");
            Debug.Log($" Alias enviado: {miAlias}");

        }
        catch (Exception e)
        {
            estadoTexto.text = $" Error: {e.Message}";
            HabilitarBotones();
            Debug.LogError($"Error conectando: {e.Message}");
        }
    }

    private void RecibirMensajes()
    {
        byte[] buffer = new byte[4096];
        StringBuilder messageBuilder = new StringBuilder();

        while (isConnected && tcpClient.Connected)
        {
            try
            {
                if (networkStream.DataAvailable)
                {
                    int bytesRead = networkStream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        messageBuilder.Append(receivedData);

                        string completeData = messageBuilder.ToString();
                        string[] messages = completeData.Split('\n');

                        // Procesar mensajes completos
                        for (int i = 0; i < messages.Length - 1; i++)
                        {
                            string message = messages[i].Trim();
                            if (!string.IsNullOrEmpty(message))
                            {
                                ProcesarMensaje(message);
                            }
                        }

                        // Guardar mensaje incompleto para la próxima iteración
                        messageBuilder.Clear();
                        messageBuilder.Append(messages[messages.Length - 1]);
                    }
                }
                Thread.Sleep(50);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error recibiendo: {e.Message}");
                Desconectar();
                break;
            }
        }
    }

    private void ProcesarMensaje(string mensaje)
    {
        Debug.Log($" Mensaje recibido: {mensaje}");

        // Ejecutar en el hilo principal de Unity
        MainThreadDispatcher.ExecuteOnMainThread(() => {
            ProcesarMensajeEnMainThread(mensaje);
        });
    }

    private void ProcesarMensajeEnMainThread(string mensaje)
    {
        try
        {
            if (mensaje.StartsWith("ID:JUGADOR1"))
            {
                miRol = "JUGADOR1";
                PlayerPrefs.SetString("MiRol", "JUGADOR1");
                PlayerPrefs.SetString("Jugador1", miAlias);
                estadoTexto.text = " Eres el Jugador 1 (Azul)";
                Debug.Log(" Identificado como JUGADOR1");
            }
            else if (mensaje.StartsWith("ID:JUGADOR2"))
            {
                miRol = "JUGADOR2";
                PlayerPrefs.SetString("MiRol", "JUGADOR2");
                PlayerPrefs.SetString("Jugador2", miAlias);
                estadoTexto.text = " Eres el Jugador 2 (Rojo)";
                Debug.Log(" Identificado como JUGADOR2");
            }
            else if (mensaje.StartsWith("JUGADOR1_ALIAS:"))
            {
                aliasJugador1 = mensaje.Substring(15);
                PlayerPrefs.SetString("Jugador1", aliasJugador1);
                ActualizarUI();
            }
            else if (mensaje.StartsWith("JUGADOR2_ALIAS:"))
            {
                aliasJugador2 = mensaje.Substring(15);
                PlayerPrefs.SetString("Jugador2", aliasJugador2);
                ActualizarUI();
            }
            else if (mensaje == "INICIAR_JUEGO")
            {
                estadoTexto.text = " ¡Iniciando juego!";
                PlayerPrefs.Save();
                Invoke("IniciarJuego", 1f);
            }
            else if (mensaje.StartsWith("ERROR:"))
            {
                string error = mensaje.Substring(6);
                estadoTexto.text = $" Error: {error}";
                HabilitarBotones();
            }
            else if (mensaje == "OPONENTE_DESCONECTADO")
            {
                estadoTexto.text = " Oponente desconectado";
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error procesando mensaje: {ex.Message}");
        }
    }

    private void ActualizarUI()
    {
        if (jugador1Texto != null) jugador1Texto.text = $"Jugador 1: {aliasJugador1}";
        if (jugador2Texto != null) jugador2Texto.text = $"Jugador 2: {aliasJugador2}";

        if (aliasJugador1 != "Esperando..." && aliasJugador2 != "Esperando...")
        {
            estadoTexto.text = $" {aliasJugador1} vs {aliasJugador2} - Listos!";
        }
    }

    private void IniciarJuego()
    {
        SceneManager.LoadScene("Juego");
    }

    public void EnviarMensaje(string mensaje)
    {
        if (!isConnected || networkStream == null)
        {
            Debug.LogWarning(" No conectado para enviar mensaje");
            return;
        }

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(mensaje + "\n");
            networkStream.Write(data, 0, data.Length);
            Debug.Log($" Enviado: {mensaje}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error enviando: {e.Message}");
            Desconectar();
        }
    }

    private void Desconectar()
    {
        isConnected = false;

        try
        {
            networkStream?.Close();
            tcpClient?.Close();
            receiveThread?.Abort();
        }
        catch { }

        MainThreadDispatcher.ExecuteOnMainThread(() => {
            estadoTexto.text = " Desconectado del servidor";
            HabilitarBotones();
        });
    }

    private void DeshabilitarBotones()
    {
        if (crearPartidaBtn != null) crearPartidaBtn.interactable = false;
        if (unirsePartidaBtn != null) unirsePartidaBtn.interactable = false;
        if (aliasInputField != null) aliasInputField.interactable = false;
    }

    private void HabilitarBotones()
    {
        if (crearPartidaBtn != null) crearPartidaBtn.interactable = true;
        if (unirsePartidaBtn != null) unirsePartidaBtn.interactable = true;
        if (aliasInputField != null) aliasInputField.interactable = true;
    }

    void OnApplicationQuit()
    {
        Desconectar();
    }

    void OnDestroy()
    {
        Desconectar();
    }
}
