using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AppFinal.Models;
using Blazored.LocalStorage;

namespace AppFinal.Services
{
    public class TurnoService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;

        public TurnoService(HttpClient http, ILocalStorageService localStorage)
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

        public async Task<List<Turno>> GetTurnosAsync()
        {
            await SetAuthHeader();
            try
            {
                var turnos = await _http.GetFromJsonAsync<List<Turno>>("/api/turnos");
                Console.WriteLine($"[TurnoService] Obtenidos {turnos?.Count ?? 0} turnos");
                return turnos ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TurnoService] Error GET: {ex.Message}");
                return new();
            }
        }
        public async Task<Turno?> CreateTurnoAsync(Turno turno)
        {
            await SetAuthHeader();
            try
            {
                var dto = new
                {
                    id = 0,
                    nombre = turno.nombre,
                    horaInicio = turno.horaInicio,
                    horaFin = turno.horaFin
                };
                var response = await _http.PostAsJsonAsync("/api/turnos", dto);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<Turno>();
                return null;
            }
            catch { return null; }
        }

        public async Task<bool> UpdateTurnoAsync(int id, Turno turno)
        {
            await SetAuthHeader();
            try
            {
                var response = await _http.PutAsJsonAsync($"/api/turnos/{id}", turno);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> DeleteTurnoAsync(int id)
        {
            await SetAuthHeader();
            try
            {
                var response = await _http.DeleteAsync($"/api/turnos/{id}");
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }
    }
}