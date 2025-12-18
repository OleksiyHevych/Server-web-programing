using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

// ===== JWT =====
var jwt = builder.Configuration.GetSection("Jwt");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = false; // у продакшені => true
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ===== YARP =====
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ===== Swagger (gateway UI) =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("gateway", new() { Title = "API Gateway UI", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new()
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Description = "Bearer {token}",
        Reference = new Microsoft.OpenApi.Models.OpenApiReference
        { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
    });
    c.AddSecurityRequirement(new()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ===== CORS =====
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DocumentTitle = "API Gateway";
        c.SwaggerEndpoint("/swagger/gateway/swagger.json", "gateway");

        // proxied swagger json (proxied services)
        c.SwaggerEndpoint("/_swagger/auth/v1/swagger.json", "auth");
        c.SwaggerEndpoint("/_swagger/actors/v1/swagger.json", "actors");
        c.SwaggerEndpoint("/_swagger/movies/v1/swagger.json", "movies");
        c.SwaggerEndpoint("/_swagger/movie-actors/v1/swagger.json", "movie-actors");

        c.ConfigObject.AdditionalItems["persistAuthorization"] = true;
    });
}

app.UseHttpsRedirection();
app.UseCors();

// ✅ Вимикаємо JWT для /api/auth/** та /swagger/**
app.UseWhen(
    context => !(context.Request.Path.StartsWithSegments("/api/auth", StringComparison.OrdinalIgnoreCase)
              || context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase)
              || context.Request.Path.StartsWithSegments("/_swagger", StringComparison.OrdinalIgnoreCase)),
    branch =>
    {
        branch.UseAuthentication();
        branch.UseAuthorization();
    }
);

// Reverse proxy
app.MapReverseProxy();

app.Run();
