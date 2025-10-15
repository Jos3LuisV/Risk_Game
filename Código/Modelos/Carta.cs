namespace RiskGame.Modelos
{
    public class Carta
    {
        public string Tipo { get; set; } // "Infanteria", "Caballeria", "Artilleria"
        public Territorio Territorio { get; set; }
        public bool Usada { get; set; }

        public Carta(string tipo, Territorio territorio)
        {
            Tipo = tipo;
            Territorio = territorio;
            Usada = false;
        }

        public bool EsValidaParaIntercambio()
        {
            return !Usada;
        }

        public void MarcarComoUsada()
        {
            Usada = true;
        }
    }
}
