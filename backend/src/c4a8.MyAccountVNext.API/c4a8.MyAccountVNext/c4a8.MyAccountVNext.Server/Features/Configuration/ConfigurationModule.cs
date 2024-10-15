﻿using c4a8.MyAccountVNext.Server.Common;

namespace c4a8.MyAccountVNext.Server.Features.Configuration
{
    public class ConfigurationModule : IModule
    {
        public static void ConfigureServices(IServiceCollection services, IConfigurationManager configurationManager, IWebHostEnvironment Environment)
        {
            services.Configure<FrontendOptions>(configurationManager.GetSection("Frontend"));
        }
    }
}
