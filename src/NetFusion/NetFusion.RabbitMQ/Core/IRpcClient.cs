using NetFusion.Messaging;
using RabbitMQ.Client;
using System.Threading.Tasks;

namespace NetFusion.RabbitMQ.Core
{
    public interface IRpcClient
    {
        Task<byte[]> Invoke(ICommand command, byte[] messageBody, 
            IModel publishChannel);
    }
}
