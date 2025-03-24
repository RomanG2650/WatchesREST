//using Microsoft.AspNetCore.Mvc;
//using WatchLibrary;
//using WatchLibrary.Repositories;
//using WatchLibrary.Database;
//using WatchesREST.Services;

//var builder = WebApplication.CreateBuilder(args);

//// Henter connection string fra appsettings.json
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

//// Registrerer DBConnection og WatchRepository som scoped services til dependency injection
//builder.Services.AddScoped<DBConnection>(provider => new DBConnection(connectionString));
//builder.Services.AddScoped<WatchRepository>();
//builder.Services.AddScoped<UserRepository>();
//builder.Services.AddScoped<UserService>();

//// Konfigurerer CORS-politik til at tillade alle origin, metoder og headers
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
//});



//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//// Tilføjer HSTS med specifikke indstillinger
//builder.Services.AddHsts(options =>
//{
//    options.Preload = true; // Forhåndsindlæser HSTS til browsere
//    options.IncludeSubDomains = true; // Gælder for alle subdomæner
//    options.MaxAge = TimeSpan.FromDays(365); // Varighed på 365 dage
//});

//var app = builder.Build();

//app.UseCors("AllowAll");

//// Tilføjer X-Content-Type-Options (underviserens linje)
//app.Use(async (context, next) =>
//{
//    context.Response.Headers.Append("X-Content-Type-Options", "nosniff"); // Beskytter mod MIME-type sniffing
//    await next();
//});

//// Tilføjer Content-Security-Policy i både udvikling og produktion, så sikkerhed kan testes
//app.Use(async (context, next) =>
//{
//    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com https://cdn.jsdelivr.net; object-src 'none'; frame-ancestors 'none'; upgrade-insecure-requests; base-uri 'self'"); // CSP beskytter mod XSS og andre angreb
//    await next();
//});

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger(); 
//    app.UseSwaggerUI(); 
//}

//app.UseHsts(); // Aktiverer HSTS middleware altid, så det kan testes i både udvikling og produktion

//app.UseHttpsRedirection(); 
//app.UseAuthorization(); 
//app.MapControllers(); 
//app.Run(); 
using Microsoft.AspNetCore.Mvc;
using WatchLibrary;
using WatchLibrary.Repositories;
using WatchLibrary.Database;
using WatchesREST.Services;

var builder = WebApplication.CreateBuilder(args);

// Henter connection string fra appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Registrer alle services før Build
// Registrer DBConnection og WatchRepository som scoped services til dependency injection
builder.Services.AddScoped<DBConnection>(provider => new DBConnection(connectionString));
builder.Services.AddScoped<WatchRepository>();
builder.Services.AddScoped<UserRepository>();    // Sørg for at registrere UserRepository
builder.Services.AddScoped<UserService>();      // Sørg for at registrere UserService
builder.Services.AddScoped<AuthenticationService>();  // Registrer AuthenticationService, så den kan bruges i controllers

// Konfigurerer CORS-politik til at tillade alle origin, metoder og headers
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Tilføj controllers og middleware til dokumentation
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

var app = builder.Build();  // Bygger applikationen, nu kan du bruge de registrerede services

// Middleware for CORS
app.UseCors("AllowAll");

// Tilføjer X-Content-Type-Options (beskyttelse mod MIME-type sniffing)
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    await next();
});

// Tilføjer Content-Security-Policy
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com https://cdn.jsdelivr.net; object-src 'none'; frame-ancestors 'none'; upgrade-insecure-requests; base-uri 'self'");
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHsts(); // Aktiverer HSTS middleware altid
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
