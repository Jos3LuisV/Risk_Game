using RiskGame.Estructuras;

public static class NombresContinentes
{
    private static Diccionario<int, string> nombresContinentes = new Diccionario<int, string>();

    static NombresContinentes()
    {
        InicializarNombres();
    }

    private static void InicializarNombres()
    {
        UnityEngine.Debug.Log("Inicializando nombres internos de continentes...");

        // América del Norte: elementos 0-8
        for (int i = 0; i <= 8; i++)
        {
            nombresContinentes.Agregar(i, "America Del Norte");
        }

        // Oceanía: elemento 9
        nombresContinentes.Agregar(9, "Oceania");

        // América del Sur: elementos 10-14
        for (int i = 10; i <= 14; i++)
        {
            nombresContinentes.Agregar(i, "America Del Sur");
        }

        // África: elementos 15-20
        for (int i = 15; i <= 20; i++)
        {
            nombresContinentes.Agregar(i, "Africa");
        }

        // Europa: elementos 24-30
        for (int i = 24; i <= 30; i++)
        {
            nombresContinentes.Agregar(i, "Europa");
        }

        // Asia: elementos 21-23, 31-41
        for (int i = 21; i <= 23; i++)
        {
            nombresContinentes.Agregar(i, "Asia");
        }
        for (int i = 31; i <= 41; i++)
        {
            nombresContinentes.Agregar(i, "Asia");
        }

        UnityEngine.Debug.Log("Nombres internos de continentes inicializados");
    }

    public static string ObtenerNombreContinente(int territorioId)
    {
        if (nombresContinentes.ContieneClave(territorioId))
        {
            return nombresContinentes.Obtener(territorioId);
        }
        return "Desconocido";
    }

    public static bool TerritorioPerteneceAContinente(int territorioId, string nombreContinente)
    {
        if (nombresContinentes.ContieneClave(territorioId))
        {
            return nombresContinentes.Obtener(territorioId) == nombreContinente;
        }
        return false;
    }

    public static string[] ObtenerTodosLosContinentes()
    {
        return new string[] {
            "America Del Norte",
            "America Del Sur",
            "Europa",
            "Africa",
            "Asia",
            "Oceania"
        };
    }

    public static int[] ObtenerTerritoriosDeContinente(string nombreContinente)
    {
        switch (nombreContinente)
        {
            case "America Del Norte":
                return new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
            case "Oceania":
                return new int[] { 9 };
            case "America Del Sur":
                return new int[] { 10, 11, 12, 13, 14 };
            case "Africa":
                return new int[] { 15, 16, 17, 18, 19, 20 };
            case "Europa":
                return new int[] { 24, 25, 26, 27, 28, 29, 30 };
            case "Asia":
                return new int[] { 21, 22, 23, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41 };
            default:
                return new int[0];
        }
    }
}
