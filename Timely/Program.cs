using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Timely.Data;
using Timely.Services;
using Timely.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// EF Core: AddDbContext reemplaza el AddScoped<ApplicationDbContext> anterior.
// Lee la connection string desde appsettings.json — nunca hardcodeada en el código.

// El proveedor de base de datos se elige en appsettings.json (Database:Provider):
//   - "Sqlite":    no requiere instalar nada; el archivo timely.db se crea al iniciar.
//   - "SqlServer": usa la connection string "Timely" (por ejemplo, LocalDB).

var proveedor = builder.Configuration["Database:Provider"] ?? "SqlServer";
bool usaSqlite = proveedor.Equals("Sqlite", StringComparison.OrdinalIgnoreCase);

if (!usaSqlite && !proveedor.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException($"Database:Provider '{proveedor}' no es válido. Usa 'Sqlite' o 'SqlServer'.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (usaSqlite)
        options.UseSqlite(builder.Configuration.GetConnectionString("TimelySqlite"));
    else
        options.UseSqlServer(builder.Configuration.GetConnectionString("Timely"));
});

// Servicios — sin cambios respecto al paso anterior
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IProyectoService, ProyectoService>();
builder.Services.AddScoped<INotaService, NotaService>();
builder.Services.AddScoped<ICalendarioService, CalendarioService>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EsAdministrador", policy => policy.RequireRole("Administrador"));
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Usuarios/Login";
        options.AccessDeniedPath = "/Usuarios/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = true;
    });

builder.Services.AddSession();

var app = builder.Build();

// Con SQLite el esquema se crea desde el modelo (las migraciones del proyecto son de SQL Server).
// Con SQL Server el esquema sigue gestionándose con migraciones (dotnet ef database update).
if (usaSqlite)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();

    // Datos de demostración: solo en desarrollo y solo si la base de datos está vacía
    if (app.Environment.IsDevelopment() && builder.Configuration.GetValue<bool>("Database:SeedDemoData"))
    {
        DemoDataSeeder.Sembrar(context);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Usuarios}/{action=Login}/{id?}");

app.Run();
