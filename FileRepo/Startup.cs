using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FileRepo.Startup))]
namespace FileRepo
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
