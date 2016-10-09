<Query Kind="Program">
  <Reference Relative="..\libs\Autofac.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\Autofac.dll</Reference>
  <Reference Relative="..\libs\MongoDB.Bson.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\MongoDB.Bson.dll</Reference>
  <Reference Relative="..\libs\MongoDB.Driver.Core.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\MongoDB.Driver.Core.dll</Reference>
  <Reference Relative="..\libs\MongoDB.Driver.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\MongoDB.Driver.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Bootstrap.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.Bootstrap.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Common.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.Common.dll</Reference>
  <Reference Relative="..\libs\NetFusion.MongoDB.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.MongoDB.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Settings.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\NetFusion.Settings.dll</Reference>
  <Reference Relative="..\libs\Newtonsoft.Json.dll">E:\_dev\git\NetFusion\samples\LinqPad\libs\Newtonsoft.Json.dll</Reference>
  <Namespace>Autofac</Namespace>
  <Namespace>MongoDB.Driver</Namespace>
  <Namespace>NetFusion.Bootstrap.Container</Namespace>
  <Namespace>NetFusion.Bootstrap.Extensions</Namespace>
  <Namespace>NetFusion.Bootstrap.Logging</Namespace>
  <Namespace>NetFusion.Bootstrap.Manifests</Namespace>
  <Namespace>NetFusion.Bootstrap.Plugins</Namespace>
  <Namespace>NetFusion.Bootstrap.Testing</Namespace>
  <Namespace>NetFusion.Common.Extensions</Namespace>
  <Namespace>NetFusion.MongoDB</Namespace>
  <Namespace>NetFusion.MongoDB.Configs</Namespace>
  <Namespace>NetFusion.MongoDB.Testing</Namespace>
  <Namespace>NetFusion.Settings</Namespace>
  <Namespace>NetFusion.Settings.Configs</Namespace>
  <Namespace>NetFusion.Settings.Strategies</Namespace>
  <Namespace>NetFusion.Settings.Testing</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

void Main()
{
	var pluginDirectory = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "../libs");

	var typeResolver = new TestTypeResolver(pluginDirectory,
		"NetFusion.Settings.dll",
		"NetFusion.MongoDB.dll")
	{
		LoadAppHostFromAssembly = true
	};

	// Bootstrap the container:
	ContainerSetup.Bootstrap(typeResolver, config =>
	{
		config.AddPlugin<LinqPadHostPlugin>();
	})
	.Build()
	.Start();

	// Execute the examples:
	var customer = new Customer
	{
		FirstName = "Sam",
		LastName = "Smith",
		City = "Cheshire",
		State = "CT"
	};
	
	CreateCustomer(customer).Dump();
	GetCustomers().Dump();
	
}

// -------------------------------------------------------------------------------------
// Mock host plug-in that will be configured within the container.
// -------------------------------------------------------------------------------------
public class LinqPadHostPlugin : MockPlugin,
	IAppHostPluginManifest
{
	
}

// -------------------------------------------------------------------------------------
// In-Memory database configuration.
// -------------------------------------------------------------------------------------
public class NetFusionDB : MongoSettings
{
}

public class DatabaseInitializer : AppSettingsInitializer<NetFusionDB>
{
	protected override IAppSettings OnConfigure(NetFusionDB settings)
	{
		settings.MongoUrl = "mongodb://localhost:27017";
  		settings.DatabaseName = "NetFusion";

		return settings;
	}
}

public Customer CreateCustomer(Customer customer)
{
	var repository = AppContainer.Instance.Services.Resolve<IExampleRepository>();
	repository.AddCustomerAsync(customer).Wait();
	return customer;
}

public IEnumerable<Customer> GetCustomers()
{
	var repository = AppContainer.Instance.Services.Resolve<IExampleRepository>();
	return repository.ListCustomersAsync().Result;
}

public class Customer
{
	public string CustomerId { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string City { get; set; }
	public string State { get; set; }
}

public interface IExampleRepository
{
	Task AddCustomerAsync(Customer custoer);
	Task<IEnumerable<Customer>> ListCustomersAsync();
}

public class ExampleRepository : IExampleRepository
{
	private readonly IMongoDbClient<NetFusionDB> _refArchDb;
	private readonly IMongoCollection<Customer> _customerColl;

	public ExampleRepository(IMongoDbClient<NetFusionDB> refArchDb)
	{
		_refArchDb = refArchDb;
		_customerColl = _refArchDb.GetCollection<Customer>();
	}

	public async Task AddCustomerAsync(Customer customer)
	{
		await _customerColl.InsertOneAsync(customer);
	}

	public async Task<IEnumerable<Customer>> ListCustomersAsync()
	{
		return await _customerColl.Find(_ => true).ToListAsync();
	}
}

public class CustomerMapping : EntityClassMap<Customer>
{
	public CustomerMapping()
	{
		this.CollectionName = "RefArch.Customers";
		this.AutoMap();

		MapStringPropertyToObjectId(c => c.CustomerId);
	}
}

public class RepositoryModule : PluginModule
{
	public override void RegisterComponents(ContainerBuilder builder)
	{
		builder.RegisterType<ExampleRepository>()
			.As<IExampleRepository>()
			.InstancePerLifetimeScope();
	}
}