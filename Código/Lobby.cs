using UnityEngine;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviour
{
    public void OnJugarClick()
    {
        // En lugar de cargar directamente el juego, cargar el Lobby
        SceneManager.LoadScene("LobbyScene");
    }

    public void OnSalirClick()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
