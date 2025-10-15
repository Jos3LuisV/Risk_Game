using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JuegoManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text jugador1Texto;
    public TMP_Text jugador2Texto;
    public Image colorJugador1Img;
    public Image colorJugador2Img;
    public Image colorNeutroImg;

    [Header("Colores de Jugadores")]
    public Color colorAzul = Color.blue;
    public Color colorRojo = Color.red;
    public Color colorVerde = Color.green;
    public Color colorAmarillo = Color.yellow;
    public Color colorNeutro = Color.gray;

    void Start()
    {
        // Usar las MISMAS keys que el LobbyManager
        string aliasJugador1 = PlayerPrefs.GetString("Jugador1", "Jugador1");
        string aliasJugador2 = PlayerPrefs.GetString("Jugador2", "Jugador2");

        // Leer el color que se guardó en el lobby
        int colorIndexJugador1 = PlayerPrefs.GetInt("MiColorIndex", 0);
        int colorIndexJugador2 = (colorIndexJugador1 == 0) ? 1 : 0;

        // Resto de tu código...
        jugador1Texto.text = aliasJugador1;
        jugador2Texto.text = aliasJugador2;

        // ✅ ESTO SÍ funcionará porque estás en la escena correcta
        colorJugador1Img.color = ObtenerColorPorIndex(colorIndexJugador1);
        colorJugador2Img.color = ObtenerColorPorIndex(colorIndexJugador2);
    }

    private Color ObtenerColorPorIndex(int index)
    {
        return index switch
        {
            0 => colorAzul,
            1 => colorRojo,
            2 => colorVerde,
            3 => colorAmarillo,
            _ => Color.white
        };
    }

    public Color ObtenerColorNeutro()
    {
        return colorNeutro;
    }

    // 🔄 CAMBIO: Eliminar OnApplicationQuit si quieres persistir datos
    // o mantenerlo si quieres limpiar al salir
    private void OnApplicationQuit()
    {
        // Opcional: Limpiar datos al salir completamente
        PlayerPrefs.DeleteKey("Jugador1");
        PlayerPrefs.DeleteKey("Jugador2");
        PlayerPrefs.DeleteKey("ColorIndexJugador1");
        PlayerPrefs.DeleteKey("ColorIndexJugador2");
        PlayerPrefs.DeleteKey("ColorNeutro");
    }
}