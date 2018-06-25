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
