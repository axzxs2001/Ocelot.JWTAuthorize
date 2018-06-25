# Ocelot.JWTAuthorize
This library is used in the verification project when Ocelot is used as an API gateway. In the Ocelot project, the API project, the verification project, and the injection function can be used.

### 1. add the following sections to the appsetting. Json file for each project
```json
{
  "JwtAuthorize": {  
    "Secret": "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890",
    "Issuer": "ocelot",
    "Audience": "everyone",
    "PolicyName": "permission",
    "DefaultScheme": "Bearer",
    "IsHttps": false,
    "Expiration": 500
  }
}
```

### 2. API Project 

> PM> Install-Package Ocelot.JWTAuthorize
Startup.cs
```c#
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.JwtAuthorize;
namespace APISample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddApiJwtAuthorize((context) =>
            {
                return ValidatePermission(context);
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }

        bool ValidatePermission(HttpContext httpContext)
        {
           
            var _permissions = new List<Permission>() {
                new Permission { Name="admin", Predicate="Get", Url="/api/values" },
                new Permission { Name="admin,system", Predicate="Post", Url="/api/values" }
            };
            var questUrl = httpContext.Request.Path.Value.ToLower();
  
            if (_permissions != null && _permissions.Where(w => w.Url.Contains("}") ? questUrl.Contains(w.Url.Split('{')[0]) : w.Url.ToLower() == questUrl).Count() > 0)
            {
                var roles = httpContext.User.Claims.SingleOrDefault(s => s.Type == ClaimTypes.Role).Value;
                var roleArr = roles.Split(',');
            
                if (_permissions.Where(w => roleArr.Contains(w.Name) && w.Predicate.ToLower() == httpContext.Request.Method.ToLower()).Count() == 0)
                {               
                    httpContext.Response.Headers.Add("error", "no permission");
                    return false;
                }
            }
            else
            {
                return false;
            }
     
            if (DateTime.Parse(httpContext.User.Claims.SingleOrDefault(s => s.Type == ClaimTypes.Expiration).Value) >= DateTime.Now)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

```
API Controller
```C#
    [Authorize("permission")]
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : Controller
    {
    }
```
### 3. Authorize Project

> PM> Install-Package Ocelot.JWTAuthorize

startup.cs
```C#
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.JwtAuthorize;

namespace AuthorizeSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTokenJwtAuthorize();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }
 
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseMvc();
        }
    }
}

```
LoginController.cs
```C#
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
```

### 4. Ocelot Project

> PM> Install-Package Ocelot.JWTAuthorize

Startup.cs
```C#
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.DependencyInjection;
using Ocelot.JwtAuthorize;
using Ocelot.Middleware;

namespace OcelotSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

     
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOcelotJwtAuthorize();
            services.AddOcelot(Configuration);
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }
     
        public async void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseMvc();
            await app.UseOcelot();
        }
    }
}

```
