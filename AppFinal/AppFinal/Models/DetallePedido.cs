namespace AppFinal.Models
{
    public class DetallePedido
    {
        public int id { get; set; }
        public int cantidad { get; set; }
        public decimal subtotal { get; set; }
        public string observacion { get; set; }
        public decimal total { get; set; }
        public Pedido pedido { get; set; }
        public Carta carta { get; set; }
        public DateTime? fechaCreacion { get; set; }
        public string usuarioCreacion { get; set; }
        public DateTime? fechaUltimaModificacion { get; set; }
        public string usuarioUltimaModificacion { get; set; }
    }
}