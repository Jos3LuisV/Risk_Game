using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Territory : MonoBehaviour
{
    private int territorioId;
    private string propietario = "NEUTRO";
    private int tropas = 0;

    private TextMeshProUGUI textoTropas;
    private Image fondo;

    // COLORES CON BUEN CONTRASTE
    private readonly Color colorJugador1 = new Color(0.1f, 0.3f, 0.8f, 0.9f);  // Azul fuerte
    private readonly Color colorJugador2 = new Color(0.8f, 0.2f, 0.2f, 0.9f);  // Rojo fuerte
    private readonly Color colorNeutro = new Color(0.4f, 0.4f, 0.4f, 0.9f);    // Gris fuerte

    public void Inicializar(int id)
    {
        territorioId = id;
        ConfigurarTerritorioVisualmente();
        ActualizarVisualizacion();
    }

    private void ConfigurarTerritorioVisualmente()
    {
        // CONFIGURAR FONDO
        ConfigurarFondo();

        // CONFIGURAR TEXTO
        ConfigurarTexto();

        // CONFIGURAR BORDE
        ConfigurarBorde();
    }

    private void ConfigurarFondo()
    {
        fondo = GetComponent<Image>();
        if (fondo == null)
        {
            fondo = gameObject.AddComponent<Image>();
        }

        // FONDO OPACO PARA MEJOR VISIBILIDAD
        fondo.color = colorNeutro;
    }

    private void ConfigurarTexto()
    {
        textoTropas = GetComponentInChildren<TextMeshProUGUI>();

        if (textoTropas != null)
        {
            // TEXTO GRANDE Y CON BUEN CONTRASTE
            textoTropas.fontSize = 28;
            textoTropas.alignment = TextAlignmentOptions.Center;
            textoTropas.verticalAlignment = VerticalAlignmentOptions.Middle;
            textoTropas.fontStyle = FontStyles.Bold;
            textoTropas.color = Color.white; 

            // SOMBRA PARA DESTACAR TEXTO
            var shadow = textoTropas.GetComponent<Shadow>();
            if (shadow == null)
            {
                shadow = textoTropas.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(0, 0, 0, 0.8f);
                shadow.effectDistance = new Vector2(2, -2);
            }

            // CONTORNO PARA TEXTO
            var outline = textoTropas.GetComponent<Outline>();
            if (outline == null)
            {
                outline = textoTropas.gameObject.AddComponent<Outline>();
                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(1, 1);
            }
        }
    }

    private void ConfigurarBorde()
    {
        // AGREGAR BORDE AL TERRITORIO
        var outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(3, 3);
            outline.effectColor = new Color(1, 1, 1, 0.8f);
        }
    }

    public void ActualizarPropietario(string nuevoPropietario, int nuevasTropas)
    {
        propietario = nuevoPropietario;
        tropas = nuevasTropas;
        ActualizarVisualizacion();
    }

    public void ActualizarTropas(int nuevasTropas)
    {
        tropas = nuevasTropas;
        ActualizarVisualizacion();
    }

    private void ActualizarVisualizacion()
    {
        // ACTUALIZAR TEXTO
        if (textoTropas != null)
        {
            textoTropas.text = tropas.ToString();
        }

        // ACTUALIZAR COLOR DE FONDO
        if (fondo != null)
        {
            switch (propietario)
            {
                case "JUGADOR1":
                    fondo.color = colorJugador1;
                    break;
                case "JUGADOR2":
                    fondo.color = colorJugador2;
                    break;
                case "NEUTRO":
                    fondo.color = colorNeutro;
                    break;
            }
        }
    }

    public string GetPropietario() => propietario;
    public int GetTropas() => tropas;
    public int GetTerritorioId() => territorioId;
}
