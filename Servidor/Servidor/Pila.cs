using System;

public class Pila<T>
{
    private class Nodo
    {
        public T Datos { get; set; }
        public Nodo Siguiente { get; set; }

        public Nodo(T datos)
        {
            Datos = datos;
            Siguiente = null;
        }
    }

    private Nodo tope;
    private int count;

    public int Count => count;
    public bool EstaVacia => tope == null;

    public Pila()
    {
        tope = null;
        count = 0;
    }

    public void Push(T elemento)
    {
        Nodo nuevoNodo = new Nodo(elemento);
        nuevoNodo.Siguiente = tope;
        tope = nuevoNodo;
        count++;
    }

    public T Pop()
    {
        if (EstaVacia)
            throw new InvalidOperationException("La pila está vacía");

        T datos = tope.Datos;
        tope = tope.Siguiente;
        count--;
        return datos;
    }

    public T Peek()
    {
        if (EstaVacia)
            throw new InvalidOperationException("La pila está vacía");

        return tope.Datos;
    }

    public void Limpiar()
    {
        tope = null;
        count = 0;
    }

    public bool Contiene(T elemento)
    {
        Nodo actual = tope;
        while (actual != null)
        {
            if (actual.Datos.Equals(elemento))
                return true;
            actual = actual.Siguiente;
        }
        return false;
    }
}
