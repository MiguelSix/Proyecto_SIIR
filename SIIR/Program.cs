using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SIIR.Data;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using SIIR.DataAccess.Data.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("ConexionSQL") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();

//Contenedor de Trabajo
builder.Services.AddScoped<IContenedorTrabajo, ContenedorTrabajo>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Student}/{controller=Information}/{action=Index}/{id?}");

// Redirect to login page instead of any other page

app.MapGet("/", context =>
{
    context.Response.Redirect("/Identity/Account/Login");
    return Task.CompletedTask;
});


app.Run();
