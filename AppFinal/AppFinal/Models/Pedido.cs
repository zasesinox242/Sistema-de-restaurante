using AppFinal.Models.Enums;

namespace AppFinal.Models
{
    public class Pedido
    {
        public int codigoPedido { get; set; }
        public EstadoPedido estado { get; set; }
        public string comentarios { get; set; }
        public Mesa mesa { get; set; }
        public Empleado empleado { get; set; }
        public DateTime? fechaCreacion { get; set; }
        public string usuarioCreacion { get; set; }
        public DateTime? fechaUltimaModificacion { get; set; }
        public string usuarioUltimaModificacion { get; set; }
    }
}