using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AppFinal.Models;

namespace AppFinal.Services
{
    public class PedidoMozoApiService
    {
        private readonly HttpClient _http;
        private readonly AuthService _authService;

        private const string EstadoNuevoPedido = "EN_ESPERA";

        public PedidoMozoApiService(HttpClient http, AuthService authService)
        {
            _http = http;
            _authService = authService;
        }

        private async Task ConfigurarTokenAsync()
        {
            var token = await _authService.ObtenerToken();

            _http.DefaultRequestHeaders.Authorization = null;

            if (!string.IsNullOrWhiteSpace(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<MesaMozoDto>> ObtenerMesasAsync()
        {
            await ConfigurarTokenAsync();

            var mesas = await _http.GetFromJsonAsync<List<MesaApi>>("api/mesas") ?? new List<MesaApi>();

            return mesas
                .OrderBy(m => m.numero ?? int.MaxValue)
                .Select(m => new MesaMozoDto
                {
                    Id = (int)(m.id ?? 0),
                    Nombre = m.numero.HasValue ? $"Mesa {m.numero}" : $"Mesa {m.id}"
                })
                .ToList();
        }

        public async Task<List<CartaMozoDto>> ObtenerCartaAsync()
        {
            await ConfigurarTokenAsync();

            var cartas = await _http.GetFromJsonAsync<List<CartaApi>>("api/cartas?eagerload=true") ?? new List<CartaApi>();

            return cartas
                .Where(c => !string.Equals(c.estado, "INACTIVO", StringComparison.OrdinalIgnoreCase))
                .Select(c => new CartaMozoDto
                {
                    Id = c.codigo ?? 0,
                    Nombre = c.nombre ?? "Sin nombre",
                    Precio = c.precio ?? 0,
                    Activo = !string.Equals(c.estado, "INACTIVO", StringComparison.OrdinalIgnoreCase)
                })
                .ToList();
        }

        // =========================
        // MÉTODOS PARA MOZO
        // =========================

        public async Task<List<PedidoMozoDto>> ObtenerPedidosAsync()
        {
            try
            {
                await ConfigurarTokenAsync();

                var usuarioActual = await _authService.ObtenerUsuario();

                var pedidos = await _http.GetFromJsonAsync<List<PedidoApi>>("api/pedidos?eagerload=true") ?? new List<PedidoApi>();
                var detalles = await _http.GetFromJsonAsync<List<DetallePedidoApi>>("api/detalle-pedidos?eagerload=true") ?? new List<DetallePedidoApi>();

                var pedidosFiltrados = pedidos;

                if (!string.IsNullOrWhiteSpace(usuarioActual))
                {
                    pedidosFiltrados = pedidos
                        .Where(p => string.Equals(p.usuarioCreacion, usuarioActual, StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(p => p.codigoPedido ?? 0)
                        .ToList();
                }
                else
                {
                    pedidosFiltrados = pedidos
                        .OrderByDescending(p => p.codigoPedido ?? 0)
                        .ToList();
                }

                return pedidosFiltrados
                    .Select(p => MapearPedido(p, detalles))
                    .ToList();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return new List<PedidoMozoDto>();
            }
        }

        public async Task<PedidoMozoDto?> ObtenerPedidoPorIdAsync(int id)
        {
            try
            {
                await ConfigurarTokenAsync();

                var usuarioActual = await _authService.ObtenerUsuario();

                var pedido = await _http.GetFromJsonAsync<PedidoApi>($"api/pedidos/{id}");
                if (pedido == null)
                    return null;

                if (!string.IsNullOrWhiteSpace(usuarioActual) &&
                    !string.Equals(pedido.usuarioCreacion, usuarioActual, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                var detalles = await _http.GetFromJsonAsync<List<DetallePedidoApi>>("api/detalle-pedidos?eagerload=true") ?? new List<DetallePedidoApi>();

                return MapearPedido(pedido, detalles);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return null;
            }
        }

        // =========================
        // MÉTODOS PARA CHEF
        // =========================

        public async Task<List<PedidoMozoDto>> ObtenerPedidosChefAsync()
        {
            try
            {
                await ConfigurarTokenAsync();

                var pedidos = await _http.GetFromJsonAsync<List<PedidoApi>>("api/pedidos?eagerload=true") ?? new List<PedidoApi>();
                var detalles = await _http.GetFromJsonAsync<List<DetallePedidoApi>>("api/detalle-pedidos?eagerload=true") ?? new List<DetallePedidoApi>();

                return pedidos
                    .Where(p =>
                        p.estado != null &&
                        !string.Equals(p.estado, "ENTREGADO", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(p.estado, "CANCELADO", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(p => p.codigoPedido ?? 0)
                    .Select(p => MapearPedido(p, detalles))
                    .ToList();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return new List<PedidoMozoDto>();
            }
        }

        public async Task<PedidoMozoDto?> ObtenerPedidoChefPorIdAsync(int id)
        {
            try
            {
                await ConfigurarTokenAsync();

                var pedido = await _http.GetFromJsonAsync<PedidoApi>($"api/pedidos/{id}");
                if (pedido == null)
                    return null;

                var detalles = await _http.GetFromJsonAsync<List<DetallePedidoApi>>("api/detalle-pedidos?eagerload=true") ?? new List<DetallePedidoApi>();

                return MapearPedido(pedido, detalles);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return null;
            }
        }

        public async Task<int> CrearPedidoAsync(CrearPedidoMozoDto dto)
        {
            await ConfigurarTokenAsync();

            var bodyPedido = new PedidoCreateRequest
            {
                estado = EstadoNuevoPedido,
                comentarios = string.IsNullOrWhiteSpace(dto.Comentarios) ? null : dto.Comentarios,
                mesa = dto.MesaId > 0 ? new MesaRefRequest { id = dto.MesaId } : null
            };

            var responsePedido = await _http.PostAsJsonAsync("api/pedidos", bodyPedido);
            var errorPedido = await responsePedido.Content.ReadAsStringAsync();

            if (!responsePedido.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"No se pudo crear el pedido. Respuesta backend: {errorPedido}");
            }

            var pedidoCreado = await responsePedido.Content.ReadFromJsonAsync<PedidoApi>();

            if (pedidoCreado?.codigoPedido == null || pedidoCreado.codigoPedido == 0)
            {
                throw new InvalidOperationException("El backend no devolvió el código del pedido creado.");
            }

            var pedidoId = pedidoCreado.codigoPedido.Value;

            foreach (var item in dto.Items)
            {
                var carta = await ObtenerCartaApiPorIdAsync(item.ItemId);

                if (carta == null || carta.codigo == null)
                {
                    throw new InvalidOperationException($"No se encontró la carta con código {item.ItemId}.");
                }

                var precio = carta.precio ?? 0;
                var subtotal = precio * item.Cantidad;

                var bodyDetalle = new DetallePedidoCreateRequest
                {
                    cantidad = item.Cantidad,
                    subtotal = subtotal,
                    observacion = null,
                    total = subtotal,
                    pedido = new PedidoRefRequest { codigoPedido = pedidoId },
                    carta = new CartaRefRequest { codigo = carta.codigo.Value }
                };

                var responseDetalle = await _http.PostAsJsonAsync("api/detalle-pedidos", bodyDetalle);
                var errorDetalle = await responseDetalle.Content.ReadAsStringAsync();

                if (!responseDetalle.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"No se pudo crear el detalle del pedido. Respuesta backend: {errorDetalle}");
                }
            }

            return pedidoId;
        }

        public async Task<bool> ActualizarEstadoPedidoAsync(int id, string nuevoEstado)
        {
            try
            {
                await ConfigurarTokenAsync();

                var pedido = await _http.GetFromJsonAsync<PedidoApi>($"api/pedidos/{id}");
                if (pedido == null)
                    return false;

                pedido.estado = FormatearEstadoBackend(nuevoEstado);

                var response = await _http.PutAsJsonAsync($"api/pedidos/{id}", pedido);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private string FormatearEstadoBackend(string estado)
        {
            return estado.ToUpper().Replace(" ", "_");
        }

        private async Task<CartaApi?> ObtenerCartaApiPorIdAsync(int id)
        {
            await ConfigurarTokenAsync();
            return await _http.GetFromJsonAsync<CartaApi>($"api/cartas/{id}");
        }

        private PedidoMozoDto MapearPedido(PedidoApi pedido, List<DetallePedidoApi> detalles)
        {
            var detallesPedido = detalles
                .Where(d => d.pedido?.codigoPedido == pedido.codigoPedido)
                .Select(d => new PedidoItemMozoDto
                {
                    ItemId = d.carta?.codigo ?? 0,
                    Nombre = d.carta?.nombre ?? "Item sin nombre",
                    Cantidad = d.cantidad ?? 0,
                    PrecioUnitario = d.cantidad.HasValue && d.cantidad.Value > 0
                        ? ((d.total ?? d.subtotal ?? 0) / d.cantidad.Value)
                        : 0
                })
                .ToList();

            return new PedidoMozoDto
            {
                Id = pedido.codigoPedido ?? 0,
                MesaId = (int)(pedido.mesa?.id ?? 0),
                MesaNombre = pedido.mesa?.numero != null ? $"Mesa {pedido.mesa.numero}" : "Sin mesa",
                Fecha = pedido.fechaCreacion?.LocalDateTime ?? DateTime.Now,
                Estado = FormatearEstado(pedido.estado),
                Comentarios = string.IsNullOrWhiteSpace(pedido.comentarios) ? "Sin comentarios" : pedido.comentarios,
                Total = detallesPedido.Sum(x => x.SubTotal),
                MozoNombre = pedido.usuarioCreacion ?? "Usuario",
                Items = detallesPedido
            };
        }

        private string FormatearEstado(string? estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
                return "Sin estado";

            var texto = estado.Replace("_", " ").ToLowerInvariant();
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(texto);
        }

        private class PedidoApi
        {
            public int? codigoPedido { get; set; }
            public string? estado { get; set; }
            public string? comentarios { get; set; }
            public string? usuarioCreacion { get; set; }
            public DateTimeOffset? fechaCreacion { get; set; }
            public MesaApi? mesa { get; set; }
        }

        private class DetallePedidoApi
        {
            public long? id { get; set; }
            public int? cantidad { get; set; }
            public decimal? subtotal { get; set; }
            public string? observacion { get; set; }
            public decimal? total { get; set; }
            public PedidoRefResponse? pedido { get; set; }
            public CartaApi? carta { get; set; }
        }

        private class MesaApi
        {
            public long? id { get; set; }
            public int? numero { get; set; }
            public int? capacidad { get; set; }
            public string? estado { get; set; }
        }

        private class CartaApi
        {
            public int? codigo { get; set; }
            public string? nombre { get; set; }
            public string? descripcion { get; set; }
            public string? estado { get; set; }
            public decimal? precio { get; set; }
        }

        private class PedidoRefResponse
        {
            public int? codigoPedido { get; set; }
        }

        private class PedidoCreateRequest
        {
            public string? estado { get; set; }
            public string? comentarios { get; set; }
            public MesaRefRequest? mesa { get; set; }
        }

        private class DetallePedidoCreateRequest
        {
            public int cantidad { get; set; }
            public decimal subtotal { get; set; }
            public string? observacion { get; set; }
            public decimal total { get; set; }
            public PedidoRefRequest? pedido { get; set; }
            public CartaRefRequest? carta { get; set; }
        }

        private class MesaRefRequest
        {
            public long id { get; set; }
        }

        private class PedidoRefRequest
        {
            public int codigoPedido { get; set; }
        }

        private class CartaRefRequest
        {
            public int codigo { get; set; }
        }
    }
}