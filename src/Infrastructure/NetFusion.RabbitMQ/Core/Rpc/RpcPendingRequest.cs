using NetFusion.Common;
using System.Threading;
using System.Threading.Tasks;

namespace NetFusion.RabbitMQ.Core.Rpc
{
    /// <summary>
    /// Represents a pending RPC style message that is awaiting a response
    /// to be placed in a specified replay queue.
    /// </summary>
    public class RpcPendingRequest
    {
        private readonly TaskCompletionSource<byte[]> _futureResult;
        private readonly CancellationTokenSource _timeCancelToken;
        private readonly CancellationTokenRegistration _cancelTokenReg;

        public RpcPendingRequest(
            TaskCompletionSource<byte[]> futureResult,
            CancellationToken externalCancellationToken,
            int cancelRequestAfterMs)
        {
            Check.NotNull(futureResult, nameof(futureResult));
            Check.IsTrue(cancelRequestAfterMs > 0, nameof(cancelRequestAfterMs),
                "cancellation time must be greater than zero");

            _futureResult = futureResult;

            // Cancellation token and registration.
            _timeCancelToken = new CancellationTokenSource();
            _timeCancelToken.CancelAfter(cancelRequestAfterMs);

            // Combines time based cancellation token with the caller's cancellation token.
            var combinedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                _timeCancelToken.Token, 
                externalCancellationToken);

            _cancelTokenReg = combinedCancellation.Token.Register(_futureResult.SetCanceled);
        }

        // Satisfies the future result of the awaiting consumer with
        // the received response.
        public void SetResult(byte[] result)
        {
            Check.NotNull(result, nameof(result));

            UnRegister();
            _futureResult.SetResult(result);
        }

        // Fail the future result with an RPC exception containing
        // the serialized exception from the server.
        public void SetException(byte[] result)
        {
            Check.NotNull(result, nameof(result));

            UnRegister();
            _futureResult.SetException(new RpcReplyException(result));
        }

        // Cancels the task and unregisters from cancellation token.
        public void Cancel()
        {
            _timeCancelToken.Cancel();
            _cancelTokenReg.Dispose();
        }

        // Unregisters the task from cancellation token.
        public void UnRegister()
        {
            _cancelTokenReg.Dispose();
        }
    }
}
