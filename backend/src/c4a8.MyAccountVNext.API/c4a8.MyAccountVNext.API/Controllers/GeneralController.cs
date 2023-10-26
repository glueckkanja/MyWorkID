using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace c4a8.MyAccountVNext.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeneralController : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet]
        public ActionResult<string> LandingPage()
        {
            return Ok("Healthy");
        }
    }
}
