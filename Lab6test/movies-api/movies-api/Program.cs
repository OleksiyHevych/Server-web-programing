using Microsoft.EntityFrameworkCore;
using MoviesService.Data;
using MoviesService.Repositories;
using MoviesService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Movies API", Version = "v1" });
    var jwtScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Bearer {token}",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtScheme, Array.Empty<string>() } });
});

// DB
builder.Services.AddDbContext<MoviesDbContext>(opt =>
    opt.UseInMemoryDatabase("MoviesDb"));

// Repositories
builder.Services.AddScoped<IMovieRepository, MovieRepository>();

//// HttpClient for MovieActorsLookupClient
//builder.Services.AddHttpClient<MovieActorsLookupClient>(client =>
//{
//    client.BaseAddress = new Uri("https://localhost:7000/"); // порт MovieActorsService
//});

// JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("SUPER_SECRET_DEMO_KEY_CHANGE_ME_32CHARS_MIN"))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.DocumentTitle = "Movies API Swagger";
    c.ConfigObject.AdditionalItems["persistAuthorization"] = true;
});

// Middleware
app.UseAuthentication();
app.UseAuthorization();

// Seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MoviesDbContext>();
    MoviesSeeder.Seed(db);
}

app.MapControllers();

app.Run("http://localhost:7102");
