using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AjustesManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button botonToggleMusica;
    public TextMeshProUGUI textoEstadoMusica;
    public Slider sliderVolumen;
    public Button botonVolver;

    [Header("Colores del Botón")]
    public Color colorEncendido = new Color(0.2f, 0.8f, 0.2f);  // Verde
    public Color colorApagado = new Color(0.8f, 0.2f, 0.2f);   // Rojo
    public Color colorTexto = Color.white;                      // Texto blanco

    private void Start()
    {
        ConfigurarBotones();
        ActualizarUI();
    }

    private void ConfigurarBotones()
    {
        if (botonToggleMusica != null)
        {
            botonToggleMusica.onClick.RemoveAllListeners();
            botonToggleMusica.onClick.AddListener(ToggleMusica);
            
            // ✅ CONFIGURAR COLORES DE TRANSICIÓN DEL BOTÓN
            ColorBlock coloresBoton = botonToggleMusica.colors;
            coloresBoton.normalColor = AudioManager.Instance.musicEnabled ? colorEncendido : colorApagado;
            coloresBoton.highlightedColor = AudioManager.Instance.musicEnabled ? 
                Color.Lerp(colorEncendido, Color.white, 0.3f) : 
                Color.Lerp(colorApagado, Color.white, 0.3f);
            coloresBoton.pressedColor = AudioManager.Instance.musicEnabled ? 
                Color.Lerp(colorEncendido, Color.black, 0.3f) : 
                Color.Lerp(colorApagado, Color.black, 0.3f);
            botonToggleMusica.colors = coloresBoton;
        }

        if (sliderVolumen != null)
        {
            sliderVolumen.onValueChanged.RemoveAllListeners();
            sliderVolumen.onValueChanged.AddListener(CambiarVolumen);
            sliderVolumen.value = AudioManager.Instance.musicVolume;
        }

        if (botonVolver != null)
        {
            botonVolver.onClick.RemoveAllListeners();
            botonVolver.onClick.AddListener(VolverAlJuego);
        }
    }

    private void ToggleMusica()
    {
        AudioManager.Instance.ToggleMusic();
        ActualizarUI();
    }

    private void CambiarVolumen(float volumen)
    {
        AudioManager.Instance.SetMusicVolume(volumen);
    }

    private void VolverAlJuego()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }

    private void ActualizarUI()
    {
        // ✅ ACTUALIZAR TEXTO
        if (textoEstadoMusica != null)
        {
            textoEstadoMusica.text = AudioManager.Instance.musicEnabled ? 
                "MÚSICA: ACTIVADA" : "MÚSICA: DESACTIVADA";
        }

        // ✅ ACTUALIZAR BOTÓN CON COLORES
        if (botonToggleMusica != null)
        {
            Image imagenBoton = botonToggleMusica.GetComponent<Image>();
            if (imagenBoton != null)
            {
                imagenBoton.color = AudioManager.Instance.musicEnabled ? 
                    colorEncendido : colorApagado;
            }

            // ✅ ACTUALIZAR TEXTO DEL BOTÓN
            TextMeshProUGUI textoBoton = botonToggleMusica.GetComponentInChildren<TextMeshProUGUI>();
            if (textoBoton != null)
            {
                textoBoton.text = AudioManager.Instance.musicEnabled ? 
                    "ENCENDIDA" : "APAGADA";
                textoBoton.color = colorTexto;
            }

            // ✅ ACTUALIZAR COLORES DE TRANSICIÓN
            ColorBlock coloresBoton = botonToggleMusica.colors;
            coloresBoton.normalColor = AudioManager.Instance.musicEnabled ? colorEncendido : colorApagado;
            coloresBoton.highlightedColor = AudioManager.Instance.musicEnabled ? 
                Color.Lerp(colorEncendido, Color.white, 0.3f) : 
                Color.Lerp(colorApagado, Color.white, 0.3f);
            coloresBoton.pressedColor = AudioManager.Instance.musicEnabled ? 
                Color.Lerp(colorEncendido, Color.black, 0.3f) : 
                Color.Lerp(colorApagado, Color.black, 0.3f);
            botonToggleMusica.colors = coloresBoton;
        }

        // ✅ ACTUALIZAR SLIDER
        if (sliderVolumen != null)
        {
            sliderVolumen.interactable = AudioManager.Instance.musicEnabled;
        }
    }

    void OnEnable()
    {
        ActualizarUI();
    }
}