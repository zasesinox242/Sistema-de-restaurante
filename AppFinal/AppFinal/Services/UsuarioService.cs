using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AppFinal.Services
{
    // Modelo que representa un usuario de JHipster (jhi_user)
    public class JhiUser
    {
        public long id { get; set; }
        public string login { get; set; } = "";
        public string email { get; set; } = "";
        public string firstName { get; set; } = "";
        public string lastName { get; set; } = "";
        public string langKey { get; set; } = "es";
        public bool activated { get; set; } = true;
        public List<string> authorities { get; set; } = new();
        public string? createdBy { get; set; }
        public string? createdDate { get; set; }
        public string? lastModifiedBy { get; set; }
        public string? lastModifiedDate { get; set; }
    }

    // DTO para crear un nuevo usuario (incluye password)
    public class CreateUserDto
    {
        public string login { get; set; } = "";
        public string email { get; set; } = "";
        public string firstName { get; set; } = "";
        public string lastName { get; set; } = "";
        public string langKey { get; set; } = "es";
        public string password { get; set; } = "";
        public List<string> authorities { get; set; } = new();
    }

    public class UsuarioService
    {
        private readonly HttpClient _http;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(HttpClient http, ILogger<UsuarioService> logger)
        {
            _http = http;
            _logger = logger;
        }

        // Obtener todos los usuarios (requiere admin)
        public async Task<List<JhiUser>> GetAllAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<List<JhiUser>>("/api/users") ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                return new();
            }
        }

        // Obtener usuario por ID
        public async Task<JhiUser?> GetByIdAsync(long id)
        {
            try
            {
                return await _http.GetFromJsonAsync<JhiUser>($"/api/users/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener usuario {id}");
                return null;
            }
        }

        // Crear usuario (requiere admin)
        public async Task<JhiUser?> CreateAsync(CreateUserDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("/api/users", dto);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<JhiUser>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                return null;
            }
        }

        // Actualizar usuario (solo campos editables: firstName, lastName, email, authorities, langKey)
        public async Task<bool> UpdateAsync(JhiUser user)
        {
            try
            {
                // JHipster espera un objeto con los campos permitidos en /api/users
                var updateDto = new
                {
                    user.id,
                    user.login,
                    user.firstName,
                    user.lastName,
                    user.email,
                    user.langKey,
                    user.authorities
                };
                var response = await _http.PutAsJsonAsync($"/api/users", updateDto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario");
                return false;
            }
        }

        // Eliminar usuario (requiere admin)
        public async Task<bool> DeleteAsync(long id)
        {
            try
            {
                var response = await _http.DeleteAsync($"/api/users/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar usuario {id}");
                return false;
            }
        }
    }
}