using Solution.Context.App.Plugin;
using Solution.Context.Domain.Plugin;
using Solution.Context.Infra.Plugin;
using Solution.Context.WebApi.Plugin;
using NetFusion.Core.Settings.Plugin;
#if (useKubernetes)
using NetFusion.Web.FileProviders;
#endif

var builder = WebApplication.CreateBuilder(args);

#if (useKubernetes)
builder.Configuration.AddJsonFile(CheckSumFileProvider.FromRelativePath("configs"), 
    "appsettings.json", 
    optional: true, 
    reloadOnChange: true).AddEnvironmentVariables();
#endif

// Configure Logging:
InitializeLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);
builder.Host.UseSerilog();

builder.Services.AddCors();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();

var bootstrapLoggerFactory = LoggerFactory.Create(config =>
{ 
    config.ClearProviders();
    config.AddSerilog(Log.Logger);
    config.SetMinimumLevel(LogLevel.Trace);
});

try
{
    // Add Plugins to the Composite-Container:
    builder.Services.CompositeContainer(builder.Configuration, bootstrapLoggerFactory, new SerilogExtendedLogger())
        .AddSettings()

        .AddPlugin<InfraPlugin>()
        .AddPlugin<AppPlugin>()
        .AddPlugin<DomainPlugin>()
        .AddPlugin<WebApiPlugin>()
        .Compose();
}
catch
{
    Log.CloseAndFlush();
}

var app = builder.Build();

var viewerUrl = app.Configuration.GetValue<string>("Netfusion:ViewerUrl");
if (!string.IsNullOrWhiteSpace(viewerUrl))
{
    app.UseCors(cors => cors.WithOrigins(viewerUrl)
        .AllowAnyMethod()
        .AllowCredentials()
        .WithExposedHeaders("WWW-Authenticate", "resource-404")
        .AllowAnyHeader());
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

#if (useKubernetes)
app.MapHealthCheck();
app.MapStartupCheck();
app.MapReadinessCheck();
#endif


if (app.Environment.IsDevelopment())
{
    app.MapCompositeLog();
}


// Reference the Composite-Application to start the plugins then
// start the web application.
var compositeApp = app.Services.GetRequiredService<ICompositeApp>();
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

lifetime.ApplicationStopping.Register(() =>
{
    compositeApp.Stop();
    Log.CloseAndFlush();
});

var runTask = app.RunAsync();
await compositeApp.StartAsync();
await runTask;
return;

void InitializeLogger()
{
    // Allows changing the minimum log level of the service at runtime.
    LogLevelControl logLevelControl = new();
    logLevelControl.SetMinimumLevel(LogLevel.Debug);
    
    // Send any Serilog configuration issues logs to console.
    Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));
    Serilog.Debugging.SelfLog.Enable(Console.Error);

    var logConfig = new LoggerConfiguration()
        .MinimumLevel.ControlledBy(logLevelControl.Switch)
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .Destructure.UsingAttributes()

        .Enrich.FromLogContext()
        .Enrich.WithCorrelationId()
        .Enrich.WithHostIdentity(WebApiPlugin.HostId, WebApiPlugin.HostName)
        .Filter.SuppressReoccurringRequestEvents();

    logConfig.WriteTo.Console(theme: AnsiConsoleTheme.Literate);

    var seqUrl = builder.Configuration.GetValue<string>("logging:seqUrl");
    if (!string.IsNullOrEmpty(seqUrl))
    {
        logConfig.WriteTo.Seq(seqUrl);
    }

    Log.Logger = logConfig.CreateLogger();
    
    // Register Log Level Control so it can be injected into
    // a service at runtime to change the level.
    builder.Services.AddLogLevelControl(logLevelControl);
}
