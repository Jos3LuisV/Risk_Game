using System;

namespace RiskGame.Estructuras
{
    public class ListaEnlazada<T>
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

        private Nodo cabeza;
        private Nodo cola;
        private int count;

        public int Count => count;
        public bool EstaVacia => cabeza == null;

        public ListaEnlazada()
        {
            cabeza = null;
            cola = null;
            count = 0;
        }

        public void Agregar(T elemento)
        {
            Nodo nuevoNodo = new Nodo(elemento);

            if (EstaVacia)
            {
                cabeza = nuevoNodo;
                cola = nuevoNodo;
            }
            else
            {
                cola.Siguiente = nuevoNodo;
                cola = nuevoNodo;
            }
            count++;
        }

        public void Insertar(int indice, T elemento)
        {
            if (indice < 0 || indice > count)
                throw new IndexOutOfRangeException();

            if (indice == count)
            {
                Agregar(elemento);
                return;
            }

            Nodo nuevoNodo = new Nodo(elemento);

            if (indice == 0)
            {
                nuevoNodo.Siguiente = cabeza;
                cabeza = nuevoNodo;
            }
            else
            {
                Nodo actual = cabeza;
                for (int i = 0; i < indice - 1; i++)
                {
                    actual = actual.Siguiente;
                }
                nuevoNodo.Siguiente = actual.Siguiente;
                actual.Siguiente = nuevoNodo;
            }
            count++;
        }

        public bool Remover(T elemento)
        {
            if (EstaVacia) return false;

            if (cabeza.Datos.Equals(elemento))
            {
                cabeza = cabeza.Siguiente;
                if (cabeza == null) cola = null;
                count--;
                return true;
            }

            Nodo actual = cabeza;
            while (actual.Siguiente != null && !actual.Siguiente.Datos.Equals(elemento))
            {
                actual = actual.Siguiente;
            }

            if (actual.Siguiente != null)
            {
                if (actual.Siguiente == cola)
                    cola = actual;

                actual.Siguiente = actual.Siguiente.Siguiente;
                count--;
                return true;
            }

            return false;
        }

        public T Obtener(int indice)
        {
            if (indice < 0 || indice >= count)
                throw new IndexOutOfRangeException();

            Nodo actual = cabeza;
            for (int i = 0; i < indice; i++)
            {
                actual = actual.Siguiente;
            }
            return actual.Datos;
        }

        public bool Contiene(T elemento)
        {
            Nodo actual = cabeza;
            while (actual != null)
            {
                if (actual.Datos.Equals(elemento))
                    return true;
                actual = actual.Siguiente;
            }
            return false;
        }

        public void Limpiar()
        {
            cabeza = null;
            cola = null;
            count = 0;
        }

        public T[] ToArray()
        {
            T[] array = new T[count];
            Nodo actual = cabeza;
            for (int i = 0; i < count; i++)
            {
                array[i] = actual.Datos;
                actual = actual.Siguiente;
            }
            return array;
        }
    }
}    
