using System.Text;
using FluentAssertions;
using NetFusion.Common.Extensions.Types;

namespace NetFusion.Common.UnitTests.Extensions;

public class TypeExtensions
{
    [Fact]
    public void CanMapCSharpType_To_JavascriptType()
    {
        "Test".GetType().GetJsTypeName().Should().Be("String");
        DateTime.UtcNow.GetType().GetJsTypeName().Should().Be("Date");
    }

    [Fact]
    public void IfUnknownCSharpType_ObjectJavascript()
    {
        new StringBuilder().GetType().GetJsTypeName().Should().Be("Object");
    }
}