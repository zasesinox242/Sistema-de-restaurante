using System.IdentityModel.Tokens.Jwt; // Sirve para leer el token (JWT)
using System.Security.Claims; // Permite trabajar con datos del usuario (claims)
using Blazored.LocalStorage; // Permite guardar datos en el navegador
using Microsoft.AspNetCore.Components.Authorization; // Maneja autenticación en Blazor

namespace AppFinal.Services
{
    // Clase que controla si el usuario está logueado o no
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _storage; // Acceso al almacenamiento del navegador

        // Usuario actual guardado en memoria (cache)
        private ClaimsPrincipal _cachedUser = new(new ClaimsIdentity());

        // Constructor
        public CustomAuthStateProvider(ILocalStorageService storage)
        {
            _storage = storage;
        }

        // Este método le dice a Blazor quién es el usuario actual
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Obtiene el token guardado
                var token = await _storage.GetItemAsync<string>("authToken");

                // Si no hay token → usuario no autenticado
                if (string.IsNullOrWhiteSpace(token))
                {
                    _cachedUser = new ClaimsPrincipal(new ClaimsIdentity()); // Usuario vacío
                    return new AuthenticationState(_cachedUser);
                }

                // Si hay token → construye el usuario con sus datos
                _cachedUser = ConstruirUsuarioDesdeToken(token);

                // Devuelve el estado autenticado
                return new AuthenticationState(_cachedUser);
            }
            catch
            {
                // Si ocurre error → usuario no autenticado
                _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
                return new AuthenticationState(_cachedUser);
            }
        }

        // Se llama cuando el usuario inicia sesión
        public void NotifyUserLogin(string token)
        {
            // Construye el usuario desde el token
            _cachedUser = ConstruirUsuarioDesdeToken(token);

            // Notifica a toda la app que el usuario cambió
            NotifyAuthenticationStateChanged(
                Task.FromResult(new AuthenticationState(_cachedUser)));
        }

        // Se llama cuando el usuario cierra sesión
        public void NotifyUserLogout()
        {
            // Limpia el usuario (lo deja vacío)
            _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());

            // Notifica a la app que ya no hay usuario logueado
            NotifyAuthenticationStateChanged(
                Task.FromResult(new AuthenticationState(_cachedUser)));
        }

        // Método que crea un usuario a partir del token
        private ClaimsPrincipal ConstruirUsuarioDesdeToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            // Lee el contenido del token
            var jwt = handler.ReadJwtToken(token);

            var claims = new List<Claim>(); // Lista donde se guardan los datos del usuario

            // Obtiene el nombre del usuario (campo "sub")
            var username = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            if (!string.IsNullOrWhiteSpace(username))
            {
                // Guarda el nombre como claim
                claims.Add(new Claim(ClaimTypes.Name, username));
            }

            // Obtiene los roles del usuario
            var roles = jwt.Claims
                .Where(c =>
                    c.Type == "auth" ||
                    c.Type == "role" ||
                    c.Type == "roles" ||
                    c.Type == "authorities")
                .Select(c => c.Value)
                .ToList();

            // Recorre cada rol encontrado
            foreach (var roleValue in roles)
            {
                // Limpia el texto (quita símbolos innecesarios)
                var rolesSeparados = roleValue
                    .Replace("[", " ")
                    .Replace("]", " ")
                    .Replace("\"", " ")
                    .Replace(",", " ")
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                // Recorre cada rol separado
                foreach (var rol in rolesSeparados)
                {
                    // Quita "ROLE_" del texto
                    var rolLimpio = rol.Replace("ROLE_", "").Trim();

                    if (!string.IsNullOrWhiteSpace(rolLimpio))
                    {
                        // Guarda el rol como claim
                        claims.Add(new Claim(ClaimTypes.Role, rolLimpio));
                    }
                }
            }

            // Crea la identidad del usuario con sus datos
            var identity = new ClaimsIdentity(claims, "jwt");

            // Devuelve el usuario completo
            return new ClaimsPrincipal(identity);
        }
    }
}