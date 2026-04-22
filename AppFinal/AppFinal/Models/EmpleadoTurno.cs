using System;

namespace AppFinal.Models
{
    public class EmpleadoTurno
    {
        public int? id { get; set; }  // nullable para creación
        public DateTime? fecha { get; set; }
        public DateTime? fechaCreacion { get; set; }
        public string? usuarioCreacion { get; set; }
        public DateTime? fechaUltimaModificacion { get; set; }
        public string? usuarioUltimaModificacion { get; set; }
        // Relaciones
        public Turno? turno { get; set; }
        public Empleado? empleado { get; set; }
    }
}