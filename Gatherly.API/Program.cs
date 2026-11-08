
using FluentValidation.AspNetCore;
using Gatherly.API.Middleware;
using Gatherly.API.Services;
using Gatherly.Data;
using Gatherly.Services;
using Gatherly.Services.Mapping;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NLog.Web;
using System.Text;
using Gatherly.Data.Context;
using Gatherly.Data.Initialization;

namespace Gatherly.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("GatherlyClient", policy =>
                {
                    policy
                        .WithOrigins(
                            "http://localhost:5500",
                            "http://127.0.0.1:5500")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // Logging
            builder.Logging.ClearProviders();
            builder.Host.UseNLog();

            // JWT Authentication
            var jwtKey = builder.Configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "JWT Key is missing from configuration.");

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,

                            ValidIssuer =
                                builder.Configuration["Jwt:Issuer"],

                            ValidAudience =
                                builder.Configuration["Jwt:Audience"],

                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(jwtKey))
                        };
                });

            // Authorization
            builder.Services.AddAuthorization();

            // FluentValidation
            builder.Services.AddFluentValidationAutoValidation();

            // Application Services
            builder.Services.AddScoped<JwtTokenService>();
            builder.Services.AddServices();
            builder.Services.AddData(builder.Configuration);

            // AutoMapper
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            // Controllers and Swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition(
                    "Bearer",
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                        Description = "Enter JWT token"
                    });

                options.AddSecurityRequirement(
                    new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                    {
                        {
                            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                            {
                                Reference =
                                    new Microsoft.OpenApi.Models.OpenApiReference
                                    {
                                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                        Id = "Bearer"
                                    }
                            },
                            Array.Empty<string>()
                        }
                    });
            });

            var app = builder.Build();

            // Database Seed
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                await DbInitializer.InitializeAsync(context);
            }

            // Middleware Pipeline
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseMiddleware<CorrelationIdMiddleware>();

            app.UseHttpsRedirection();

            app.UseCors("GatherlyClient");

            app.UseAuthentication();
            app.UseAuthorization();

            // Swagger
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapControllers();

            app.Run();
        }
    }
}
