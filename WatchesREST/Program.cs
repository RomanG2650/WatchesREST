using Microsoft.AspNetCore.Mvc;
using WatchLibrary;
using WatchLibrary.Repositories;
using WatchLibrary.Database;

var builder = WebApplication.CreateBuilder(args);

// Henter connection string fra appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Registrerer DBConnection og WatchRepository som scoped services til dependency injection
builder.Services.AddScoped<DBConnection>(provider => new DBConnection(connectionString));
builder.Services.AddScoped<WatchRepository>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Tilføjer HSTS med specifikke indstillinger
builder.Services.AddHsts(options =>
{
    options.Preload = true; // Forhåndsindlæser HSTS til browsere
    options.IncludeSubDomains = true; // Gælder for alle subdomæner
    options.MaxAge = TimeSpan.FromDays(365); // Varighed på 365 dage
});

var app = builder.Build();


app.UseCors("AllowAll");

// Tilføjer X-Content-Type-Options header for at forhindre MIME-type sniffing
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    await next();
});

// Tilføjer Content-Security-Policy header
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self'; object-src 'none'; frame-ancestors 'none'; upgrade-insecure-requests; base-uri 'self'");
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); 
    app.UseSwaggerUI(); 
}
else
{
    app.UseHsts(); // Aktiverer HSTS middleware i produktion
}

app.UseHttpsRedirection(); 
app.UseAuthorization(); 
app.MapControllers(); 
app.Run(); 
