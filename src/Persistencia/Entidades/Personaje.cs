using System;
using System.Collections.Generic;

namespace Persistencia.Entidades
{
    public abstract class Personaje
    {
        public int Id { get; set; }
        private string _nombre;
        private int _vidaMax;
        private int _vida;
        private int _ataque;
        private int _defensa;
        private readonly List<Habilidad> _habilidades = new();

        public string Nombre
        {
            get => _nombre;
            set => _nombre = string.IsNullOrWhiteSpace(value) 
                ? throw new ArgumentException("El nombre del personaje no puede estar vacío.") 
                : value;
        }

        public int VidaMax
        {
            get => _vidaMax;
            set => _vidaMax = value <= 0 
                ? throw new ArgumentException("La vida máxima debe ser mayor a cero.") 
                : value;
        }

        public int Vida
        {
            get => _vida;
            set
            {
                if (value < 0) _vida = 0;
                else if (value > VidaMax) _vida = VidaMax;
                else _vida = value;
            }
        }

        public int Ataque
        {
            get => _ataque;
            set => _ataque = value <= 0 
                ? throw new ArgumentException("El ataque debe ser mayor a cero.") 
                : value;
        }

        public int Defensa
        {
            get => _defensa;
            set => _defensa = value < 0 
                ? throw new ArgumentException("La defensa no puede ser negativa.") 
                : value;
        }

        public bool EstaVivo => Vida > 0;
        public IReadOnlyList<Habilidad> Habilidades => _habilidades.AsReadOnly();

        protected Personaje(string nombre, int vidaMax, int ataque, int defensa)
        {
            Nombre = nombre;
            VidaMax = vidaMax;
            Vida = vidaMax;
            Ataque = ataque;
            Defensa = defensa;
        }

        public void AgregarHabilidad(Habilidad habilidad)
        {
            if (habilidad == null) throw new ArgumentNullException(nameof(habilidad));
            _habilidades.Add(habilidad);
        }

        public virtual void Atacar(Personaje objetivo)
        {
            if (objetivo == null) throw new ArgumentNullException(nameof(objetivo));
            if (!EstaVivo) throw new InvalidOperationException("Un personaje derrotado no puede atacar.");

            int danio = Math.Max(1, Ataque - objetivo.Defensa);
            objetivo.RecibirDanio(danio);
        }

        public virtual void RecibirDanio(int cantidad)
        {
            if (cantidad < 0) return;
            Vida -= cantidad;
        }
    }
}