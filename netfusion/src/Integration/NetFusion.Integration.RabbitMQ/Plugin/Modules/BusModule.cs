using EasyNetQ;
using EasyNetQ.Persistent;
using NetFusion.Common.Base;
using NetFusion.Core.Settings;
using NetFusion.Integration.RabbitMQ.Bus;
using NetFusion.Integration.RabbitMQ.Internal;
using NetFusion.Integration.RabbitMQ.Plugin.Configs;
using NetFusion.Integration.RabbitMQ.Plugin.Settings;

namespace NetFusion.Integration.RabbitMQ.Plugin.Modules;

public class BusModule : PluginModule,
    IBusModule
{
    private RabbitMqConfig? _rabbitMqConfig;
    private BusSettings? _busSettings;
    
    // The bus instances keyed by name created from BusSettings.
    private readonly Dictionary<string, BusConnection> _buses = new Dictionary<string, BusConnection>();

    private BusSettings BusSettings => _busSettings ??
        throw new NullReferenceException("Bus settings not initialized.");
    
    public event EventHandler<ReconnectionEventArgs>? Reconnection;

    public RabbitMqConfig RabbitMqConfig => _rabbitMqConfig ?? 
        throw new NullReferenceException("Plugin configuration not initialized");
    
    public override void Initialize()
    {
        _rabbitMqConfig = Context.Plugin.GetConfig<RabbitMqConfig>();
            
        try
        {
            _busSettings = Context.Configuration.GetSettings(new BusSettings());
            _busSettings.SetNamedConfigurations();
        }
        catch (SettingsValidationException ex)
        {
            NfExtensions.Logger.Log<BusModule>(LogLevel.Error, ex.Message);
            throw;
        }
    }

    public BusConnection GetConnection(string busName)
    {
        if (string.IsNullOrWhiteSpace(busName))
            throw new ArgumentException("Namespace must be specified.", nameof(busName));
            
        if (_buses.TryGetValue(busName, out BusConnection? connection))
        {
            return connection;
        }

        throw new RabbitMqPluginException(
            $"A client for the namespace {connection} has not been configured.", "BUS_CONN_NOTFOUND");
    }

    protected override Task OnStartModuleAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger<BusConnection>();
        
        foreach (ConnectionSettings connSettings in BusSettings.Connections.Values)
        {
            CreateBus(logger, connSettings);
        }

        return base.OnStartModuleAsync(services);
    }
    
    protected override Task OnStopModuleAsync(IServiceProvider services)
    {
        foreach(BusConnection busConn in _buses.Values)
        {
            busConn.Bus.Dispose();
        }
            
        return base.OnStopModuleAsync(services);
    }
    
    private void CreateBus(ILogger<BusConnection> logger, ConnectionSettings connSettings)
    {
        if (_buses.ContainsKey(connSettings.BusName))
        {
            throw new RabbitMqPluginException(
                $"A bus has already been created for the bus named: {connSettings.BusName}." + 
                "Check configuration for duplicates.");
        }

        var busConn = new BusConnection(Context, connSettings);
        
        _buses[connSettings.BusName] = busConn;
        MonitorConnection(connSettings, busConn.Bus);
    }
    
    private void MonitorConnection(ConnectionSettings conn, IBus bus)
    {
        if (bus.Advanced == null) return;
            
        bus.Advanced.Disconnected += (_, _) => conn.IsConnected = false;

        bus.Advanced.Connected += (_, args) =>
        {
            // Initial connection to broker:
            if (conn.IsConnected == null)
            {
                conn.IsConnected = true;
                return;
            }
                
            Context.Logger.LogInformation("Connection reestablished to broker {BusName} ", conn.BusName);

            // If this is not the first time connecting, then the connection 
            // event is for a reconnection from a dropped connection.
            conn.IsConnected = true;

            if (args.Type == PersistentConnectionType.Consumer)
            {
                Reconnection?.Invoke(this, new ReconnectionEventArgs(conn));  
            }
        };
    }

    public override void Log(IDictionary<string, object> moduleLog)
    {
        if (_busSettings == null) return;
        
        var busLog = new Dictionary<string, object>();
        moduleLog["Buses"] = busLog;
        
        foreach (ConnectionSettings connection in _busSettings.Connections.Values)
        {
            busLog[connection.BusName] = new
            {
                connection.Heartbeat,
                connection.Hosts,
                connection.Timeout,
                connection.VHostName,
                connection.ConnectIntervalAttempt
            };
        }
    }
}