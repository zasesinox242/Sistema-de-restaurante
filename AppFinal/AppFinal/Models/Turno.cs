namespace AppFinal.Models
{
    public class Turno
    {
        public int? id { get; set; }
        public string nombre { get; set; } = "";
        public string horaInicio { get; set; } = "";   // ← string
        public string horaFin { get; set; } = "";       // ← string
        public DateTime? fechaCreacion { get; set; }
        public string? usuarioCreacion { get; set; }
        public DateTime? fechaUltimaModificacion { get; set; }
        public string? usuarioUltimaModificacion { get; set; }
    }
}