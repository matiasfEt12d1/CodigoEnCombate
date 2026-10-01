using System;

namespace Persistencia.Entidades
{
    public class Habilidad
    {
        private string _nombre;
        private int _costoRecurso;
        private int _potencia;

        public string Nombre
        {
            get => _nombre;
            set => _nombre = string.IsNullOrWhiteSpace(value) 
                ? throw new ArgumentException("El nombre de la habilidad no puede estar vacío.") 
                : value;
        }

        public int CostoRecurso
        {
            get => _costoRecurso;
            set => _costoRecurso = value < 0 
                ? throw new ArgumentException("El costo de recurso no puede ser negativo.") 
                : value;
        }

        public int Potencia
        {
            get => _potencia;
            set => _potencia = value <= 0 
                ? throw new ArgumentException("La potencia de la habilidad debe ser mayor a cero.") 
                : value;
        }

        public Habilidad(string nombre, int costoRecurso, int potencia)
        {
            Nombre = nombre;
            CostoRecurso = costoRecurso;
            Potencia = potencia;
        }
    }
}