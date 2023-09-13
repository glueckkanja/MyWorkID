using c4a8.MyAccountVNext.API.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace c4a8.MyAccountVNext.API.Controllers
{
    [AllowAnonymous]
    [Route("[controller]")]
    [ApiController]
    public class FrontendOptionsController : ControllerBase
    {
        private readonly FrontendOptions _frontendOptions;

        public FrontendOptionsController(IOptions<FrontendOptions> frontendOptions)
        {
            _frontendOptions = frontendOptions.Value;
        }

        [HttpGet("")]
        public FrontendOptions GetFrontendOptions()
        {
            return _frontendOptions;
        }
    }
}
