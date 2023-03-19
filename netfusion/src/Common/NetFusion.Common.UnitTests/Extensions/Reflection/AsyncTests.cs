using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using NetFusion.Common.Extensions.Reflection;

namespace NetFusion.Common.UnitTests.Extensions.Reflection;

public class AsyncTests
{
    [Fact]
    public void CanDetermineIfMethod_IsAsynchronous()
    {
        typeof(TestMethods).GetMethod(nameof(TestMethods.AsyncMethodToTest))
            .IsAsyncMethod().Should().BeTrue();

        typeof(TestMethods).GetMethod(nameof(TestMethods.AsyncMethodWithResultToTest))
            .IsAsyncMethod().Should().BeTrue();

        typeof(TestMethods).GetMethod(nameof(TestMethods.NonAsyncVoidMethod))
            .IsAsyncMethod().Should().BeFalse();

        typeof(TestMethods).GetMethod(nameof(TestMethods.NonAsyncIntMethod))
            .IsAsyncMethod().Should().BeFalse();
    }

    [Fact]
    public void CanDetermineIfMethod_ReturnsAsynchronousResult()
    {
        typeof(TestMethods).GetMethod(nameof(TestMethods.AsyncMethodToTest))
            .IsAsyncMethodWithResult().Should().BeFalse();

        typeof(TestMethods).GetMethod(nameof(TestMethods.AsyncMethodWithResultToTest))
            .IsAsyncMethodWithResult().Should().BeTrue();

        typeof(TestMethods).GetMethod(nameof(TestMethods.NonAsyncVoidMethod))
            .IsAsyncMethodWithResult().Should().BeFalse();

        typeof(TestMethods).GetMethod(nameof(TestMethods.NonAsyncIntMethod))
            .IsAsyncMethodWithResult().Should().BeFalse();
    }

    [Fact]
    public void CanDetermineIfMethod_IsAsynchronousAndCanCanceled()
    {
        typeof(TestMethods).GetMethod(nameof(TestMethods.CancelableAsyncMethod))
            .IsCancellableMethod().Should().BeTrue();

        typeof(TestMethods).GetMethod(nameof(TestMethods.AsyncMethodToTest))
            .IsCancellableMethod().Should().BeFalse();

        typeof(TestMethods).GetMethod(nameof(TestMethods.AsyncMethodWithResultToTest))
            .IsCancellableMethod().Should().BeFalse();

        typeof(TestMethods).GetMethod(nameof(TestMethods.NonAsyncVoidMethod))
            .IsCancellableMethod().Should().BeFalse();

        typeof(TestMethods).GetMethod(nameof(TestMethods.NonAsyncIntMethod))
            .IsCancellableMethod().Should().BeFalse();
    }

    private static class TestMethods
    {
        public static Task AsyncMethodToTest()
        {
            throw new NotImplementedException();
        }

        public static Task<int> AsyncMethodWithResultToTest()
        {
            throw new NotImplementedException();
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
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