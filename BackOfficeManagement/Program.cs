using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using BackOfficeManagement.Data;
using BackOfficeManagement.Services;
using Serilog;
using System.Globalization;
using System.Reflection;
using BackOfficeManagement.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var baseDir = AppDomain.CurrentDomain.BaseDirectory;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
var configuration = builder.Configuration
                        .SetBasePath(baseDir)
                        .AddJsonFile($"appsettings.{environment}.json", true)
                        .Build();

#region Logger
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateBootstrapLogger();
builder.Logging.ClearProviders();
builder.Host.UseSerilog();
#endregion

Log.Information($"Environment on {environment}");

builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});

builder.Services.AddRazorPages();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new List<CultureInfo>
    {
        // new CultureInfo("en-US"),
        new CultureInfo("th-TH")
    };

    options.DefaultRequestCulture = new RequestCulture("th-TH");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());
});

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
    {
        var type = typeof(BackOfficeManagement.SharedResource);
        var assemblyName = new AssemblyName(type.GetTypeInfo().Assembly.FullName);
        var factory = builder.Services.BuildServiceProvider().GetService<IStringLocalizerFactory>();
        var localizer = factory.Create("SharedResource", assemblyName.Name);
        options.DataAnnotationLocalizerProvider = (t, f) => localizer;
    });

//MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var serverVersion = ServerVersion.AutoDetect(connectionString);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireLowercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;
    options.SignIn.RequireConfirmedAccount = true;
})
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddHttpClient();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(3);
    options.SlidingExpiration = true;

    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddTransient<LanguageService>();
builder.Services.AddTransient<LocationService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ILineMessageRepository, LineMessageService>();
builder.Services.AddScoped<ILineDailySummaryRepository, LineDilySummaryService>();
builder.Services.AddHostedService<DailyLineSummaryWorker>();
builder.Services.AddScoped<IDashboardRepo, DashboardService>();

var app = builder.Build();

#region Database Migrate & Add new User
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var _logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        try
        {
            //Create database
            var databaseCreator = dbContext.GetService<Microsoft.EntityFrameworkCore.Storage.IDatabaseCreator>() as Microsoft.EntityFrameworkCore.Storage.RelationalDatabaseCreator;
            if (databaseCreator is not null)
            {
                if (!databaseCreator.CanConnect()) databaseCreator.Create();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error create the database");
        }

        if (dbContext.Database.IsMySql())
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            try
            {
                await dbContext.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error migtation the database");
            }

            await ApplicationSeed.SeedDefaultUserAsync(userManager, roleManager);

            //seed data
            await ApplicationSeed.SeedSampleDataAsync(dbContext);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "error seeding the database");
    }
}
#endregion

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "identity",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
