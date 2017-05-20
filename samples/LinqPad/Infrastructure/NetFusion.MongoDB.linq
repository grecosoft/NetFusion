<Query Kind="Program">
  <NuGetReference>NetFusion.Base</NuGetReference>
  <NuGetReference>NetFusion.Bootstrap</NuGetReference>
  <NuGetReference>NetFusion.Common</NuGetReference>
  <NuGetReference>NetFusion.MongoDB</NuGetReference>
  <NuGetReference>NetFusion.Settings</NuGetReference>
  <NuGetReference>NetFusion.Test</NuGetReference>
  <Namespace>Autofac</Namespace>
  <Namespace>MongoDB.Driver</Namespace>
  <Namespace>NetFusion.Bootstrap.Container</Namespace>
  <Namespace>NetFusion.Bootstrap.Plugins</Namespace>
  <Namespace>NetFusion.MongoDB</Namespace>
  <Namespace>NetFusion.MongoDB.Configs</Namespace>
  <Namespace>NetFusion.Settings</Namespace>
  <Namespace>NetFusion.Test.Container</Namespace>
  <Namespace>NetFusion.Test.Plugins</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

void Main()
{
	var testDb = new NetFusionDB
	{
		MongoUrl = "mongodb://localhost:27017",
		DatabaseName = "NetFusion"
	};
	
	var resolver = new TestTypeResolver(this.GetType());
	var hostPlugin = new MockAppHostPlugin();
	var corePlugin = new MockCorePlugin();

	corePlugin.UseMongoDbPlugin(); 

	resolver.AddPlugin(hostPlugin, corePlugin);
	var container = ContainerSetup.Bootstrap(resolver);
	
	container.WithConfig((AutofacRegistrationConfig config) =>
		{
			config.Build = builder =>
			{
				builder.RegisterInstance(testDb);
			};
		});

	container.Build();
	container.Start();

	//container.Log.Dump();
	
	// Execute the examples:
	var customer = new Customer
	{
		FirstName = "Sam",
		LastName = "Smith",
		City = "Cheshire",
		State = "CT"
	};

	CreateCustomer(container, customer).Dump();
	GetCustomers(container).Dump();
}

public class NetFusionDB : MongoSettings
{
}

public Customer CreateCustomer(AppContainer container, Customer customer)
{
	var repository = container.Services.Resolve<IExampleRepository>();
	repository.AddCustomerAsync(customer).Wait();
	return customer;
}

public IEnumerable<Customer> GetCustomers(AppContainer container)
{
	var repository = container.Services.Resolve<IExampleRepository>();
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