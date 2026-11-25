
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Portfolio.Data;
using Portfolio.Models;
using Portfolio.Services;
using Portfolio.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. MVC E RAZOR
// ==========================================

builder.Services.AddControllersWithViews();

// ==========================================
// 2. DATABASE POSTGRESQL
// ==========================================

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
});

// ==========================================
// 3. ASP.NET CORE IDENTITY
// ==========================================

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Requisiti password
        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;

        // Blocco dopo tentativi falliti
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);

        options.Lockout.MaxFailedAccessAttempts = 5;

        options.Lockout.AllowedForNewUsers = true;

        // Utenti
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ==========================================
// 4. COOKIE DI AUTENTICAZIONE
// ==========================================

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";

    options.AccessDeniedPath =
        "/account/access-denied";

    options.Cookie.Name = "Portfolio.Auth";

    options.Cookie.HttpOnly = true;

    options.Cookie.SameSite = SameSiteMode.Lax;

    // HTTP locale, HTTPS in produzione
    options.Cookie.SecurePolicy =
        builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;

    // Durata del login
    options.ExpireTimeSpan = TimeSpan.FromHours(8);

    options.SlidingExpiration = true;
});

// ==========================================
// 5. SERVIZI APPLICATIVI
// ==========================================

builder.Services.AddScoped<IProjectService, ProjectService>();

// ==========================================
// 6. CREAZIONE APPLICAZIONE
// ==========================================

var app = builder.Build();

// Gestione errori e HTTPS
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

// File statici
app.UseStaticFiles();

// Routing
app.UseRouting();

// Autenticazione e autorizzazione
app.UseAuthentication();
app.UseAuthorization();

// Static Web Assets
app.MapStaticAssets();

// Endpoint dei Controller con attributi
app.MapControllers();

// Routing MVC convenzionale
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
).WithStaticAssets();

// Test temporaneo
if (app.Environment.IsDevelopment())
{
    app.MapGet("/ping", () => "Portfolio funzionante!");
}

app.Run();
