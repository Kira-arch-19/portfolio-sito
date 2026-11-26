    using Microsoft.AspNetCore.HttpOverrides;
    using System.Net;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.RateLimiting;
    using System.Threading.RateLimiting;
    using Microsoft.AspNetCore.DataProtection;    
    using Portfolio.Data;
    using Portfolio.Models;
    using Portfolio.Services;
    using Portfolio.Services.Interfaces;

    var builder = WebApplication.CreateBuilder(args);

    // ==========================================
    // 1. MVC E RAZOR
    // ==========================================
// ==========================================
// DATA PROTECTION - DOCKER
// ==========================================

// Salva le chiavi crittografiche nel volume Docker.
// Evita di invalidare i cookie quando il container
// viene ricreato.

builder.Services.AddDataProtection()
    .SetApplicationName("Portfolio")
    .PersistKeysToFileSystem(
        new DirectoryInfo("/app/keys"));
    builder.Services.AddControllersWithViews(options =>
    {
    options.Filters.Add(
        new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()
    );
    });
// ==========================================
// REVERSE PROXY - CADDY
// ==========================================

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    // Consideriamo attendibile soltanto il gateway
    // Docker utilizzato dal reverse proxy.
    options.KnownProxies.Add(IPAddress.Parse("172.18.0.1"));

    options.ForwardLimit = 1;
});
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
builder.Services.AddScoped<IContactEmailService, ContactEmailService>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("contact", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(10),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});
    // ==========================================
    // 6. CREAZIONE APPLICAZIONE
    // ==========================================

    var app = builder.Build();
// Deve essere eseguito prima dei middleware HTTPS,
// HSTS e autenticazione.
app.UseForwardedHeaders();
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        string? email = app.Configuration["AdminSeed:Email"];
        string? password = app.Configuration["AdminSeed:Password"];

        if (!string.IsNullOrWhiteSpace(email) &&
            !string.IsNullOrWhiteSpace(password))
        {
            // Crea il ruolo Admin
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                var roleResult = await roleManager.CreateAsync(
                    new IdentityRole("Admin"));

                if (!roleResult.Succeeded)
                    throw new Exception(string.Join(", ",
                        roleResult.Errors.Select(e => e.Description)));
            }

            // Cerca l'utente
            var admin = await userManager.FindByEmailAsync(email);

            // Crea l'utente solo se non esiste
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = email,
                    Email = email
                };

                var result = await userManager.CreateAsync(
                    admin, password);

                if (!result.Succeeded)
                    throw new Exception(string.Join(", ",
                        result.Errors.Select(e => e.Description)));
            }

            // Assegna il ruolo Admin
            if (!await userManager.IsInRoleAsync(admin, "Admin"))
            {
                var result = await userManager.AddToRoleAsync(
                    admin, "Admin");

                if (!result.Succeeded)
                    throw new Exception(string.Join(", ",
                        result.Errors.Select(e => e.Description)));
            }

            Console.WriteLine("ADMIN CONFIGURATO CORRETTAMENTE");
        }
    }

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
    app.UseRateLimiter();
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

    app.Run();
