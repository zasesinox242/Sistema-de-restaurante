using AppFinal.Components;
using AppFinal.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazoredLocalStorage();

// Autorización en Blazor
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

// HttpClient para JHipster
builder.Services.AddScoped(sp =>
{
    return new HttpClient
    {
        BaseAddress = new Uri("http://localhost:8080/")
    };
});

// Registrar proveedor de autenticación
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthStateProvider>());

// Servicios
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmpleadoService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<CartaService>();
builder.Services.AddScoped<TurnoService>();
builder.Services.AddScoped<EmpleadoTurnoService>();
builder.Services.AddScoped<JhiUserService>();
builder.Services.AddScoped<PedidoMozoApiService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();