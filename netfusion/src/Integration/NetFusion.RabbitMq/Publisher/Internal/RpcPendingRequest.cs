using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetFusion.RabbitMQ.Publisher.Internal
{
    /// <summary>
    /// Represents a pending RPC style message that is awaiting a response
    /// to be placed in a specified replay queue.
    /// </summary>
    public class RpcPendingRequest
    {
        private readonly TaskCompletionSource<byte[]> _taskSource;
        private readonly CancellationTokenSource _timeCancelToken;
        private CancellationTokenRegistration _cancelTokenReg;

        public RpcPendingRequest(
            TaskCompletionSource<byte[]> taskSource,
            CancellationToken externalCancellationToken,
            int cancelRequestAfterMs)
        {
            if (cancelRequestAfterMs <= 0)
                throw new ArgumentOutOfRangeException(nameof(cancelRequestAfterMs), 
                    "Cancellation time must be greater than zero");
            
            _taskSource = taskSource ?? throw new ArgumentNullException(nameof(taskSource));

            // Cancellation token and registration.
            _timeCancelToken = new CancellationTokenSource();
            _timeCancelToken.CancelAfter(cancelRequestAfterMs);

            // Combines time-based cancellation token with the caller's cancellation token.
            var combinedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                _timeCancelToken.Token, 
                externalCancellationToken);

            _cancelTokenReg = combinedCancellation.Token.Register(_taskSource.SetCanceled);
        }

        // Satisfies the future result of the awaiting consumer with the received response.
        public void SetResult(byte[] result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            UnRegister();
            _taskSource.SetResult(result);
        }

        // Fail the future result with an RPC exception containing
        // the serialized exception from the server.
        public void SetException(byte[] result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            UnRegister();
            _taskSource.SetException(new RpcReplyException(result));
        }

        // Cancels the task and unregister from cancellation token.
        public void Cancel()
        {
            _timeCancelToken.Cancel();
            _cancelTokenReg.Dispose();
        }

        // Unregister the task from cancellation token.
        public void UnRegister()
        {
            _cancelTokenReg.Dispose();
        }
    }
}