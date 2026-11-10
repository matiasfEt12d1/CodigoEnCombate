namespace Aplicacion
{
    public enum TipoError
    {
        Ninguno,
        Validacion,
        NoEncontrado,
        Conflicto
    }

    // Resultado de una operación de servicio: el valor o el motivo por el que no se pudo hacer.
    // La API lo traduce a 200/201/204, 400, 404 o 409.
    public class Resultado
    {
        public TipoError Error { get; }
        public string? Mensaje { get; }
        public bool Exito => Error == TipoError.Ninguno;

        protected Resultado(TipoError error, string? mensaje)
        {
            Error = error;
            Mensaje = mensaje;
        }

        public static Resultado Ok() => new(TipoError.Ninguno, null);
        public static Resultado Invalido(string mensaje) => new(TipoError.Validacion, mensaje);
        public static Resultado NoEncontrado(string mensaje) => new(TipoError.NoEncontrado, mensaje);
        public static Resultado Conflicto(string mensaje) => new(TipoError.Conflicto, mensaje);
    }

    public class Resultado<T> : Resultado
    {
        public T? Valor { get; }

        private Resultado(T? valor, TipoError error, string? mensaje) : base(error, mensaje)
        {
            Valor = valor;
        }

        public static Resultado<T> Ok(T valor) => new(valor, TipoError.Ninguno, null);
        public static new Resultado<T> Invalido(string mensaje) => new(default, TipoError.Validacion, mensaje);
        public static new Resultado<T> NoEncontrado(string mensaje) => new(default, TipoError.NoEncontrado, mensaje);
        public static new Resultado<T> Conflicto(string mensaje) => new(default, TipoError.Conflicto, mensaje);
    }
}
