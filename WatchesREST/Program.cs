using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc;
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
builder.Services.AddScoped<OrderRepository>(); //Kurven
builder.Services.AddScoped<UserRepository>();    // Sørg for at registrere UserRepository
builder.Services.AddScoped<UserService>();      // Sørg for at registrere UserService
builder.Services.AddSingleton<JwtService>();


builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CartService>(); // CartService


// Konfigurerer CORS-politik til at tillade alle origin, metoder og headers
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontend", policy =>
	{
		policy.WithOrigins("http://127.0.0.1:5500") // ← eller http://127.0.0.1:5500 alt efter din frontend
			  .AllowCredentials()
			  .AllowAnyHeader()
			  .AllowAnyMethod();
	});
});

// Tilføjer session understøttelse
builder.Services.AddDistributedMemoryCache(); // Kræves for at bruge session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session udløber efter 30 minutter
    options.Cookie.HttpOnly = true; // Forhindrer JavaScript adgang til cookien
    options.Cookie.IsEssential = true; // Gør sessionen nødvendig for appens funktionalitet
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Sikrer kun, at sessioncookie sendes over HTTPS
	options.Cookie.SameSite = SameSiteMode.None; // 🔥 Dette fikser fejlen
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

// Konfigurerer JWT Authentication

var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration")); // Hentet fra appsettings.json
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddSingleton<JwtService>(); // Registrer din JWT token-generator

var app = builder.Build();  // Bygger applikationen, nu kan du bruge de registrerede services

// Middleware for CORS
app.UseCors("AllowFrontend");

// Tilføjer sikkerhedsheaders
app.Use(async (context, next) =>
{
	context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
	await next();
});

app.Use(async (context, next) =>
{
	context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com https://cdn.jsdelivr.net; object-src 'none'; frame-ancestors 'none'; upgrade-insecure-requests; base-uri 'self'");
	await next();
});

// Swagger (kun i udviklingsmiljø)
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}


app.UseHsts();
app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Tilføjet routing + flyttet session
app.UseSession();     //  Session SKAL være her

app.MapControllers();

app.Run();

