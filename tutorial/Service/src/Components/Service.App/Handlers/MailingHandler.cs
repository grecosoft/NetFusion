using System;
using NetFusion.Messaging;
using Service.Domain.Events;
using Service.Domain.Services;

namespace Service.App.Handlers
{
    public class MailingHandler : IMessageConsumer
    {
        private readonly IExampleResultLog _log;
        
        public MailingHandler(IExampleResultLog log)
        {
            _log = log;
        }

        [InProcessHandler]
        public void MailLastCatalogWhen(NewAccountCreated domainEvent)
        {
            _log.AddResult("Mailing Handler Called.");
            
            if (domainEvent.AccountNumber == null)
            {
                throw new InvalidOperationException("Catalog count not be mailed.");
            }

            if (domainEvent.FirstName != null && domainEvent.FirstName == "Sam")
            {
                Console.WriteLine("Sam's Club Catalog");
            }
        }
    }
}