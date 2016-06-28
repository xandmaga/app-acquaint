using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(app_acquaintService.Startup))]

namespace app_acquaintService
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMobileApp(app);
        }
    }
}