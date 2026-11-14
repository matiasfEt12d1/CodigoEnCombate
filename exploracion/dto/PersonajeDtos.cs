// Prueba de concepto de Indagación (Actividad 2): DTO de entrada y de salida.
// Un DTO solo transporta datos entre el cliente y el servicio: no tiene reglas ni comportamiento.

namespace Exploracion.Dto;

// Entrada: lo único que el cliente puede mandar para crear un personaje.
// No trae Id, vida ni ataque: esos los decide el servicio.
public record CrearPersonajeSolicitud(string? Nombre, string? Tipo);

// Salida: lo único que el cliente recibe del personaje creado.
public record PersonajeCreadoRespuesta(int Id, string Nombre);
