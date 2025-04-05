using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MatchMate.Startup))]
namespace MatchMate
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            app.MapSignalR();
        }
    }
}