using System;

public class Cola<T>
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

    private Nodo frente;
    private Nodo final;
    private int count;

    public int Count => count;
    public bool EstaVacia => frente == null;

    public Cola()
    {
        frente = null;
        final = null;
        count = 0;
    }

    public void Enqueue(T elemento)
    {
        Nodo nuevoNodo = new Nodo(elemento);

        if (EstaVacia)
        {
            frente = nuevoNodo;
            final = nuevoNodo;
        }
        else
        {
            final.Siguiente = nuevoNodo;
            final = nuevoNodo;
        }
        count++;
    }

    public T Dequeue()
    {
        if (EstaVacia)
            throw new InvalidOperationException("La cola está vacía");

        T datos = frente.Datos;
        frente = frente.Siguiente;

        if (frente == null)
            final = null;

        count--;
        return datos;
    }

    public T Peek()
    {
        if (EstaVacia)
            throw new InvalidOperationException("La cola está vacía");

        return frente.Datos;
    }

    public void Limpiar()
    {
        frente = null;
        final = null;
        count = 0;
    }

    public bool Contiene(T elemento)
    {
        Nodo actual = frente;
        while (actual != null)
        {
            if (actual.Datos.Equals(elemento))
                return true;
            actual = actual.Siguiente;
        }
        return false;
    }
}