# Ocelot.JWTAuthorize
<img src="https://github.com/axzxs2001/Ocelot.JWTAuthorize/blob/master/Ocelot.JWTAuthorize/Ocelot.JWTAuthorize/githublogo.png" alt="GitHub" title="Ocelot.JwtAuthorize" width="360" height="200" />

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
    "RequireExpirationTime": true
  }
}
```

### 2. API Project 

>#### PM>Install-Package Ocelot.JWTAuthorize
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
               //validate permissions return(permit) true or false(denied)
                return true;
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

>#### PM>Install-Package Ocelot.JWTAuthorize
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
namespace AuthorizeSample.Controllers
{
    [Route("auth/[controller]")]
    [ApiController]
    public class LoginController : Controller
    {
        readonly ILogger<LoginController> _logger;
        readonly ITokenBuilder _tokenBuilder;
        public LoginController(ITokenBuilder tokenBuilder, ILogger<LoginController> logger)
        {
            _logger = logger;
            _tokenBuilder = tokenBuilder;

        }
        [HttpPost]
        public IActionResult Login([FromBody]LoginModel loginModel)
        {
            _logger.LogInformation($"{loginModel.UserName} login！");
            if (loginModel.UserName == "gsw" && loginModel.Password == "111111")
            {
                var claims = new Claim[] {
                    new Claim(ClaimTypes.Name, "gsw"),
                    new Claim(ClaimTypes.Role, "admin")                  
                };     
                //DateTime.Now.AddSeconds(1200) is expiration time
                var token = _tokenBuilder.BuildJwtToken(claims, DateTime.Now.AddSeconds(1200));
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

```

### 4. Ocelot Project

>#### PM>Install-Package Ocelot.JWTAuthorize
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
