using System;
using System.IO;
using FluentAssertions;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Web.Rest.CodeGen;
using NetFusion.Web.Rest.CodeGen.Plugin.Configs;
using NetFusion.Web.Rest.CodeGen.Plugin.Modules;
using NetFusion.Web.UnitTests.Rest.CodeGeneration.Server;
using NetFusion.Web.UnitTests.Rest.CodeGeneration.Setup;
using Xunit;

namespace NetFusion.Web.UnitTests.Rest.CodeGeneration;

/// <summary>
/// Contains unit tests for the plugin that generates TypeScript files from
/// an WebApi's publicly exposed REST Api models.  The TypeGen nuget package
/// is used to complete the actual code generation.
/// </summary>
public class CodeGenerationTests
{
    /// <summary>
    /// By default, TypeScript is generated to a default named directory within the 
    /// base directory of the application host.  This location is also where the
    /// generated code files are served from.
    /// </summary>
    [Fact]
    public void DefaultOutputDirectory_Configured()
    {
        var config = new RestCodeGenConfig();
        config.CodeGenerationDirectory.Should().Be(
            Path.Combine(AppContext.BaseDirectory, "ts-resources"));
    }

    /// <summary>
    /// During the bootstrapping of the application host, the directory
    /// containing the generated TypeScript files can be specified.
    /// </summary>
    [Fact]
    public void DefaultOutputDirectory_CanBeSpecified()
    {
        var config = new RestCodeGenConfig();
        config.SetCodeGenerationDirectory("/test/data/ts");
        config.CodeGenerationDirectory.Should().Be("/test/data/ts");
    }

    /// <summary>
    /// During the bootstrap process, the UseRestCodeGen extension method can
    /// be invoked on IApplicationBuilder to add middleware component to the
    /// pipeline that will server the TypeScript files.
    /// </summary>
    [Fact]
    public void TypeScriptFiles_ReturnedByDefaultEndpoint()
    {
        var config = new RestCodeGenConfig();
        config.EndpointUrl.Should().Be("/api/net-fusion/rest");
    }

    /// <summary>
    /// The host application can specific a specific endpoint.
    /// </summary>
    [Fact]
    public void EndpointToReadTypeScriptFiles_CanBeSpecified()
    {
        var config = new RestCodeGenConfig();
        config.UseEndpoint("/api/rest/ts");
        config.EndpointUrl.Should().Be("/api/rest/ts");
    }

    /// <summary>
    /// Models marked with the ExposedNameAttribute attribute are loaded for code generation.
    /// </summary>
    [Fact]
    public void AllExposedModels_LoadedForCodeGeneration()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.Container(TestSetup.WithDefaults)
                .Assert.PluginModule<CodeGenModule>(m =>
                {
                    m.ResourceTypes.Should().HaveCount(2);
                    m.ResourceTypes.Should().Contain(typeof(ApiModelOne));
                    m.ResourceTypes.Should().Contain(typeof(ApiModelTwo));
                });
        });
    }

    /// <summary>
    /// When the host application is bootstrapped, code generation can be disabled.
    /// This can be used by the developer if the public API is not changing or if
    /// the build process will generate the TypeScript files.  TypeGen also has a
    /// CLI that can be used.
    /// </summary>
    [Fact]
    public void IfDisabled_Models_NotLoadedForCodeGeneration()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.Container(TestSetup.WithDefaults)
                .PluginConfig<RestCodeGenConfig>(c => c.Disable())
                .Assert.PluginModule<CodeGenModule>(m =>
                {
                    m.ResourceTypes.Should().BeEmpty();
                });
        });
    }

    /// <summary>
    /// When the plugin is bootstrapped, a module registers the IApiCodeGenService
    /// that can be used to retrieve the generate typescript files.
    /// </summary>
    [Fact]
    public void ServiceAdded_ForCodeGeneration()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.Container(TestSetup.WithDefaults)
                .Act.OnCompositeApp(ca => ca.Start())
                .Assert.Service<IApiCodeGenService>(s =>
                {
                    s.Should().NotBeNull();
                });
        });
    }

    /// <summary>
    /// The IApiCodeGenService provides access for reading the generated TypeScript files.
    /// The names specified with the ExposedName attribute is used to lookup the code.
    /// </summary>
    [Fact]
    public void CanReadGeneratedTypeScript()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.Container(TestSetup.WithDefaults)
                .PluginConfig<RestCodeGenConfig>(c => 
                    c.SetCodeGenerationDirectory(Path.Combine(AppContext.BaseDirectory, "unit-test-gen-code")))
                .Act.OnCompositeApp(ca => ca.Start())
                .Assert.Service<IApiCodeGenService>(s =>
                {
                    s.TryGetResourceCodeFile("ResourceOne", out Stream stream).Should().BeTrue();
                    using var reader = new StreamReader(stream);
                    string code = reader.ReadToEnd();
                    code.Should().Contain("modelOneProp: string");
                });
        });
    }
}