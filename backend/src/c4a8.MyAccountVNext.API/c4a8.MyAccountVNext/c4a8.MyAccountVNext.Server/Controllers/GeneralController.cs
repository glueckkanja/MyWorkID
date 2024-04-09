﻿using c4a8.MyAccountVNext.Server.Hubs;
using c4a8.MyAccountVNext.Server.Options;
using c4a8.MyAccountVNext.Server.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace c4a8.MyAccountVNext.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeneralController : ControllerBase
    {
        private readonly FrontendOptions _frontendOptions;

        public GeneralController(IOptions<FrontendOptions> frontendOptions)
        {
            _frontendOptions = frontendOptions.Value;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<string>> LandingPage()
        {
            return Ok("Healthy");
        }

        [AllowAnonymous]
        [HttpGet("config/frontend")]
        public ActionResult<FrontendOptions> GetFrontendConfig()
        {
            return Ok(_frontendOptions);
        }   
    }
}
