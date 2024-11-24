using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SIIR.Data;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using SIIR.DataAccess.Data.Repository;
using Microsoft.AspNetCore.Identity.UI.Services;
using SIIR.Utilities;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// Configura la cultura para español
var cultureInfo = new CultureInfo("es-ES");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("ConexionSQL") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddMvc()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
        options => options.SignIn.RequireConfirmedAccount = false
        )
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<SpanishIdentityErrorDescriber>();

// sessions must expire after 15 minutes of inactivity
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

// authentication must be done using cookies
builder.Services.ConfigureApplicationCookie(options =>
{
	options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
	options.LoginPath = "/Identity/Account/Login";
	options.LogoutPath = "/Identity/Account/Logout";
	options.AccessDeniedPath = "/Identity/Account/AccessDenied";
	options.SlidingExpiration = true;
});

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddTransient<IEmailSender, EmailSender>(i =>
    new EmailSender(
        builder.Configuration["EmailSettings:SmtpServer"],
        int.Parse(builder.Configuration["EmailSettings:SmtpPort"]),
        builder.Configuration["EmailSettings:FromEmailAddress"],
        builder.Configuration["EmailSettings:FromEmailPassword"]
    ));

//Contenedor de Trabajo
builder.Services.AddScoped<IContenedorTrabajo, ContenedorTrabajo>();

//PdfQuest
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

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

var supportedCultures = new[] { new CultureInfo("es-ES") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("es-ES"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseStaticFiles();

app.UseSession();

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
