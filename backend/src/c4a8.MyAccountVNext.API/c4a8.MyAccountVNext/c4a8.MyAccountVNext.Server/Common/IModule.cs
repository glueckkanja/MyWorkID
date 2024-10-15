namespace c4a8.MyAccountVNext.Server.Common
{
    public interface IModule
    {
        static abstract void ConfigureServices(IServiceCollection services, IConfigurationManager configurationManager, IWebHostEnvironment Environment);
    }
}
