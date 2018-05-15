using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(IdentityMvcWeb.Startup))]
namespace IdentityMvcWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
