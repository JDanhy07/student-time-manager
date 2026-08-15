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
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Timely")));

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
        options.LoginPath = "/Usuarios/Index";
        options.AccessDeniedPath = "/Usuarios/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = true;
    });

builder.Services.AddSession();

var app = builder.Build();

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
