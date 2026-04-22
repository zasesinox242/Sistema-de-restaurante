using System;

namespace AppFinal.Models
{
    public class Carta
    {
        public int? codigo { get; set; }  // nullable para creación (autogenerado)
        public string nombre { get; set; } = "";
        public string? descripcion { get; set; }
        public string estado { get; set; } = "Activo";
        public decimal precio { get; set; }
        // Relación con Categoria
        public Categoria? categoria { get; set; }
        // Auditoría
        public DateTime? fechaCreacion { get; set; }
        public string? usuarioCreacion { get; set; }
        public DateTime? fechaUltimaModificacion { get; set; }
        public string? usuarioUltimaModificacion { get; set; }
    }
}