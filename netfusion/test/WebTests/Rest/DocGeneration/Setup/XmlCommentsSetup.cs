using NetFusion.Rest.Docs.Entities;
using NetFusion.Rest.Docs.Plugin;
using NetFusion.Rest.Docs.Plugin.Configs;
using NetFusion.Rest.Docs.Xml.Services;

namespace WebTests.Rest.DocGeneration.Setup
{
    public class XmlCommentsSetup
    {
        public static XmlTypeCommentService TypeService
        {
            get
            {
                var docModule = new MockDocModule();
                var xmlCommentSrv = new XmlCommentService(docModule);
                return new XmlTypeCommentService(xmlCommentSrv);
            }
        }
        
        public static XmlCommentService XmlService
        {
            get
            {
                var docModule = new MockDocModule();
                return new XmlCommentService(docModule);
            }
        }
    }

    public class MockDocModule : IDocModule
    {
        public RestDocConfig RestDocConfig => new RestDocConfig();
        public HalComments HalComments => new HalComments();
    }
}
