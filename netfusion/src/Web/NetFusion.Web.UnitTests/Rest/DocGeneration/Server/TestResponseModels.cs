using NetFusion.Web.Rest.Resources;

namespace NetFusion.Web.UnitTests.Rest.DocGeneration.Server;

public class TestResponseModel
{
}

public class TestCreatedResponseModel
{

}

[Resource("api.sample.model")]
public class ModelWithExposedName
{

}

public class ModelWithoutExposedName
{

}

[Resource("api.root.model")]
public class RootResponseModel
{

}

[Resource("api.embedded.model")]
public class EmbeddedChildModel
{

}

public class ModelWithResourceLinks
{
    public string ModelId { get; set; }
    public int VersionNumber { get; set; }
}

public class EmbeddedModelWithResourceLinks
{
    public string ModelId { get; set; }
}