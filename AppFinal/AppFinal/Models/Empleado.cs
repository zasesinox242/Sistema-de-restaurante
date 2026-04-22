using AppFinal.Models.Enums;

public class Empleado
{
    public int? codigoEmpleado { get; set; }  // ← ahora nullable
    public string nombre { get; set; } = "";
    public string apellido { get; set; } = "";
    public string tipoDocumento { get; set; } = "";
    public string numeroDocumento { get; set; } = "";
    public string estado { get; set; } = "";
    public Jornada? jornada { get; set; }
    public DateTime? fechaCreacion { get; set; }
    public string? usuarioCreacion { get; set; }
    public DateTime? fechaUltimaModificacion { get; set; }
    public string? usuarioUltimaModificacion { get; set; }
    public long? usuarioId { get; set; }
}