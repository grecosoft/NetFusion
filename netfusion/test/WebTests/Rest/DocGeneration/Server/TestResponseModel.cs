using NetFusion.Rest.Resources;

namespace WebTests.Rest.DocGeneration.Server
{
    public class TestResponseModel
    {
    }

    public class TestCreatedResponseModel
    {

    }

    [ExposedName("api.sample.model")]
    public class ModelWithExposedName
    {

    }

    public class ModelWithoutExposedName
    {

    }

    [ExposedName("api.root.model")]
    public class RootResponseModel
    {

    }

    [ExposedName("api.embedded.model")]
    public class EmbeddedChildModel
    {

    }
}
