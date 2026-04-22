namespace AppFinal.Models
{
    public class Categoria
    {
        public int codigoCategoria { get; set; }
        public string nombre { get; set; } = "";
        public string? area { get; set; }
        // Auditoría (opcional)
        public DateTime? fechaCreacion { get; set; }
        public string? usuarioCreacion { get; set; }
        public DateTime? fechaUltimaModificacion { get; set; }
        public string? usuarioUltimaModificacion { get; set; }
    }
}