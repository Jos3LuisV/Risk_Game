using System;

public class Diccionario<K, V>
{
    public class ParClaveValor
    {
        public K Clave { get; set; }
        public V Valor { get; set; }

        public ParClaveValor(K clave, V valor)
        {
            Clave = clave;
            Valor = valor;
        }
    }

    private ListaEnlazada<ParClaveValor>[] buckets;
    private int count;
    private const int CAPACIDAD_INICIAL = 16;

    public int Count => count;
    public bool EstaVacio => count == 0;

    public Diccionario()
    {
        buckets = new ListaEnlazada<ParClaveValor>[CAPACIDAD_INICIAL];
        for (int i = 0; i < CAPACIDAD_INICIAL; i++)
        {
            buckets[i] = new ListaEnlazada<ParClaveValor>();
        }
        count = 0;
    }

    private int ObtenerIndiceBucket(K clave)
    {
        int hashCode = clave != null ? clave.GetHashCode() : 0;
        return Math.Abs(hashCode) % buckets.Length;
    }

    public void Agregar(K clave, V valor)
    {
        if (clave == null)
            throw new ArgumentException("La clave no puede ser nula");

        int indice = ObtenerIndiceBucket(clave);
        var lista = buckets[indice];

        for (int i = 0; i < lista.Count; i++)
        {
            if (lista.Obtener(i).Clave.Equals(clave))
            {
                throw new Exception("La clave ya existe en el diccionario");
            }
        }

        lista.Agregar(new ParClaveValor(clave, valor));
        count++;
    }

    public bool ContieneClave(K clave)
    {
        if (clave == null) return false;

        int indice = ObtenerIndiceBucket(clave);
        var lista = buckets[indice];

        for (int i = 0; i < lista.Count; i++)
        {
            if (lista.Obtener(i).Clave.Equals(clave))
                return true;
        }
        return false;
    }

    public V Obtener(K clave)
    {
        if (clave == null)
            throw new ArgumentException("La clave no puede ser nula");

        int indice = ObtenerIndiceBucket(clave);
        var lista = buckets[indice];

        for (int i = 0; i < lista.Count; i++)
        {
            var par = lista.Obtener(i);
            if (par.Clave.Equals(clave))
                return par.Valor;
        }

        throw new Exception("La clave no existe en el diccionario");
    }

    public bool Remover(K clave)
    {
        if (clave == null) return false;

        int indice = ObtenerIndiceBucket(clave);
        var lista = buckets[indice];

        for (int i = 0; i < lista.Count; i++)
        {
            var par = lista.Obtener(i);
            if (par.Clave.Equals(clave))
            {
                lista.Remover(par);
                count--;
                return true;
            }
        }
        return false;
    }

    public void Limpiar()
    {
        for (int i = 0; i < buckets.Length; i++)
        {
            buckets[i].Limpiar();
        }
        count = 0;
    }

    public void Actualizar(K clave, V valor)
    {
        if (clave == null)
            throw new ArgumentException("La clave no puede ser nula");

        if (Remover(clave))
        {
            Agregar(clave, valor);
        }
        else
        {
            throw new Exception("La clave no existe en el diccionario");
        }
    }

    // ✅ INDEXADOR PARA USAR [ ] COMO Dictionary<>
    public V this[K clave]
    {
        get => Obtener(clave);
        set
        {
            if (ContieneClave(clave))
                Actualizar(clave, value);
            else
                Agregar(clave, value);
        }
    }
}
