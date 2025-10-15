using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // ¡Nuevo namespace!

public class Salirdeljuego : MonoBehaviour
{
    [Header("Configuración de Confirmación")]
    public GameObject panelConfirmacion;
    public Text mensajeTexto;
    public Button siButton;
    public Button noButton;
    public Button salirButton;

    [Header("Personalización")]
    public string mensajeConfirmacion = "¿Estás seguro de que quieres salir?";

    // Referencia al Input System
    private Keyboard keyboard;

    void Start()
    {
        // Obtener referencia al teclado
        keyboard = Keyboard.current;

        // Ocultar panel de confirmación al iniciar
        if (panelConfirmacion != null)
        {
            panelConfirmacion.SetActive(false);
        }

        ConfigurarBotones();
    }

    void ConfigurarBotones()
    {
        if (salirButton != null)
        {
            salirButton.onClick.RemoveAllListeners();
            salirButton.onClick.AddListener(AbrirConfirmacion);
        }

        if (siButton != null)
        {
            siButton.onClick.RemoveAllListeners();
            siButton.onClick.AddListener(ConfirmarSalida);
        }

        if (noButton != null)
        {
            noButton.onClick.RemoveAllListeners();
            noButton.onClick.AddListener(CerrarConfirmacion);
        }

        if (mensajeTexto != null)
        {
            mensajeTexto.text = mensajeConfirmacion;
        }
    }

    void Update()
    {
        // Usar Input System en lugar de Input clásico
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            AbrirConfirmacion();
        }
    }

    public void AbrirConfirmacion()
    {
        Debug.Log("⚠️ Abriendo panel de confirmación...");

        if (panelConfirmacion != null)
        {
            panelConfirmacion.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            ConfirmarSalida();
        }
    }

    public void ConfirmarSalida()
    {
        Debug.Log("🎮 Cerrando juego...");
        Time.timeScale = 1f;
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void CerrarConfirmacion()
    {
        Debug.Log("✅ Continuando con el juego...");

        if (panelConfirmacion != null)
        {
            panelConfirmacion.SetActive(false);
        }

        Time.timeScale = 1f;
    }
}
