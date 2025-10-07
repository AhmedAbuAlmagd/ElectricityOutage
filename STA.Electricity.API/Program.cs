
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using STA.Electricity.API.Models;
using System.IO.Compression;
using System.Reflection;

namespace STA.Electricity.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // Fast JSON serialization settings
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                    options.JsonSerializerOptions.WriteIndented = false;
                });

            // Add Swagger/OpenAPI services
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "STA Electricity Outage Management API",
                    Version = "v1",
                    Description = "API for managing electricity cutting down incidents, transferring data from STA to FTA.",
                    Contact = new OpenApiContact
                    {
                        Name = "STA Electricity Team",
                        Email = "support@sta-electricity.com"
                    }
                });

                // Include XML comments for better documentation
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                // Add examples and better descriptions
                c.EnableAnnotations();
            });

            // Add response compression for better performance
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/json", "text/json" });
            });

            builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });

            builder.Services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });

            // Add rate limiting
            builder.Services.AddMemoryCache();
            builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
            builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
            builder.Services.AddInMemoryRateLimiting();
            builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            // Add Entity Framework
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add Service Layer (using stored procedures)
            builder.Services.AddScoped<STA.Electricity.API.Interfaces.ISyncService, STA.Electricity.API.Services.SyncService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "STA Electricity API v1");
                    c.RoutePrefix = string.Empty; 
                    c.DocumentTitle = "STA Electricity API Documentation";
                    c.DefaultModelsExpandDepth(-1); 
                    c.DisplayRequestDuration();
                    c.EnableTryItOutByDefault();
                });
            }
            else
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "STA Electricity API v1");
                    c.RoutePrefix = "docs"; 
                });
            }

            // Use CORS
            app.UseCors("AllowAll");

            // Use response compression
            app.UseResponseCompression();

            // Use rate limiting
            app.UseIpRateLimiting();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            // Add a simple health check endpoint
            app.MapGet("/health", () => new {
                Status = "Healthy",
                Timestamp = DateTime.Now,
                Environment = app.Environment.EnvironmentName
            });

            app.MapControllers();

            app.Run();
        }
    }
}
