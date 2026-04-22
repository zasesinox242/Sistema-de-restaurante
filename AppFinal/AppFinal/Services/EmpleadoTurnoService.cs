using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AppFinal.Models;
using Blazored.LocalStorage;

namespace AppFinal.Services
{
    public class EmpleadoTurnoService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }  // 👈 clave para el enum Jornada
        };

        public EmpleadoTurnoService(HttpClient http, ILocalStorageService localStorage)
        {
            _http = http;
            _localStorage = localStorage;
        }

        private async Task SetAuthHeader()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<EmpleadoTurno>> GetEmpleadoTurnosAsync()
        {
            await SetAuthHeader();
            try
            {
                var response = await _http.GetAsync("/api/empleado-turnos?eagerload=true");
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[EmpleadoTurnoService] GET Status: {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                {
                    var lista = JsonSerializer.Deserialize<List<EmpleadoTurno>>(content, _jsonOptions) ?? new();
                    Console.WriteLine($"[EmpleadoTurnoService] Deserializadas {lista.Count} asignaciones");
                    return lista;
                }
                else
                {
                    Console.WriteLine($"[EmpleadoTurnoService] Error GET: {content}");
                    return new();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmpleadoTurnoService] Exception GET: {ex.Message}");
                return new();
            }
        }

        // Payload sin "id" cuando es creación, con "id" cuando es actualización
        private static object BuildPayload(int id, Empleado empleado, Turno turno, string fecha)
        {
            if (id == 0) // creación
            {
                return new
                {
                    fecha,
                    turno = new { id = turno.id },
                    empleado = new { codigoEmpleado = empleado.codigoEmpleado }
                };
            }
            else // actualización
            {
                return new
                {
                    id,
                    fecha,
                    turno = new { id = turno.id },
                    empleado = new { codigoEmpleado = empleado.codigoEmpleado }
                };
            }
        }

        public async Task<EmpleadoTurno?> CreateEmpleadoTurnoAsync(Empleado empleado, Turno turno, string fecha)
        {
            await SetAuthHeader();
            try
            {
                var payload = BuildPayload(0, empleado, turno, fecha);
                var json = JsonSerializer.Serialize(payload);
                Console.WriteLine($"[EmpleadoTurnoService] POST payload: {json}");
                var response = await _http.PostAsync("/api/empleado-turnos", new StringContent(json, Encoding.UTF8, "application/json"));
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[EmpleadoTurnoService] POST Status: {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                    return JsonSerializer.Deserialize<EmpleadoTurno>(responseBody, _jsonOptions);
                else
                    Console.WriteLine($"[EmpleadoTurnoService] POST Error: {responseBody}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmpleadoTurnoService] Exception POST: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateEmpleadoTurnoAsync(int id, Empleado empleado, Turno turno, string fecha)
        {
            await SetAuthHeader();
            try
            {
                var payload = BuildPayload(id, empleado, turno, fecha);
                var json = JsonSerializer.Serialize(payload);
                Console.WriteLine($"[EmpleadoTurnoService] PUT payload: {json}");
                var response = await _http.PutAsync($"/api/empleado-turnos/{id}", new StringContent(json, Encoding.UTF8, "application/json"));
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[EmpleadoTurnoService] PUT Status: {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                    return true;
                else
                    Console.WriteLine($"[EmpleadoTurnoService] PUT Error: {responseBody}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmpleadoTurnoService] Exception PUT: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteEmpleadoTurnoAsync(int id)
        {
            await SetAuthHeader();
            try
            {
                var response = await _http.DeleteAsync($"/api/empleado-turnos/{id}");
                Console.WriteLine($"[EmpleadoTurnoService] DELETE Status: {response.StatusCode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmpleadoTurnoService] Exception DELETE: {ex.Message}");
                return false;
            }
        }
    }
}