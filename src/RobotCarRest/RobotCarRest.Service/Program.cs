// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NetworkController.ZipUtilities;
using Paregov.RobotCar.Rest.Service.BusinessLogic;
using Paregov.RobotCar.Rest.Service.Hardware;
using Paregov.RobotCar.Rest.Service.Servos;
using Paregov.RobotCar.Rest.Service.SoftwareUpdaters;
using Serilog;

namespace Paregov.RobotCar.Rest.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var baseDirectory = AppContext.BaseDirectory;
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            var builder = WebApplication.CreateBuilder(args);

            // Remove the default logging providers and integrate Serilog.
            builder.Host.UseSerilog((context, services, configuration) => configuration
                // Read configuration from appsettings.json if needed.
                .ReadFrom.Configuration(context.Configuration)
                // Read services from dependency injection.
                .ReadFrom.Services(services)
                // Enrich logs with contextual information.
                .Enrich.FromLogContext()
                // Configure the sinks (where logs are written to).
                .WriteTo.Console() // For viewing logs in the terminal during development.
                .WriteTo.File(
                    path: $"{baseDirectory}/logs/webapp-.log", // The log file path.
                    rollingInterval: RollingInterval.Day, // Create a new file daily.
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}", // A detailed format.
                    retainedFileCountLimit: 7 // Keep the last 7 daily logs.
                ));
        
            // Add services to the container.
            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("X-Api-Version")
                );
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API - V1", Version = "v1.0" });
                c.SwaggerDoc("v2", new OpenApiInfo { Title = "My API - V2", Version = "v2.0" });
            });

            builder.Services.AddSingleton<IServos, Servos.Servos>();
            builder.Services.AddSingleton<IUnzipUtility, UnzipUtility>();
            builder.Services.AddSingleton<ISpiCommunicator, SpiCommunicator>();
            builder.Services.AddSingleton<IHardwareControl, HardwareControl>();
            builder.Services.AddSingleton<IFirmwareUpdater, FirmwareUpdater>();
            builder.Services.AddSingleton<IRestUpdater, RestUpdater>();
            builder.Services.AddSingleton<IMotorSpeed, MotorSpeed>();

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(5000);
                options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // Set max request body size to 10 MB
            });

            var app = builder.Build();

            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Starting NetworkController application...");
            logger.LogInformation($"Base directory: {baseDirectory}.");

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseSerilogRequestLogging();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
