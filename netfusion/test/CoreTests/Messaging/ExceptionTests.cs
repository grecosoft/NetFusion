using System;
using Xunit;

namespace CoreTests.Messaging
{
    public class ExceptionTests
    {
        [Fact]
        public void Test()
        {
            var e = new AggregateException("Outer",
          
                    new InvalidOperationException("sasfasf"),
                    new AggregateException("Inner",
                    
                        new InvalidOperationException("Inner2"),
                        new AggregateException("sfsdf", new Exception[] { new InvalidOperationException("Inner4")})
                    )
                        
                );

            var fe = e.Flatten();

            var i = 100;
        }
        
    }
}