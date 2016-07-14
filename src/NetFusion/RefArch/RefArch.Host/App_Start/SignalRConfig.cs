using Microsoft.Owin;
using Owin;
using RefArch.Host.App_Start;

[assembly:OwinStartup(typeof(SignalRConfig))]
namespace RefArch.Host.App_Start
{
    public class SignalRConfig
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}