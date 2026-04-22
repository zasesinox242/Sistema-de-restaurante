using AppFinal.Models.Enums;

namespace AppFinal.Models
{
    public class Usuario
    {
        public int id { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public RolNombre rol { get; set; }
        public DateTime? fechaCreacion { get; set; }
        public string usuarioCreacion { get; set; }
        public DateTime? fechaUltimaModificacion { get; set; }
        public string usuarioUltimaModificacion { get; set; }
        public Empleado empleado { get; set; }
    }
}