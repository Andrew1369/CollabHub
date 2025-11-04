using CollabHub.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using CollabHub.Services;
using CollabHub.Hubs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using CollabHub.Services;


var builder = WebApplication.CreateBuilder(args);

// Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        o.JsonSerializerOptions.WriteIndented = true;
    });

var dbPathEnv = Environment.GetEnvironmentVariable("DB_PATH");
var cs = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(dbPathEnv))
{
    cs = $"Data Source={dbPathEnv}";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);


builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(cs));
builder.Services.AddHttpClient<IGoogleDriveService, GoogleDriveService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
});

builder.Services.AddAuthorization();

builder.Services.AddSignalR();

builder.Services.AddSingleton<IFeatureFlagService, FeatureFlagService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

var uploadsPhysical = Environment.GetEnvironmentVariable("UPLOADS_PATH");
if (!string.IsNullOrEmpty(uploadsPhysical))
{
    Directory.CreateDirectory(uploadsPhysical);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsPhysical),
        RequestPath = "/uploads"
    });
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

if (app.Environment.IsDevelopment())
{
    Seed.EnsureSeeded(app);
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}
app.MapHub<TodoHub>("/hubs/todo");
app.Run();
