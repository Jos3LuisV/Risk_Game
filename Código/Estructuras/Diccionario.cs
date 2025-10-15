using System;

namespace RiskGame.Estructuras
{
    public class Diccionario<K, V>
    {
        // CLASE PÚBLICA
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
        private const double FACTOR_CARGA = 0.75;

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

        private int ObtenerIndiceBucket(K clave, int longitud)
        {
            int hashCode = clave != null ? clave.GetHashCode() : 0;
            return Math.Abs(hashCode) % longitud;
        }

        private void Redimensionar()
        {
            int nuevaCapacidad = buckets.Length * 2;
            var nuevosBuckets = new ListaEnlazada<ParClaveValor>[nuevaCapacidad];

            for (int i = 0; i < nuevaCapacidad; i++)
            {
                nuevosBuckets[i] = new ListaEnlazada<ParClaveValor>();
            }

            // Reinsertar todos los elementos
            for (int i = 0; i < buckets.Length; i++)
            {
                var lista = buckets[i];
                for (int j = 0; j < lista.Count; j++)
                {
                    var par = lista.Obtener(j);
                    int nuevoIndice = ObtenerIndiceBucket(par.Clave, nuevaCapacidad);
                    nuevosBuckets[nuevoIndice].Agregar(par);
                }
            }

            buckets = nuevosBuckets;
        }

        public void Agregar(K clave, V valor)
        {
            if (clave == null)
                throw new ArgumentException("La clave no puede ser nula");

            // Redimensionar si es necesario
            if ((double)count / buckets.Length > FACTOR_CARGA)
            {
                Redimensionar();
            }

            int indice = ObtenerIndiceBucket(clave, buckets.Length);
            var lista = buckets[indice];

            // Verificar si la clave ya existe
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

        public bool IntentarAgregar(K clave, V valor)
        {
            if (clave == null) return false;

            try
            {
                Agregar(clave, valor);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ContieneClave(K clave)
        {
            if (clave == null) return false;

            int indice = ObtenerIndiceBucket(clave, buckets.Length);
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

            int indice = ObtenerIndiceBucket(clave, buckets.Length);
            var lista = buckets[indice];

            for (int i = 0; i < lista.Count; i++)
            {
                var par = lista.Obtener(i);
                if (par.Clave.Equals(clave))
                    return par.Valor;
            }

            throw new Exception("La clave no existe en el diccionario");
        }

        public bool IntentarObtener(K clave, out V valor)
        {
            valor = default(V);

            if (clave == null) return false;

            int indice = ObtenerIndiceBucket(clave, buckets.Length);
            var lista = buckets[indice];

            for (int i = 0; i < lista.Count; i++)
            {
                var par = lista.Obtener(i);
                if (par.Clave.Equals(clave))
                {
                    valor = par.Valor;
                    return true;
                }
            }
            return false;
        }

        public bool Remover(K clave)
        {
            if (clave == null) return false;

            int indice = ObtenerIndiceBucket(clave, buckets.Length);
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

        public ListaEnlazada<K> ObtenerClaves()
        {
            var claves = new ListaEnlazada<K>();
            for (int i = 0; i < buckets.Length; i++)
            {
                var lista = buckets[i];
                for (int j = 0; j < lista.Count; j++)
                {
                    claves.Agregar(lista.Obtener(j).Clave);
                }
            }
            return claves;
        }

        public ListaEnlazada<V> ObtenerValores()
        {
            var valores = new ListaEnlazada<V>();
            for (int i = 0; i < buckets.Length; i++)
            {
                var lista = buckets[i];
                for (int j = 0; j < lista.Count; j++)
                {
                    valores.Agregar(lista.Obtener(j).Valor);
                }
            }
            return valores;
        }

        public ListaEnlazada<ParClaveValor> ObtenerPares()
        {
            var pares = new ListaEnlazada<ParClaveValor>();
            for (int i = 0; i < buckets.Length; i++)
            {
                var lista = buckets[i];
                for (int j = 0; j < lista.Count; j++)
                {
                    pares.Agregar(lista.Obtener(j));
                }
            }
            return pares;
        }

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
}
