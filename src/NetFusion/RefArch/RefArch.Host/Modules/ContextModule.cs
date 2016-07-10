using Autofac;
using NetFusion.Bootstrap.Plugins;
using RefArch.Domain.Samples.WebApi;
using System;
using System.Web;

namespace RefArch.Host.Modules
{
    public class ContextModule : PluginModule
    {
        public override void RegisterComponents(ContainerBuilder builder)
        {
            builder.Register(c => {
                var principal = HttpContext.Current.User as ExampleClaimsPrincipal;
                
                if (principal == null)
                {
                    throw new InvalidOperationException(
                        "The current claim principal could not be accessed.");
                }

                LookupUserInfo(c, principal);
                return principal;
            })
            .AsSelf().InstancePerLifetimeScope();
        }

        private void LookupUserInfo(IComponentContext context, ExampleClaimsPrincipal principal)
        {
            var userInfoSrv = context.Resolve<IUserService>();
            var userInfo = userInfoSrv.GetUserInfo(principal.UserId);
            principal.SetUserContextInfo(userInfo);
        }
    }
}
