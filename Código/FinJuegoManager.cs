/*
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FinJuegoManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI textoGanador;
    public TextMeshProUGUI textoPerdedor;
    public TextMeshProUGUI textoTerritorios;
    public Button botonSalir;

    void Start()
    {
        Debug.Log("FinJuegoManager iniciado");

        // VERIFICAR QUE LAS REFERENCIAS ESTÁN CONECTADAS
        if (textoGanador == null) Debug.LogError("textoGanador no está conectado");
        if (textoPerdedor == null) Debug.LogError("textoPerdedor no está conectado");
        if (textoTerritorios == null) Debug.LogError("textoTerritorios no está conectado");
        if (botonSalir == null) Debug.LogError("botonSalir no está conectado");

        // CONFIGURAR BOTÓN SALIR
        if (botonSalir != null)
        {
            botonSalir.onClick.RemoveAllListeners();
            botonSalir.onClick.AddListener(SalirDelJuego);
        }

        // MOSTRAR RESULTADOS
        MostrarResultados();
    }

    private void MostrarResultados()
    {
        Debug.Log("Mostrando resultados del juego...");

        // OBTENER DATOS GUARDADOS
        string jugadorGanador = PlayerPrefs.GetString("JugadorGanador", "JUGADOR1");
        int territoriosConquistados = PlayerPrefs.GetInt("TerritoriosConquistados", 4);

        Debug.Log($"Datos guardados: Ganador={jugadorGanador}, Territorios={territoriosConquistados}");

        // DETERMINAR JUGADOR PERDEDOR
        string jugadorPerdedor = jugadorGanador == "JUGADOR1" ? "JUGADOR2" : "JUGADOR1";

        // MOSTRAR EN PANTALLA
        if (textoGanador != null)
        {
            textoGanador.text = $"{jugadorGanador} HA GANADO ";
            textoGanador.color = Color.green;
            Debug.Log($"Texto ganador actualizado: {textoGanador.text}");
        }

        if (textoPerdedor != null)
        {
            textoPerdedor.text = $"{jugadorPerdedor} HA PERDIDO ";
            textoPerdedor.color = Color.red;
            Debug.Log($"Texto perdedor actualizado: {textoPerdedor.text}");
        }

        if (textoTerritorios != null)
        {
            textoTerritorios.text = $"{jugadorGanador} conquistó {territoriosConquistados} territorios";
            Debug.Log($"Texto territorios actualizado: {textoTerritorios.text}");
        }
    }

    private void SalirDelJuego()
    {
        Debug.Log("Cerrando juego...");

        // CERRAR EL JUEGO
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
*/