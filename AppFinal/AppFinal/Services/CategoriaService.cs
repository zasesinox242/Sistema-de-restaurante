using System.Net.Http.Headers;
using System.Net.Http.Json;
using AppFinal.Models;
using Blazored.LocalStorage;

namespace AppFinal.Services
{
    public class CategoriaService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;

        public CategoriaService(HttpClient http, ILocalStorageService localStorage)
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

        public async Task<List<Categoria>> GetCategoriasAsync()
        {
            await SetAuthHeader();
            try
            {
                var categorias = await _http.GetFromJsonAsync<List<Categoria>>("/api/categorias");
                Console.WriteLine($"[CategoriaService] Obtenidas {categorias?.Count ?? 0} categorías");
                return categorias ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CategoriaService] Error GET: {ex.Message}");
                return new();
            }
        }
    }
}