namespace AppFinal.Models
{
    public class PedidoMozoDto
    {
        public int Id { get; set; }
        public int MesaId { get; set; }
        public string MesaNombre { get; set; } = "";
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = "En espera";
        public string Comentarios { get; set; } = "Sin comentarios";
        public decimal Total { get; set; }
        public string MozoNombre { get; set; } = "";
        public List<PedidoItemMozoDto> Items { get; set; } = new();
    }

    public class PedidoItemMozoDto
    {
        public int ItemId { get; set; }
        public string Nombre { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal SubTotal => Cantidad * PrecioUnitario;
    }

    public class MesaMozoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
    }

    public class CartaMozoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public decimal Precio { get; set; }
        public bool Activo { get; set; } = true;
    }

    public class CrearPedidoMozoDto
    {
        public int MesaId { get; set; }
        public string Comentarios { get; set; } = "";
        public List<CrearPedidoItemMozoDto> Items { get; set; } = new();
    }

    public class CrearPedidoItemMozoDto
    {
        public int ItemId { get; set; }
        public int Cantidad { get; set; }
    }
}