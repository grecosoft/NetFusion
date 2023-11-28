using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentAssertions;
using NetFusion.Common.Extensions.Reflection;

namespace NetFusion.Common.UnitTests.Extensions.Reflection;

public class AsyncTests
{
    [Fact]
    public void CanDetermineIfMethod_IsAsynchronous()
    {
        GetTestMethod(nameof(TestMethods.AsyncMethodToTest))
            .IsTaskMethod().Should().BeTrue();

        GetTestMethod(nameof(TestMethods.AsyncMethodWithResultToTest))
            .IsTaskMethod().Should().BeTrue();

        GetTestMethod(nameof(TestMethods.NonAsyncVoidMethod))
            .IsTaskMethod().Should().BeFalse();

        GetTestMethod(nameof(TestMethods.NonAsyncIntMethod))
            .IsTaskMethod().Should().BeFalse();
    }

    private MethodInfo GetTestMethod(string methodName)
    {
        return typeof(TestMethods).GetMethod(methodName) ?? 
               throw new NullReferenceException();
    }

    [Fact]
    public void CanDetermineIfMethod_ReturnsAsynchronousResult()
    {
        GetTestMethod(nameof(TestMethods.AsyncMethodToTest))
            .IsTaskMethodWithResult().Should().BeFalse();

        GetTestMethod(nameof(TestMethods.AsyncMethodWithResultToTest))
            .IsTaskMethodWithResult().Should().BeTrue();

        GetTestMethod(nameof(TestMethods.NonAsyncVoidMethod))
            .IsTaskMethodWithResult().Should().BeFalse();

        GetTestMethod(nameof(TestMethods.NonAsyncIntMethod))
            .IsTaskMethodWithResult().Should().BeFalse();
    }

    [Fact]
    public void CanDetermineIfMethod_IsAsynchronousAndCanCanceled()
    {
        GetTestMethod(nameof(TestMethods.CancelableAsyncMethod))
            .IsCancellableMethod().Should().BeTrue();

        GetTestMethod(nameof(TestMethods.AsyncMethodToTest))
            .IsCancellableMethod().Should().BeFalse();

        GetTestMethod(nameof(TestMethods.AsyncMethodWithResultToTest))
            .IsCancellableMethod().Should().BeFalse();

        GetTestMethod(nameof(TestMethods.NonAsyncVoidMethod))
            .IsCancellableMethod().Should().BeFalse();

        GetTestMethod(nameof(TestMethods.NonAsyncIntMethod))
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