<Query Kind="Program">
  <Reference Relative="..\libs\Autofac.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\Autofac.dll</Reference>
  <Reference Relative="..\libs\MongoDB.Bson.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\MongoDB.Bson.dll</Reference>
  <Reference Relative="..\libs\MongoDB.Driver.Core.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\MongoDB.Driver.Core.dll</Reference>
  <Reference Relative="..\libs\MongoDB.Driver.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\MongoDB.Driver.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Bootstrap.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.Bootstrap.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Common.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.Common.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Eventing.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.Eventing.dll</Reference>
  <Reference Relative="..\libs\NetFusion.MongoDB.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.MongoDB.dll</Reference>
  <Reference Relative="..\libs\NetFusion.RabbitMQ.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.RabbitMQ.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Settings.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.Settings.dll</Reference>
  <Reference Relative="..\libs\NetFusion.Settings.Mongo.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\NetFusion.Settings.Mongo.dll</Reference>
  <Reference Relative="..\libs\Newtonsoft.Json.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\Newtonsoft.Json.dll</Reference>
  <Reference Relative="..\libs\RabbitMQ.Client.dll">C:\_dev\git\Research\LinqPad\NetFusion\libs\RabbitMQ.Client.dll</Reference>
  <Namespace>Autofac</Namespace>
  <Namespace>MongoDB.Driver</Namespace>
  <Namespace>NetFusion.Bootstrap.Container</Namespace>
  <Namespace>NetFusion.Bootstrap.Extensions</Namespace>
  <Namespace>NetFusion.Bootstrap.Logging</Namespace>
  <Namespace>NetFusion.Bootstrap.Manifests</Namespace>
  <Namespace>NetFusion.Bootstrap.Plugins</Namespace>
  <Namespace>NetFusion.Bootstrap.Testing</Namespace>
  <Namespace>NetFusion.Common.Extensions</Namespace>
  <Namespace>NetFusion.Common.Extensions</Namespace>
  <Namespace>NetFusion.Eventing</Namespace>
  <Namespace>NetFusion.Eventing.Config</Namespace>
  <Namespace>NetFusion.MongoDB</Namespace>
  <Namespace>NetFusion.MongoDB.Configs</Namespace>
  <Namespace>NetFusion.MongoDB.Testing</Namespace>
  <Namespace>NetFusion.RabbitMQ</Namespace>
  <Namespace>NetFusion.RabbitMQ.Configs</Namespace>
  <Namespace>NetFusion.RabbitMQ.Consumers</Namespace>
  <Namespace>NetFusion.Settings</Namespace>
  <Namespace>NetFusion.Settings.Configs</Namespace>
  <Namespace>NetFusion.Settings.Strategies</Namespace>
  <Namespace>NetFusion.Settings.Testing</Namespace>
</Query>

// *************************************************************************************
// 								NetFusion.MongoDb Plug-in
// *************************************************************************************
public void SetupMongoDbExamples(MockPlugin hostPlugin)
{
	hostPlugin
		.AddPluginType<LinqPadTestDb>()
		.AddPluginType<TestDocumentMap>();
}

public void RunMongoDbExamples()
{
	nameof(RunMongoDbExamples).Dump();

	InsertAndQueryDocument();
}

// Example of using the MongoDB plug-in to insert and query a document.
// -------------------------------------------------------------------------------------
public void InsertAndQueryDocument()
{
	nameof(InsertAndQueryDocument).Dump();

	var context = AppContainer.Instance.Services.Resolve<IMongoDbClient<LinqPadTestDb>>();
	var testDocColl = context.GetCollection<TestDocument>();

	var testDoc = new TestDocument
	{
		ValueOne = "test value one",
		ValueTwo = "test value two"
	};

	testDocColl.InsertOneAsync(testDoc).Wait();
	testDoc.TestDocId.Dump();

	var builder = Builders<TestDocument>.Filter;
	var filter = builder.Eq(d => d.TestDocId, testDoc.TestDocId);

	var result = testDocColl.Find(filter).ToListAsync().Result;
	result.Dump();
}

public class LinqPadTestDb : MongoSettings
{
	public LinqPadTestDb()
	{
		this.IsInitializationRequired = false;
		this.DatabaseName = "LinkPadTestDb";
		this.MongoUrl = "mongodb://localhost:27017";
	}
}

public class TestDocument
{
	public string TestDocId { get; set; }
	public string ValueOne { get; set; }
	public string ValueTwo { get; set; }
}

public class TestDocumentMap : EntityClassMap<TestDocument>
{
	public TestDocumentMap()
	{
		this.CollectionName = "LinqPad.TestDoc";

		this.AutoMap();
		MapStringObjectIdProperty(p => p.TestDocId);
	}
}