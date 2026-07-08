using System;
using System.Linq;
using System.Text.Json;
using Asp.Versioning;
using Auth0.AspNetCore.Authentication.Api;
using LineItem.Api.HealthChecks;
using LineItem.Api.Middleware;
using LineItem.BuildVersion;
using LineItem.Repositories;
using LineItem.Repositories.Helpers;
using LineItem.Repositories.Interfaces;
using LineItem.Services;
using LineItem.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace LineItem.Api;

public class Program
{
    public static void Main(string[] args)
    {
        // bootstrap logger for capturing issues during application startup
        Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            // configure the service collection
            ConfigureServices(builder.Services, builder.Configuration);

            // configure the application (middleware, controllers, etc.)
            var application = builder.Build();
            ConfigureApplication(application);

            // start the application
            application.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application encountered a fatal unhandled exception");
            Environment.ExitCode = 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog((serviceProvider, loggerConfig) =>
        {
            // Minimum Levels and Overrides
            loggerConfig
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning);

            // Enrichers
            loggerConfig
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName();

            // Output formatting
            var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
            if (environment.IsEnvironment("local"))
                loggerConfig.WriteTo.Console();
            else
                loggerConfig.WriteTo.Console(new CompactJsonFormatter());

            // Complete two-stage initialization w/ bootstrap logger (see here: https://github.com/serilog/serilog-aspnetcore?tab=readme-ov-file#two-stage-initialization)
            loggerConfig.ReadFrom.Services(serviceProvider);
        });

        // configure versioning
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("X-Api-Version")
                );
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        // add services to the container.
        services.AddControllers(options =>
        {
            // stops ASP.NET Core from removing "Async" from action names
            options.SuppressAsyncSuffixInActionNames = false;
        });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // configure authentication
        services.AddAuth0ApiAuthentication(configuration.GetSection("Auth0"));
        services.AddAuthorization();

        // configure CORS
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        // add repositories
        services
            .ConfigureDapperMapping()
            .AddDatabaseOptions(configuration)
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IProductCategoryRepository, ProductCategoryRepository>()
            .AddScoped<IProductDraftRepository, ProductDraftRepository>()
            .AddScoped<IProductRepository, ProductRepository>();

        // add services
        services
            .AddScoped<IUserService, UserService>()
            .AddScoped<IProductCategoryService, ProductCategoryService>()
            .AddScoped<IProductDraftService, ProductDraftService>()
            .AddScoped<IProductService, ProductService>();

        // Configure health checks
        services
            .AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>(nameof(DatabaseHealthCheck));

        // add cache for user context
        services.AddMemoryCache();
    }

    private static void ConfigureApplication(WebApplication application)
    {
        // configure the HTTP request pipeline.
        if (application.Environment.IsEnvironment("local"))
        {
            application.UseDeveloperExceptionPage();
            application.UseHttpsRedirection();
        }

        // learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        application.UseSwagger();
        application.UseSwaggerUI();

        // configure exception handler
        application.UseExceptionHandler("/error");

        // configure middleware
        application.UseRouting();
        application.UseCors();
        application.UseAuthentication();
        application.UseMiddleware<UserContextMiddleware>();
        application.UseAuthorization();

        // configure controllers
        application.MapControllers();

        // configure health checks
        application.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";

                var response = JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
                    duration = report.TotalDuration,
                    info = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                        data = e.Value.Data
                    }),
                    buildVersion = BuildVersionProvider.GetVersion()
                }, new JsonSerializerOptions { WriteIndented = true });

                await context.Response.WriteAsync(response);
            }
        });

        // TESTING ONLY
        // Public endpoint - no authentication required
        application.MapGet("/public", () => Results.Ok(new { Message = "This endpoint is public" }))
            .WithName("GetPublic");

        // Protected endpoint - requires authentication
        application.MapGet("/private", () => Results.Ok(new { Message = "This endpoint requires authentication" }))
            .RequireAuthorization()
            .WithName("GetPrivate");
    }
}