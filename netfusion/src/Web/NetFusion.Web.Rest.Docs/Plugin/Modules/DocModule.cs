using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Web.Rest.Docs.Core;
using NetFusion.Web.Rest.Docs.Core.Descriptions;
using NetFusion.Web.Rest.Docs.Core.Services;
using NetFusion.Web.Rest.Docs.Entities;
using NetFusion.Web.Rest.Docs.Plugin.Configs;
using NetFusion.Web.Rest.Docs.Xml;
using NetFusion.Web.Rest.Docs.Xml.Services;

namespace NetFusion.Web.Rest.Docs.Plugin.Modules;

/// <summary>
/// The main plugin module registering documentation related services.
/// </summary>
public class DocModule : PluginModule,
    IDocModule
{
    private const string HalCommentFileName = "HalComments.json";
        
    public RestDocConfig RestDocConfig { get; private set; }
    public HalComments HalComments { get; private set; }
        
    public override void Initialize()
    {
        RestDocConfig = Context.Plugin.GetConfig<RestDocConfig>();
    }

    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IXmlCommentService, XmlCommentService>();
        services.AddSingleton<ITypeCommentService, XmlTypeCommentService>();

        services.AddScoped<IDocBuilder, DocBuilder>();
        services.AddScoped<IApiDocService, ApiDocService>();
        
        // Registers the configured document description implementations
        // with the dependency-injection container.
        foreach (Type descriptionType in RestDocConfig.DescriptionTypes)
        {
            services.AddScoped(typeof(IDocDescription), descriptionType);
        }
    }
    
    protected override async Task OnStartModuleAsync(IServiceProvider services)
    {
        string halCommentFilePath = Path.Combine(RestDocConfig.DescriptionDirectory, $"{HalCommentFileName}");
        if (!File.Exists(halCommentFilePath))
        {
            HalComments = new HalComments();
            return;
        }

        try
        {
            HalComments = await JsonSerializer.DeserializeAsync<HalComments>(File.OpenRead(halCommentFilePath),
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
        catch (Exception ex)
        {
            Context.Logger.LogError(ex, "Error reading HAL Comments from path: {PathName}", halCommentFilePath);
        }
    }
}