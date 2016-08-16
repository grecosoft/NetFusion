using System;
using System.Threading;

namespace NetFusion.Common
{
    public static class MethodInvoker
    {

        /// <summary>
        /// Executes an action after a specified delay a given number of times.
        /// </summary>
        /// <typeparam name="TException">The exception, when received, for which retries
        /// should be tried.</typeparam>
        /// <param name="millisecondsDelay"></param>
        /// <param name="times"></param>
        /// <param name="action"></param>
        public static void TryCallFor<TException>(int millisecondsDelay, int times, Action action)
            where TException : Exception
        {
            var i = 0;

            while(i < times)
            {
                try
                {
                    Thread.Sleep(millisecondsDelay);
                    action();
                    return;
                  
                }
                catch (TException)
                {
                    if (++i == times)
                    {
                        throw;
                    }
                }
                catch // Not the expected exception.
                {
                    throw;
                }
            }
        }

    }
}
