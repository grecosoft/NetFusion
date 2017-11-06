using Polly;
using System;

namespace NetFusion.Base.Exceptions
{
    /// <summary>
    /// Contains methods for executing an action and retrying if an exception is raised.
    /// </summary>
    public static class ExecuteAction
    {
        /// <summary>
        /// Executes an action and retries after a number of back-off seconds.
        /// </summary>
        /// <typeparam name="TException">The exception for which the action should be retried.</typeparam>
        /// <param name="numberRetries">The number attempts to retry the action.</param>
        /// <param name="action">The action to invoked.  Passed the current retry count.</param>
        /// <param name="backoffSeconds">The number of back-off seconds.</param>
        public static void WithRetry<TException>(
            int numberRetries,
            Action<int> action,
            int backoffSeconds = 2) where TException : Exception
        {
            if (numberRetries < 0)
                throw new ArgumentOutOfRangeException(nameof(numberRetries), 
                    "Number of retries must be greater than zero.");

            if (action == null)
                throw new ArgumentNullException(nameof(action),
                    "Action to invoke cannot be null.");

            Policy policy = GetBackoffPolicy<TException>(numberRetries, backoffSeconds);
            int retryCount = -1;

            policy.Execute(() => {
                retryCount++;
                action(retryCount);
            });
        }

        private static Policy GetBackoffPolicy<TException>(int numberRetries, int backoffSeconds)
            where TException : Exception
        {
            // Retry a specified number of times, using a function to 
            // calculate the duration to wait between retries based on 
            // the current retry attempt (allows for exponential back-off)
            // In this case will wait for
            //  2 ^ 1 = 2 seconds then
            //  2 ^ 2 = 4 seconds then
            //  2 ^ 3 = 8 seconds then
            //  2 ^ 4 = 16 seconds then
            //  2 ^ 5 = 32 seconds
            return Policy
              .Handle<TException>()
              .WaitAndRetry(numberRetries, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(backoffSeconds, retryAttempt))
              );
        }
    }
}
