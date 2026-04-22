using AppFinal.Models;
using Blazored.LocalStorage;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppFinal.Services
{
    public class CartaService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;

        public CartaService(HttpClient http, ILocalStorageService localStorage)
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

        // GET all (con eagerload para incluir categoría)
        public async Task<List<Carta>> GetCartasAsync()
        {
            await SetAuthHeader();
            try
            {
                var response = await _http.GetAsync("/api/cartas?eagerload=true");
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[CartaService] JSON recibido: {json}"); // 👈 LOG
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new JsonStringEnumConverter() }
                    };
                    var cartas = JsonSerializer.Deserialize<List<Carta>>(json, options);
                    Console.WriteLine($"[CartaService] Obtenidas {cartas?.Count ?? 0} cartas");
                    if (cartas != null && cartas.Any())
                    {
                        var primera = cartas.First();
                        Console.WriteLine($"[CartaService] Primera carta: {primera.nombre}, Categoría: {primera.categoria?.nombre ?? "NULL"}");
                    }
                    return cartas ?? new();
                }
                else
                {
                    Console.WriteLine($"[CartaService] Error GET: {json}");
                    return new();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CartaService] Exception GET: {ex.Message}");
                return new();
            }
        }

        // CREATE (enviar codigo = 0 y categoria como objeto con codigoCategoria)
        public async Task<Carta?> CreateCartaAsync(Carta carta)
        {
            await SetAuthHeader();
            try
            {
                // DTO SIN la propiedad codigo (el backend lo genera automáticamente)
                var dto = new
                {
                    nombre = carta.nombre,
                    descripcion = carta.descripcion,
                    estado = carta.estado,
                    precio = carta.precio,
                    categoria = carta.categoria != null ? new { codigoCategoria = carta.categoria.codigoCategoria } : null
                };
                var json = JsonSerializer.Serialize(dto);
                Console.WriteLine($"[CartaService] Enviando POST: {json}");
                var response = await _http.PostAsJsonAsync("/api/cartas", dto);
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[CartaService] POST Status: {response.StatusCode}, Response: {content}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            Converters = { new JsonStringEnumConverter() }
                        };
                        var creado = JsonSerializer.Deserialize<Carta>(content, options);
                        if (creado != null)
                            return creado;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[CartaService] Error deserializando respuesta: {ex.Message}");
                    }
                    // Si no se pudo deserializar pero la respuesta fue exitosa, retornamos el objeto original
                    // para que la página sepa que fue exitoso y recargue.
                    return carta;
                }
                else
                {
                    Console.WriteLine($"[CartaService] Error POST: {content}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CartaService] Exception: {ex.Message}");
                return null;
            }
        }

        // UPDATE (enviar el objeto completo, incluyendo codigo y categoria)
        public async Task<bool> UpdateCartaAsync(int id, Carta carta)
        {
            await SetAuthHeader();
            try
            {
                carta.codigo = id; // asegurar
                var dto = new
                {
                    codigo = carta.codigo,
                    nombre = carta.nombre,
                    descripcion = carta.descripcion,
                    estado = carta.estado,
                    precio = carta.precio,
                    categoria = carta.categoria != null ? new { codigoCategoria = carta.categoria.codigoCategoria } : null
                };
                var response = await _http.PutAsJsonAsync($"/api/cartas/{id}", dto);
                Console.WriteLine($"[CartaService] PUT Status: {response.StatusCode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CartaService] Exception PUT: {ex.Message}");
                return false;
            }
        }

        // DELETE
        public async Task<bool> DeleteCartaAsync(int id)
        {
            await SetAuthHeader();
            try
            {
                var response = await _http.DeleteAsync($"/api/cartas/{id}");
                Console.WriteLine($"[CartaService] DELETE Status: {response.StatusCode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CartaService] Exception DELETE: {ex.Message}");
                return false;
            }
        }
    }
}