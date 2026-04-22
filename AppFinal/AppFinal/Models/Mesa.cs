using AppFinal.Models.Enums;

namespace AppFinal.Models
{
    public class Mesa
    {
        public int id { get; set; }
        public int numero { get; set; }
        public int capacidad { get; set; }
        public EstadoMesa estado { get; set; }
        public DateTime? fechaCreacion { get; set; }
        public string usuarioCreacion { get; set; }
        public DateTime? fechaUltimaModificacion { get; set; }
        public string usuarioUltimaModificacion { get; set; }
    }
}