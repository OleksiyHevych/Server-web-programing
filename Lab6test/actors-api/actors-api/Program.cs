using ActorsApi.Data;
using ActorsApi.Models;
using ActorsApi.Repositories;
using ActorsApi.Services; // <- Додано
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===== InMemory DB =====
builder.Services.AddDbContext<ActorsContext>(opt =>
    opt.UseInMemoryDatabase("MovieActorsDb"));
builder.Services.AddScoped<DbContext, ActorsContext>();
builder.Services.AddScoped<IRepository<Actor>, Repository<Actor>>();

// ---------- Controllers ----------
builder.Services.AddControllers();

//// ---------- MovieActorsLookup через HttpClient ----------
//builder.Services.AddHttpClient<MovieActorsLookup>(client =>
//{
//    client.BaseAddress = new Uri("https://localhost:7000"); // адреса MovieActorsService
//});

// ---------- JWT ----------
var jwt = builder.Configuration.GetSection("Jwt");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = false; // dev
        o.SaveToken = true;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("SUPER_SECRET_DEMO_KEY_CHANGE_ME_32CHARS_MIN"))
        };

    });

builder.Services.AddAuthorization();

// ---------- Swagger (+ Bearer Authorize) ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Actors API", Version = "v1" });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Description = "Введіть токен у форматі: Bearer {token}",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtScheme, Array.Empty<string>() } });
});

// ---------- CORS (для тестів/Swagger Try it out) ----------
builder.Services.AddCors(p => p.AddDefaultPolicy(b => b.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// ---------- CORS ----------
app.UseCors();

// ---------- Міграції ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ActorsContext>();
    db.Database.EnsureCreated();
}

// ---------- Swagger (dev) ----------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DocumentTitle = "Actors API Swagger";
        c.ConfigObject.AdditionalItems["persistAuthorization"] = true;
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
