using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using pract8_trpo.Models;

using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

using Task = System.Threading.Tasks.Task;
using TaskModel = pract8_trpo.Models.Task;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите токен"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new List<string>()
        }
    });
});
builder.Services.AddAuthorization();



builder.Services.AddDbContext<KaramovDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<List<User>>();
builder.Services.AddSingleton<List<TaskModel>>();
builder.Services.AddSingleton<ConcurrentDictionary<string, DateTime>>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

var key = builder.Configuration["Jwt:Key"] ?? throw new Exception("Не задан секретный ключ JWT");
if (Encoding.UTF8.GetByteCount(key) < 32) throw new Exception("Ключ JWT слишком короткий");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidIssuer = "pract8_trpo",

            ValidateAudience = true,
            ValidAudience = "pract8_client",

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key)
            ),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var jti = context.Principal?
                    .FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                if (string.IsNullOrEmpty(jti))
                {
                    context.Fail("В JWT отсутсвует jti");
                    return Task.CompletedTask;
                }

                var revokedTokens = context.HttpContext.RequestServices
                    .GetRequiredService<ConcurrentDictionary<string, DateTime>>();

                if (revokedTokens.ContainsKey(jti))
                    context.Fail("JWT отозван");

                return Task.CompletedTask;
            }
        };

    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();