<Query Kind="Program">
  <NuGetReference>NetFusion.Base</NuGetReference>
  <NuGetReference>NetFusion.Bootstrap</NuGetReference>
  <NuGetReference>NetFusion.Common</NuGetReference>
  <NuGetReference>NetFusion.Settings</NuGetReference>
  <NuGetReference>NetFusion.Test</NuGetReference>
  <Namespace>Autofac</Namespace>
  <Namespace>Microsoft.Extensions.Configuration</Namespace>
  <Namespace>NetFusion.Bootstrap.Configuration</Namespace>
  <Namespace>NetFusion.Bootstrap.Container</Namespace>
  <Namespace>NetFusion.Bootstrap.Exceptions</Namespace>
  <Namespace>NetFusion.Bootstrap.Extensions</Namespace>
  <Namespace>NetFusion.Bootstrap.Logging</Namespace>
  <Namespace>NetFusion.Bootstrap.Manifests</Namespace>
  <Namespace>NetFusion.Bootstrap.Plugins</Namespace>
  <Namespace>NetFusion.Settings</Namespace>
  <Namespace>NetFusion.Test.Container</Namespace>
  <Namespace>NetFusion.Test.Plugins</Namespace>
  <Namespace>NetFusion.Testing.Logging</Namespace>
</Query>

void Main()
{
	var resolver = new TestTypeResolver(this.GetType());
	
	// Create a host and core plug-in.
	var hostPlugin = new MockAppHostPlugin();
	var corePlugin = new MockCorePlugin();
	
	// Create a core plug-in and configure it to represent the settings plugin.
	corePlugin.UseSettingsPlugin();

	resolver.AddPlugin(hostPlugin, corePlugin);
	var container = ContainerSetup.Bootstrap(resolver);

	container.WithConfig<EnviromentConfig>(ec => {
		var configBuilder = CreateInMemoryConfig();
		ec.UseConfiguration(configBuilder);
	});
	
	// Build and start the container.
	container.Build();
	container.Start();
	
	// At runtime, application settings can be injected into a dependent 
	// application component.
	var settings = container.Services.Resolve<WindowSettings>();
	settings.Dump();
	
	container.Log.Dump();
}

// The following adds an in-memory configuration.  In a real application host, a settings configuration reading from
// an external data source such as a JSON file would be used or any other of the available or custom setting providers.
private static IConfigurationBuilder CreateInMemoryConfig()
{
	var builder = new ConfigurationBuilder();
	
	var dict = new Dictionary<string, string>
				{
					{"App:MainWindow:Height", "20"},
					{"App:MainWindow:Width", "50"},
					{"App:MainWindows:ValidatedValue", "3" },
					{"App:MainWindow:Dialog:Colors:Frame", "RED"},
					{"App:MainWindow:Dialog:Colors:Title", "DARK_RED"}
				};

	builder.AddInMemoryCollection(dict);
	return builder;
}

// The following is a typed representation for the above settings:
[ConfigurationSection("App:MainWindow")]
public class DisplaySettings : AppSettings
{
	public int Height { get; set; } = 1000;
	public int Width { get; set; } = 2000;
}

public class WindowSettings : DisplaySettings
{
	public Dialog Dialog { get; set; }
}

public class Dialog
{
	public Colors Colors { get; set; }
}

public class Colors
{
	public string Frame { get; set; }
	public string Title { get; set; }
}