using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkshopWeb.Data;
using WorkshopWeb.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

/* Database Context Dependency Injection */
var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
var dbPassword = Environment.GetEnvironmentVariable("DB_SA_PASSWORD");
var connectionString = $"Data Source={dbHost};Initial Catalog={dbName};User ID=sa;Password={dbPassword};TrustServerCertificate=True;";
builder.Services.AddDbContext<WorkshopDbContext>(opt => opt.UseSqlServer(connectionString));

builder.Services.AddIdentity<CustomUser, IdentityRole>(
        options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.Password.RequiredUniqueChars = 0;
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;

        }
    )
    .AddEntityFrameworkStores<WorkshopDbContext>()
    .AddDefaultTokenProviders();

/* ===================================== */

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<CustomUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        DbInitializer.SeedAdminUser(userManager, roleManager).Wait();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
