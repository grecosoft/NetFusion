using FluentAssertions;
using NetFusion.Common.Extensions.Reflection;
using System;
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

            GetType().GetMethod(nameof(AscynMethodWithResultToTest))
                .IsAsyncMethod().Should().BeTrue();

            GetType().GetMethod(nameof(NonAsyncVoidMethod))
                .IsAsyncMethod().Should().BeFalse();

            GetType().GetMethod(nameof(NonAsyncIntMethod))
                .IsAsyncMethod().Should().BeFalse();
        }

        [Fact (DisplayName = "Can determine if method returns Asynchronous result")]
        public void CanDetermineIfMethod_ReturnsAsynchronousResult()
        {
            GetType().GetMethod(nameof(AsyncMethodToTest))
                .IsAsyncMethodWithResult().Should().BeFalse();

            GetType().GetMethod(nameof(AscynMethodWithResultToTest))
                .IsAsyncMethodWithResult().Should().BeTrue();

            GetType().GetMethod(nameof(NonAsyncVoidMethod))
                .IsAsyncMethodWithResult().Should().BeFalse();

            GetType().GetMethod(nameof(NonAsyncIntMethod))
                .IsAsyncMethodWithResult().Should().BeFalse();
        }

        [Fact (DisplayName = "Can determine if method is Asynchronous and can be Canceled")]
        public void CanDetermineIfMethod_IsAsynchronousAndCanCanceled()
        {
            GetType().GetMethod(nameof(CancelableAsyncMethod))
               .IsCancellableMethod().Should().BeTrue();

            GetType().GetMethod(nameof(AsyncMethodToTest))
               .IsCancellableMethod().Should().BeFalse();

            GetType().GetMethod(nameof(AscynMethodWithResultToTest))
                .IsCancellableMethod().Should().BeFalse();

            GetType().GetMethod(nameof(NonAsyncVoidMethod))
                .IsCancellableMethod().Should().BeFalse();

            GetType().GetMethod(nameof(NonAsyncIntMethod))
                .IsCancellableMethod().Should().BeFalse();
        }

#pragma warning disable xUnit1013 // Public method should be marked as test
        public static Task AsyncMethodToTest()
#pragma warning restore xUnit1013 // Public method should be marked as test
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

#pragma warning disable xUnit1013 // Public method should be marked as test
        public static void NonAsyncVoidMethod()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {

        }

        public static int NonAsyncIntMethod()
        {
            throw new NotImplementedException();
        }
    }
   
}
