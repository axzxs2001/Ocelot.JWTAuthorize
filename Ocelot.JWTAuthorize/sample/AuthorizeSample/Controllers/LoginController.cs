using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocelot.JwtAuthorize;

namespace AuthorizeSample.Controllers
{
    [Route("auth/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        readonly ILogger<LoginController> _logger;
        readonly JwtAuthorizationRequirement _jwtAuthorizationRequirement;
        public LoginController(JwtAuthorizationRequirement jwtAuthorizationRequirement, ILogger<LoginController> logger)
        {
            _logger = logger;
            _jwtAuthorizationRequirement = jwtAuthorizationRequirement;

        }
        [HttpPost]
        public IActionResult Login([FromBody]LoginModel loginModel)
        {
            _logger.LogInformation($"{loginModel.UserName} login！");
            if (loginModel.UserName == "gsw" && loginModel.Password == "111111")
            {
                var claims = new Claim[] {
                    new Claim(ClaimTypes.Name, "gsw"),
                    new Claim(ClaimTypes.Role, "admin"),
                    new Claim(ClaimTypes.Expiration, DateTime.Now.AddSeconds(_jwtAuthorizationRequirement.Expiration.TotalSeconds).ToString())
                };
                var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
                identity.AddClaims(claims);
                var token = TokenBuilder.BuildJwtToken(claims, _jwtAuthorizationRequirement);
                _logger.LogInformation($"{loginModel.UserName} login success，and generate token return");
                return new JsonResult(new { Result = true, Data = token });
            }
            else
            {
                _logger.LogInformation($"{loginModel.UserName} login faile");
                return new JsonResult(new
                {
                    Result = false,
                    Message = "Authentication Failure"
                });
            }
        }
    }
}
