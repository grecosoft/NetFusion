using FluentAssertions;
using NetFusion.Common.Extensions.Reflection;
using Xunit;
using System.Linq;

namespace CommonTests.Extensions.Reflection
{
    public class QueryReflectionTests
    {
        [Fact(DisplayName = "Given Type can determine Derived type of Another")]
        public void GivenType_CanDetermine_DerivedTypeOfAnother()
        {
            typeof(TestClass1).IsDerivedFrom<TestBase>().Should().BeTrue();
            typeof(TestClass1).IsDerivedFrom<ITest>().Should().BeTrue();
            typeof(TestClass2).IsDerivedFrom<ITest>().Should().BeFalse();
            typeof(TestClass1).IsDerivedFrom<TestClass1>().Should().BeFalse();
        }

        [Fact (DisplayName = "Given Type can determine Concrete type of Another")]
        public void GivenType_CanDetermine_ConcreteDerivedTypeOfAnother()
        {
            typeof(TestClass3).IsConcreteTypeDerivedFrom<ITest>().Should().BeFalse();
            typeof(TestClass1).IsConcreteTypeDerivedFrom<ITest>().Should().BeTrue();
        }

        [Fact(DisplayName = "Given Type find all Interfaces Deriving from Type")]
        public void GivenType_FindAllInterfacedDerivingFromType()
        {
            typeof(TestClass6).GetInterfacesDerivedFrom<IService>().Should().HaveCount(0);
            var foundInterface = typeof(TestClass7).GetInterfacesDerivedFrom<IService>().SingleOrDefault();
            foundInterface.Should().NotBeNull();
            foundInterface.Should().NotBeOfType(typeof(IModuleService));

            var foundInfterfaces = typeof(TestClass8).GetInterfacesDerivedFrom<IService>();
            foundInfterfaces.Should().HaveCount(2);
            foundInfterfaces.Should().Contain(typeof(IModuleService));
            foundInfterfaces.Should().Contain(typeof(IRunnable));
        }

        [Fact(DisplayName = "Given Type check declared Default Constructor")]
        public void GivenType_CheckDeclaredDefaultConstructor()
        {
            typeof(TestClass4).HasDefaultConstructor().Should().BeFalse();
            typeof(TestClass5).HasDefaultConstructor().Should().BeTrue();
        }

        interface ITest { }
        abstract class TestBase { }
        class TestClass1 : TestBase, ITest { }
        class TestClass2 : TestBase { }
        abstract class TestClass3 : ITest { }

        class TestClass4
        {
            private object data;
            public TestClass4(object data) { this.data = data; }
        }

        class TestClass5
        {
            private object data;
            public TestClass5() { }
            public TestClass5(object data) { this.data = data; }
        }

        interface IService { }
        class TestClass6 : IService { }
        interface IModuleService : IService { }
        class TestClass7 : IModuleService { };
        interface IRunnable : IService { }
        class TestClass8 : IModuleService, IRunnable { }
    }
}
