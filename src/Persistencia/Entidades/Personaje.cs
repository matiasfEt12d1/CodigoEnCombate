using System;
using System.Collections.Generic;

namespace Persistencia.Entidades
{
    public abstract class Personaje
    {
        public int Id { get; set; }
        private string _nombre = string.Empty;
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

        // Todo lo que puede perder antes de ser derrotado. Sirve para medir el daño recibido
        // aunque una parte la absorba otra cosa que no sea la vida (por ejemplo, el escudo del Guerrero).
        public virtual int VidaEfectiva => Vida;

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

        // Cada tipo usa su propia habilidad (IHabilidad): el turno no pregunta por la clase concreta.
        public abstract void UsarHabilidad(Personaje objetivo);

        // Si tiene los recursos para usar su habilidad ahora (maná, flechas, etc.).
        public abstract bool PuedeUsarHabilidad { get; }

        // Pasos comunes a todas las habilidades: validar, revisar recursos y aplicar.
        protected void EjecutarHabilidad<TLanzador>(IHabilidad<TLanzador> habilidad, TLanzador lanzador, Personaje objetivo)
            where TLanzador : Personaje
        {
            if (habilidad == null) throw new ArgumentNullException(nameof(habilidad));
            ValidarPuedeActuar(objetivo);
            if (!habilidad.PuedeUsarse(lanzador))
                throw new InvalidOperationException($"{Nombre} no tiene recursos suficientes para usar {habilidad.Nombre}.");

            habilidad.Aplicar(lanzador, objetivo);
        }

        // Deja al personaje listo para una nueva batalla.
        public virtual void Restaurar()
        {
            Vida = VidaMax;
        }

        protected void ValidarPuedeActuar(Personaje objetivo)
        {
            if (objetivo == null) throw new ArgumentNullException(nameof(objetivo));
            if (!EstaVivo) throw new InvalidOperationException("Un personaje derrotado no puede actuar.");
        }

        // Sobrecarga de operadores: dos referencias son el mismo personaje si son el mismo objeto
        // o si los dos ya están guardados con el mismo Id.
        public static bool operator ==(Personaje? izquierdo, Personaje? derecho)
        {
            if (ReferenceEquals(izquierdo, derecho)) return true;
            if (izquierdo is null || derecho is null) return false;
            return izquierdo.Id > 0 && izquierdo.Id == derecho.Id;
        }

        public static bool operator !=(Personaje? izquierdo, Personaje? derecho) => !(izquierdo == derecho);

        public override bool Equals(object? obj) => obj is Personaje otro && this == otro;

        // Sin Id todavía, cada objeto es distinto: se usa el hash de la referencia.
        public override int GetHashCode() =>
            Id > 0 ? Id.GetHashCode() : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
    }
}