using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

            services.AddOcelotPolicyJwtBearer((context, jwtAuthorizationRequirement) =>
            {
                return ValidatePermission(context,jwtAuthorizationRequirement);
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

        bool ValidatePermission(HttpContext httpContext, JwtAuthorizationRequirement jwtAuthorizationRequirement)
        {
           
            var _permissions = new List<Permission>() {
                new Permission { Name="admin", Predicate="Get", Url="/api/values" },
                new Permission { Name="admin", Predicate="Post", Url="/api/values" }
            };
            var questUrl = httpContext.Request.Path.Value.ToLower();
            //权限中是否存在请求的url
            if (_permissions != null && _permissions.Where(w => w.Url.Contains("}") ? questUrl.Contains(w.Url.Split('{')[0]) : w.Url.ToLower() == questUrl).Count() > 0)
            {
                var roles = httpContext.User.Claims.SingleOrDefault(s => s.Type == jwtAuthorizationRequirement.ClaimType).Value;
                var roleArr = roles.Split(',');
                //验证权限
                if (_permissions.Where(w => roleArr.Contains(w.Name) && w.Predicate.ToLower() == httpContext.Request.Method.ToLower()).Count() == 0)
                {
                    //无权限从header中返回错误   
                    httpContext.Response.Headers.Add("error", "no permission");
                    return false;
                }
            }
            else
            {
                return true;
            }
            //判断过期时间
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
