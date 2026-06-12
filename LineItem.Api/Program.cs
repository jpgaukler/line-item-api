using System;
using Asp.Versioning;
using Auth0.AspNetCore.Authentication.Api;
using LineItem.Api.Middleware;
using LineItem.Repositories;
using LineItem.Repositories.Helpers;
using LineItem.Repositories.Interfaces;
using LineItem.Repositories.Options;
using LineItem.Services;
using LineItem.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace LineItem.Api;

public static class Program
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

            // configure and run the application
            var application = builder.Build();
            ConfigureApplication(application);

            // start the application
            application.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application encountered a fatal unhandled exception");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // configure logging (two-stage initialization: https://github.com/serilog/serilog-aspnetcore?tab=readme-ov-file#two-stage-initialization
        services.AddSerilog((serviceProvider, logConfiguration) =>
            logConfiguration
                .ReadFrom.Configuration(configuration)
                .ReadFrom.Services(serviceProvider)
        );

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

        // set up database options
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection("DatabaseOptions"))
            .Validate(options =>
                    !string.IsNullOrWhiteSpace(options.Host) &&
                    !string.IsNullOrWhiteSpace(options.Port) &&
                    !string.IsNullOrWhiteSpace(options.Database) &&
                    !string.IsNullOrWhiteSpace(options.Username) &&
                    !string.IsNullOrWhiteSpace(options.Password),
                "DatabaseOptions is invalid. Host, Port, Database, Username, and Password are required."
            )
            .ValidateOnStart();

        // add repositories
        services
            .ConfigureDapperMapping()
            .AddScoped<IUserRepository, UserRepository>();

        // add services
        services.AddScoped<IUserService, UserService>();

        // Configure health checks
        services.AddHealthChecks();

        // add cache for user context
        services.AddMemoryCache();
    }

    private static void ConfigureApplication(WebApplication application)
    {
        // configure the HTTP request pipeline.
        if (application.Environment.IsDevelopment())
            application.UseDeveloperExceptionPage();

        // learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        application.UseSwagger();
        application.UseSwaggerUI();

        // configure exception handler
        application.UseExceptionHandler("/error");

        // configure middleware
        application.UseHttpsRedirection();
        application.UseRouting();
        application.UseCors();
        application.UseAuthentication();
        application.UseMiddleware<UserContextMiddleware>();
        application.UseAuthorization();

        // configure controllers
        application.MapControllers();

        // configure health checks
        application.MapHealthChecks("/health");

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