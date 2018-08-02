using Demo.WebApi.Commands;
using NetFusion.Messaging;

namespace Demo.WebApi.Handlers
{
    public class TestHandler : IMessageConsumer
    {
        [InProcessHandler]
        public void GeneratePdf(TestCommand command)
        {

        }

        [InProcessHandler]
        public void GetPropertyTax(GetPropertyTaxCommand command)
        {

        }
    }
}