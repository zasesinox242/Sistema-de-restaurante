using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AppFinal.Models;
using Blazored.LocalStorage;

namespace AppFinal.Services
{
    public class EmpleadoService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;

        public EmpleadoService(HttpClient http, ILocalStorageService localStorage)
        {
            _http = http;
            _localStorage = localStorage;
        }

        // ✅ Método para añadir el token a las cabeceras
        private async Task SetAuthHeader()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // 📋 Obtener empleados (con conversión de enum desde string)
        public async Task<List<Empleado>> GetEmpleadosAsync()
        {
            await SetAuthHeader();
            try
            {
                var response = await _http.GetAsync("/api/empleados");
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"JSON de empleados: {json}"); // 👈 ver si aparece usuarioId
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new JsonStringEnumConverter() }
                    };
                    var empleados = JsonSerializer.Deserialize<List<Empleado>>(json, options);
                    return empleados ?? new();
                }
                return new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error GET: {ex.Message}");
                return new();
            }
        }

        // ➕ Crear empleado (sin enviar codigoEmpleado, jornada como string)
        public async Task<Empleado?> CreateEmpleadoAsync(Empleado empleado)
        {
            await SetAuthHeader();
            try
            {
                var dto = new
                {
                    nombre = empleado.nombre,
                    apellido = empleado.apellido,
                    tipoDocumento = empleado.tipoDocumento,
                    numeroDocumento = empleado.numeroDocumento,
                    estado = empleado.estado,
                    jornada = empleado.jornada?.ToString()
                };
                var response = await _http.PostAsJsonAsync("/api/empleados", dto);
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[POST] Status: {response.StatusCode}, Response: {content}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            Converters = { new JsonStringEnumConverter() }
                        };
                        var creado = JsonSerializer.Deserialize<Empleado>(content, options);
                        if (creado != null)
                            return creado;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deserializando respuesta: {ex.Message}");
                    }
                    empleado.codigoEmpleado = null;
                    return empleado;
                }
                else
                {
                    Console.WriteLine($"Error POST: {content}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return null;
            }
        }

        // ✏️ Actualizar empleado
        public async Task<bool> UpdateEmpleadoAsync(int id, Empleado empleado)
        {
            await SetAuthHeader();
            try
            {
                var response = await _http.PutAsJsonAsync($"/api/empleados/{id}", empleado);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        // 🗑️ Eliminar empleado
        public async Task<bool> DeleteEmpleadoAsync(int id)
        {
            await SetAuthHeader();
            try
            {
                var response = await _http.DeleteAsync($"/api/empleados/{id}");
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        // 🔗 Vincular/desvincular empleado con usuario (actualizar usuarioId)
        public async Task<bool> UpdateEmpleadoUsuarioIdAsync(int empleadoId, long? usuarioId)
        {
            await SetAuthHeader();
            try
            {
                var empleados = await GetEmpleadosAsync();
                var empleado = empleados.FirstOrDefault(e => e.codigoEmpleado == empleadoId);
                if (empleado == null) return false;
                empleado.usuarioId = usuarioId;
                return await UpdateEmpleadoAsync(empleadoId, empleado);
            }
            catch { return false; }
        }
    }
}