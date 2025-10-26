using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TCatcherClient.Startup))]
namespace TCatcherClient
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
