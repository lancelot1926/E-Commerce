using Ecommerce.Api.Hubs;
using Ecommerce.Application.Common.Realtime;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure;
using Ecommerce.Infrastructure.Auth;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Infrastructure (DbContext, repos, services, JWT generator)
builder.Services.AddInfrastructure(builder.Configuration);

// Controllers + endpoints
builder.Services.AddControllers();

builder.Services.AddMemoryCache(); // <— for a tiny TTL cache

// JWT Auth
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtSettings>(jwtSection);
var jwt = jwtSection.Get<JwtSettings>()!;
var keyBytes = Encoding.UTF8.GetBytes(jwt.Key);
builder.Services.AddSignalR();
builder.Services.AddScoped<IUserEvents, SignalRUserEvents>();
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // dev only
        options.SaveToken = true;
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
        options.Events = new JwtBearerEvents
        {
            /*OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine("JWT fail: " + ctx.Exception.Message);
                return Task.CompletedTask;
            },
            OnChallenge = ctx =>
            {
                Console.WriteLine("JWT challenge: " + ctx.ErrorDescription);
                return Task.CompletedTask;
            }*/
            OnTokenValidated = async ctx =>
            {
                var userIdStr = ctx.Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                             ?? ctx.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var tvClaim = ctx.Principal?.FindFirst("tv")?.Value;

                if (!int.TryParse(userIdStr, out var userId) || !int.TryParse(tvClaim, out var tokenVersion))
                {
                    ctx.Fail("Invalid token claims.");
                    return;
                }

                var services = ctx.HttpContext.RequestServices;
                var cache = services.GetRequiredService<IMemoryCache>();
                var repo = services.GetRequiredService<IUserRepository>();

                var cacheKey = $"uv:{userId}";
                if (!cache.TryGetValue(cacheKey, out (int tv, bool banned) snap))
                {
                    var user = await repo.GetByIdAsync(userId, ctx.HttpContext.RequestAborted);
                    if (user is null) { ctx.Fail("User not found."); return; }
                    snap = (user.TokenVersion, user.IsBanned);
                    cache.Set(cacheKey, snap, TimeSpan.FromMinutes(2)); // tiny TTL
                }

                if (snap.banned || snap.tv != tokenVersion)
                {
                    ctx.Fail("Token invalidated.");
                    return;
                }
            },
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/user"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// Swagger + Bearer button
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ecommerce API", Version = "v1" });
    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste your JWT token here (without the 'Bearer ' prefix)."
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy
            .WithOrigins("http://localhost:3000") // React dev server
            .AllowAnyHeader()
            .AllowAnyMethod().AllowCredentials());
});

var app = builder.Build();

app.UseStaticFiles();
app.UseCors("AllowReactApp");
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();



app.UseAuthentication();
app.UseAuthorization();

app.MapHub<UserEventsHub>("/hubs/user").RequireCors("AllowReactApp");
app.MapControllers().RequireCors("AllowReactApp");
app.MapControllers();

app.Run();
