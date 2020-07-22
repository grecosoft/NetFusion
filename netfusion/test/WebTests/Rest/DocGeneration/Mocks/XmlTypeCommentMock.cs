using NetFusion.Rest.Docs.Plugin;
using NetFusion.Rest.Docs.Plugin.Configs;
using NetFusion.Rest.Docs.XmlComments;

namespace WebTests.Rest.DocGeneration.Mocks
{
    public class XmlTypeCommentMock
    {
        public static XmlTypeCommentService Arrange
        {
            get
            {
                var docModule = new MockDocModule();
                var xmlCommentSrv = new XmlCommentService(docModule);
                return new XmlTypeCommentService(xmlCommentSrv);
            }
        }
    }

    public class MockDocModule : IDocModule
    {
        public RestDocConfig RestDocConfig => new RestDocConfig();
    }
}
