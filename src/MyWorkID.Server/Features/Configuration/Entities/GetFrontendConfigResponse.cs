using Microsoft.Graph.Models;
using MyWorkID.Server.Options;

namespace MyWorkID.Server.Features.Configuration.Entities
{
    public class GetFrontendConfigResponse : FrontendOptions
    {
        /// <summary>
        /// The maximum risk level a user is allowed to dismiss themselves.
        /// </summary>
        public RiskLevel MaxDismissibleRiskLevel { get; set; } = RiskLevel.Low;

        public GetFrontendConfigResponse() { }

        public GetFrontendConfigResponse(
            FrontendOptions frontendOptions,
            UserRiskStateOptions userRiskStateOptions
        )
        {
            FrontendClientId = frontendOptions.FrontendClientId;
            TenantId = frontendOptions.TenantId;
            BackendClientId = frontendOptions.BackendClientId;
            CustomCssUrl = frontendOptions.CustomCssUrl;
            AppTitle = frontendOptions.AppTitle;
            FaviconUrl = frontendOptions.FaviconUrl;
            HelpUrl = frontendOptions.HelpUrl;
            MaxDismissibleRiskLevel = userRiskStateOptions.MaxDismissibleRiskLevel;
        }
    }
}
