using NetFusion.Messaging;
using System.Threading.Tasks;

namespace NetFusion.RabbitMQ.Core
{
    public interface IRpcClient
    {
        Task<byte[]> Invoke(ICommand command, byte[] messageBody);
    }
}
