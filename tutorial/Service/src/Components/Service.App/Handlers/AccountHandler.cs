using System;
using System.Threading.Tasks;
using NetFusion.Messaging;
using Service.Domain.Events;
using Service.Domain.Services;

namespace Service.App.Handlers
{
    public class AccountHandler : IMessageConsumer
    {
        private readonly IExampleResultLog _log;
        
        public AccountHandler(IExampleResultLog log)
        {
            _log = log;
        }
        
        [InProcessHandler]
        public Task EstablishCreditWhen(NewAccountCreated command)
        {
            _log.AddResult("Account Handler was Called.");
            
            if (command.AccountNumber == null)
            {
                throw new InvalidOperationException("Account number can't be null.");
            }
            
            return Task.Delay(5);
        }
    }
}