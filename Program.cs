using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Services;
using Portfolio.Services.Interfaces;
var builder = WebApplication.CreateBuilder(args);

// Registrazione dei servizi MVC
builder.Services.AddControllersWithViews();

// Registrazione del database PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
});
builder.Services.AddScoped<IProjectService,ProjectService>();
var app = builder.Build();

// Configurazione HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
).WithStaticAssets();

app.Run();
