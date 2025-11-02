using CollabHub.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var dbPathEnv = Environment.GetEnvironmentVariable("DB_PATH");
var cs = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(dbPathEnv))
{
    cs = $"Data Source={dbPathEnv}";
}
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(cs));

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

if (app.Environment.IsDevelopment())
{
    Seed.EnsureSeeded(app);
}

app.Run();
