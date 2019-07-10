using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Rest.Client;
using NetFusion.Rest.Server.Plugin;
using NetFusion.Settings;
using NetFusion.Settings.Plugin;
using NetFusion.Test.Hosting;
using NetFusion.Test.Plugins;
using NetFusion.Web.Mvc.Plugin;
using Xunit;

namespace WebTests
{
    public class SampleTest
    {
        [Fact]
        public Task Demo()
        {
            return WebHostFixture.TestAsync<SampleTest>(async host =>
            {
                var response = await host
                    .WithSettings(new Dictionary<string, string>
                    {
                        {"Test:Settings:ValueOne", "Test Value One"},
                        {"Test:Settings:ValueTwo", "2"}
                    })
                    .ComposedFrom(composite =>
                    {
                        composite.AddSettings();
                        
                        var hostPlugin = new MockHostPlugin();
                        hostPlugin.AddPluginType<TestSettings>();
                        composite.AddPlugin(hostPlugin);
                        
                    })
                    .Act.OnRestClient(async c =>
                        {
                            var req = ApiRequest.Get("http://localhost/api/test/settings");
                            return await c.SendAsync<TestSettings>(req);
                        });

                response.Assert.ApiResponse(resp =>
                {
                    resp.Content.Should().NotBeNull();
                    resp.Content.Should().BeOfType<TestSettings>();

                    var settings = (TestSettings)resp.Content;
                    settings.ValueOne.Should().Be("Test Value One");
                    settings.ValueTwo.Should().Be(2);
                });
            });    
        }
    }
    
    [Route("api/test")]
    public class TestController : Controller
    {
        private readonly TestSettings _testSettings;
        
        public TestController(TestSettings testSettings)
        {
            _testSettings = testSettings;
        }
        
        [HttpGet("settings")]
        public IActionResult GetSettings()
        {
            return Ok(_testSettings);
        }
    }

    [ConfigurationSection("Test:Settings")]
    public class TestSettings : IAppSettings
    {
        public string ValueOne { get; set; }
        public int ValueTwo { get; set; }
    }
    
    
}