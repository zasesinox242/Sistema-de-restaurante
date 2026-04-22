using System.IdentityModel.Tokens.Jwt; // Sirve para leer el token (JWT)
using System.Net.Http.Json; // Permite enviar datos en formato JSON
using AppFinal.Models;
using Blazored.LocalStorage; // Permite guardar datos en el navegador

namespace AppFinal.Services
{
    // Servicio encargado de todo lo relacionado al login y autenticación
    public class AuthService
    {
        private readonly HttpClient _http; // Para hacer peticiones al backend
        private readonly ILocalStorageService _localStorage; // Para guardar datos en el navegador
        private readonly CustomAuthStateProvider _authStateProvider; // Para avisar a la app si el usuario inició o cerró sesión

        // Constructor: recibe las dependencias necesarias
        public AuthService(
            HttpClient http,
            ILocalStorageService localStorage,
            CustomAuthStateProvider authStateProvider)
        {
            _http = http;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
        }

        // Método para iniciar sesión
        public async Task<string?> Login(string username, string password)
        {
            // Crea el objeto con los datos del login
            var login = new LoginRequest
            {
                username = username,
                password = password,
                rememberMe = true
            };

            // Envía los datos al backend
            var response = await _http.PostAsJsonAsync("api/authenticate", login);

            // Lee la respuesta como texto (por si hay error)
            var contenido = await response.Content.ReadAsStringAsync();

            // Si el backend responde con error
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"ERROR BACKEND: {contenido}");
            }

            // Convierte la respuesta a objeto
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

            // Si no vino token, algo falló
            if (result == null || string.IsNullOrWhiteSpace(result.id_token))
            {
                throw new Exception("No vino token. Contenido: " + contenido);
            }

            // Guarda el token en el navegador
            await _localStorage.SetItemAsync("authToken", result.id_token);

            // Notifica a la aplicación que el usuario inició sesión
            _authStateProvider.NotifyUserLogin(result.id_token);

            // Devuelve el token
            return result.id_token;
        }

        // Método para cerrar sesión
        public async Task Logout()
        {
            // Elimina el token del navegador
            await _localStorage.RemoveItemAsync("authToken");

            // Notifica a la app que el usuario cerró sesión
            _authStateProvider.NotifyUserLogout();
        }

        // Obtiene el token guardado
        public async Task<string?> ObtenerToken()
        {
            try
            {
                return await _localStorage.GetItemAsync<string>("authToken");
            }
            catch (InvalidOperationException)
            {
                return null; // Si ocurre error, devuelve null
            }
        }

        // Obtiene el rol del usuario desde el token
        public async Task<string?> ObtenerRol()
        {
            try
            {
                // Obtiene el token guardado
                var token = await _localStorage.GetItemAsync<string>("authToken");

                if (string.IsNullOrWhiteSpace(token))
                    return null;

                // Lee el token
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                // Busca el rol dentro del token
                var claimRol = jwt.Claims.FirstOrDefault(c =>
                    c.Type == "auth" ||
                    c.Type == "role" ||
                    c.Type == "roles" ||
                    c.Type == "authorities");

                var rol = claimRol?.Value;

                if (string.IsNullOrWhiteSpace(rol))
                    return null;

                // Limpia el texto del rol (quita símbolos innecesarios)
                return rol
                    .Replace("[", "")
                    .Replace("]", "")
                    .Replace("\"", "")
                    .Replace("ROLE_", "")
                    .Trim();
            }
            catch
            {
                return null; // Si falla algo, devuelve null
            }
        }

        // Obtiene el nombre del usuario desde el token
        public async Task<string?> ObtenerUsuario()
        {
            try
            {
                // Obtiene el token
                var token = await _localStorage.GetItemAsync<string>("authToken");

                if (string.IsNullOrWhiteSpace(token))
                    return null;

                // Lee el token
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                // Busca el usuario (campo "sub")
                return jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}