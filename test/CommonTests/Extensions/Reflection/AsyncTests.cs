using FluentAssertions;
using NetFusion.Common.Extensions.Reflection;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CommonTests.Extensions.Reflection
{

    public class AsyncTests
    {
        [Fact (DisplayName = "Can determine if method is Asynchronous")]
        public void CanDetermineIfMethod_IsAsynchronous()
        {
            GetType().GetMethod(nameof(AsyncMethodToTest))
                .IsAsyncMethod().Should().BeTrue();

            GetType().GetMethod(nameof(AsyncTests.AscynMethodWithResultToTest))
                .IsAsyncMethod().Should().BeTrue();

            GetType().GetMethod(nameof(AsyncTests.NonAsyncVoidMethod))
                .IsAsyncMethod().Should().BeFalse();

            GetType().GetMethod(nameof(AsyncTests.NonAsyncIntMethod))
                .IsAsyncMethod().Should().BeFalse();
        }

        [Fact (DisplayName = "Can determine if method returns Asynchronous result")]
        public void CanDetermineIfMethod_ReturnsAsynchronousResult()
        {
            GetType().GetMethod(nameof(AsyncTests.AsyncMethodToTest))
                .IsAsyncMethodWithResult().Should().BeFalse();

            GetType().GetMethod(nameof(AsyncTests.AscynMethodWithResultToTest))
                .IsAsyncMethodWithResult().Should().BeTrue();

            GetType().GetMethod(nameof(AsyncTests.NonAsyncVoidMethod))
                .IsAsyncMethodWithResult().Should().BeFalse();

            GetType().GetMethod(nameof(AsyncTests.NonAsyncIntMethod))
                .IsAsyncMethodWithResult().Should().BeFalse();
        }

        [Fact (DisplayName = "Can determine if method is Asynchronous and can be Canceled")]
        public void CanDetermineIfMethod_IsAsynchronousAndCanCanceled()
        {
            GetType().GetMethod(nameof(AsyncTests.CancelableAsyncMethod))
               .IsCancellableMethod().Should().BeTrue();

            GetType().GetMethod(nameof(AsyncTests.AsyncMethodToTest))
               .IsCancellableMethod().Should().BeFalse();

            GetType().GetMethod(nameof(AsyncTests.AscynMethodWithResultToTest))
                .IsCancellableMethod().Should().BeFalse();

            GetType().GetMethod(nameof(AsyncTests.NonAsyncVoidMethod))
                .IsCancellableMethod().Should().BeFalse();

            GetType().GetMethod(nameof(AsyncTests.NonAsyncIntMethod))
                .IsCancellableMethod().Should().BeFalse();
        }

        public static Task AsyncMethodToTest()
        {
            throw new NotImplementedException();
        }

        public static Task<int> AscynMethodWithResultToTest()
        {
            throw new NotImplementedException();
        }

        public static Task<int> CancelableAsyncMethod(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public static void NonAsyncVoidMethod()
        {

        }

        public static int NonAsyncIntMethod()
        {
            throw new NotImplementedException();
        }
    }
   
}
