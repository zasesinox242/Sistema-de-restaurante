using System.Net.Http.Headers; // Permite enviar el token en las peticiones
using System.Net.Http.Json; // Permite enviar y recibir JSON fácilmente
using System.Text;
using System.Text.Json; // Sirve para convertir JSON a objetos
using AppFinal.Models;
using Blazored.LocalStorage; // Permite guardar y leer datos del navegador

namespace AppFinal.Services
{
    // Servicio que maneja todo lo relacionado a usuarios (crear, listar, editar, eliminar)
    public class JhiUserService
    {
        private readonly HttpClient _http; // Para hacer llamadas al backend
        private readonly ILocalStorageService _localStorage; // Para obtener el token guardado

        // Configuración para leer JSON sin importar mayúsculas/minúsculas
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        // Constructor
        public JhiUserService(HttpClient http, ILocalStorageService localStorage)
        {
            _http = http;
            _localStorage = localStorage;
        }

        // Método privado que agrega el token a cada petición
        private async Task SetAuthHeader()
        {
            // Obtiene el token del navegador
            var token = await _localStorage.GetItemAsync<string>("authToken");

            // Si existe, lo agrega como "Bearer"
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // Obtiene la lista de usuarios
        public async Task<List<JhiUser>> GetUsersAsync()
        {
            await SetAuthHeader(); // Agrega token

            try
            {
                // Llama al backend
                var response = await _http.GetAsync("/api/admin/users");

                // Si responde correctamente
                if (response.IsSuccessStatusCode)
                {
                    // Lee el contenido
                    var json = await response.Content.ReadAsStringAsync();

                    // Convierte JSON a lista de usuarios
                    return JsonSerializer.Deserialize<List<JhiUser>>(json, _jsonOptions) ?? new();
                }

                return new(); // Si falla, devuelve lista vacía
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error GetUsers: {ex.Message}");
                return new();
            }
        }

        // Crea un nuevo usuario
        public async Task<JhiUser?> CreateUserAsync(JhiUserCreateDto dto)
        {
            await SetAuthHeader();

            try
            {
                // Envía los datos al backend
                var response = await _http.PostAsJsonAsync("/api/admin/users-with-password", dto);

                var content = await response.Content.ReadAsStringAsync();

                // Muestra en consola para depuración
                Console.WriteLine($"CreateUser Status: {response.StatusCode}, Response: {content}");

                // Si fue exitoso, devuelve el usuario creado
                if (response.IsSuccessStatusCode)
                    return JsonSerializer.Deserialize<JhiUser>(content, _jsonOptions);

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateUser Exception: {ex.Message}");
                return null;
            }
        }

        // Actualiza un usuario existente
        public async Task<bool> UpdateUserAsync(JhiUser user)
        {
            await SetAuthHeader();

            try
            {
                // Envía los datos actualizados
                var response = await _http.PutAsJsonAsync("/api/admin/users", user);

                // Devuelve true si todo salió bien
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Cambia la contraseña de un usuario
        public async Task<bool> ChangePasswordAsync(string login, string newPassword)
        {
            await SetAuthHeader();

            try
            {
                // Crea objeto con los datos necesarios
                var dto = new ChangePasswordDto
                {
                    login = login,
                    newPassword = newPassword
                };

                // Envía al backend
                var response = await _http.PostAsJsonAsync("/api/admin/users/change-password", dto);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Elimina un usuario
        public async Task<bool> DeleteUserAsync(string login)
        {
            await SetAuthHeader();

            try
            {
                // Llama al backend con el login en la URL
                var response = await _http.DeleteAsync($"/api/admin/users/{login}");

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}