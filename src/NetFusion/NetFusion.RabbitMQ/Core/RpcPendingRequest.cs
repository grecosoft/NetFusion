using NetFusion.Common;
using System.Threading;
using System.Threading.Tasks;

namespace NetFusion.RabbitMQ.Core
{
    /// <summary>
    /// Represents a pending RPC style message that is awaiting a response
    /// to be placed in a specified replay queue.
    /// </summary>
    public class RpcPendingRequest
    {
        private readonly TaskCompletionSource<byte[]> _futureResult;
        private readonly CancellationTokenSource _cancelToken;
        private readonly CancellationTokenRegistration _cancelTokenReg;

        public RpcPendingRequest(
            TaskCompletionSource<byte[]> futureResult, 
            int cancelRequestAfterMs)
        {
            Check.NotNull(futureResult, nameof(futureResult));
            Check.IsTrue(cancelRequestAfterMs > 0, nameof(cancelRequestAfterMs),
                "cancellation time must be greater than zero");

            _futureResult = futureResult;

            // Cancellation token and registration.
            _cancelToken = new CancellationTokenSource();
            _cancelToken.CancelAfter(cancelRequestAfterMs);
            _cancelTokenReg =_cancelToken.Token.Register(_futureResult.SetCanceled);
        }

        // Satisfies the future result of the awaiting consumer with
        // the received response.
        public void SetResult(byte[] result)
        {
            Check.NotNull(result, nameof(result));

            UnRegister();
            _futureResult.SetResult(result);
        }

        // Cancels the task and unregisters from cancelation token.
        public void Cancel()
        {
            _cancelToken.Cancel();
            _cancelTokenReg.Dispose();
        }

        // Unregisters the task from cancelation token.
        public void UnRegister()
        {
            _cancelTokenReg.Dispose();
        }
    }
}
