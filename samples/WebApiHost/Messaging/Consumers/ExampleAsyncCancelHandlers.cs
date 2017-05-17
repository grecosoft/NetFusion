using NetFusion.Messaging;
using System.Threading;
using System.Threading.Tasks;
using WebApiHost.Messaging.Messages;

namespace WebApiHost.Messaging.Consumers
{
    public class ExampleAsyncCancelHandler1 : IMessageConsumer
    {
        [InProcessHandler]
        public Task OnEventAsync(ExampleAsyncCancelEvent evt, CancellationToken ct)
        {
            return Worker.Run(evt, ct);
        }
    }

    public class ExampleAsyncCancelHandler2 : IMessageConsumer
    {
        [InProcessHandler]
        public Task OnEventAsync(ExampleAsyncCancelEvent evt, CancellationToken ct)
        {
            return Worker.Run(evt, ct);
        }
    }


    public static class Worker
    {
        public static Task Run(ExampleAsyncCancelEvent evt, CancellationToken ct)
        {
            return Task.Run(
                () =>
                {
                    for (int i = 0; i < evt.Seconds; i++)
                    {
                        Thread.Sleep(1000);
                        if (ct.IsCancellationRequested)
                        {
                            ct.ThrowIfCancellationRequested();
                            break;
                        }
                    }
                }, ct);
        }
    }
}
