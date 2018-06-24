using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Ocelot.JWTAuthorize
{
    /// <summary>
    /// Ocelot下JwtBearer扩展
    /// </summary>
    public static class JWTBearerExtension
    {
        /// <summary>
        /// 注入Ocelot下JwtBearer，在ocelot网关的Startup的ConfigureServices中调用
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="issuer">发行人</param>
        /// <param name="audience">订阅人</param>
        /// <param name="secret">密钥</param>
        /// <param name="defaultScheme">默认架构</param>
        /// <param name="isHttps">是否https</param>
        /// <returns></returns>
        public static AuthenticationBuilder AddOcelotJwtBearer(this IServiceCollection services)
        {
            var configuration = services.SingleOrDefault(s => s.ServiceType.Name == typeof(IConfiguration).Name)?.ImplementationInstance as IConfiguration;
            if (configuration == null)
            {
                throw new OcelotJwtAuthoizeException("can't find JWTAuthorize section in appsetting.json");
            }
            var config = configuration.GetSection("JWTAuthorize");
            var keyByteArray = Encoding.ASCII.GetBytes(config["Secret"]);
            var signingKey = new SymmetricSecurityKey(keyByteArray);
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = config["Issuer"],//发行人
                ValidateAudience = true,
                ValidAudience = config["Audience"],//订阅人
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = true,
            };
            return services.AddAuthentication(options =>
            {
                options.DefaultScheme = config["DefaultScheme"];
            })
             .AddJwtBearer(config["DefaultScheme"], opt =>
             {
                 //不使用https
                 opt.RequireHttpsMetadata = bool.Parse(config["IsHttps"]);
                 opt.TokenValidationParameters = tokenValidationParameters;
             });
        }

        /// <summary>
        /// 注入Ocelot jwt策略，在业务API应用中的Startup的ConfigureServices调用
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="issuer">发行人</param>
        /// <param name="audience">订阅人</param>
        /// <param name="secret">密钥</param>
        /// <param name="defaultScheme">默认架构</param>
        /// <param name="policyName">自定义策略名称</param>
        /// <param name="deniedUrl">拒绝路由</param>
        /// <param name="isHttps">是否https</param>
        /// <returns></returns>
        public static AuthenticationBuilder AddOcelotPolicyJwtBearer(this IServiceCollection services, Func<HttpContext, JWTAuthorizationRequirement,bool> action)
        {
            var configuration = services.SingleOrDefault(s => s.ServiceType.Name == typeof(IConfiguration).Name)?.ImplementationInstance as IConfiguration;
            if (configuration == null)
            {
                throw new OcelotJwtAuthoizeException("can't find JWTAuthorize section in appsetting.json");
            }
            var config = configuration.GetSection("JWTAuthorize");

            var keyByteArray = Encoding.ASCII.GetBytes(config["Secret"]);
            var signingKey = new SymmetricSecurityKey(keyByteArray);
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = config["Issuer"],//发行人
                ValidateAudience = true,
                ValidAudience = config["Audience"],//订阅人
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = true,

            };
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            //如果第三个参数，是ClaimTypes.Role，上面集合的每个元素的Name为角色名称，如果ClaimTypes.Name，即上面集合的每个元素的Name为用户名
            var permissionRequirement = new JWTAuthorizationRequirement(
               config["DeniedUrl"],
                ClaimTypes.Role,
                config["Issuer"],
                config["Audience"],
                signingCredentials,
                expiration: TimeSpan.FromMinutes(double.Parse(config["Expiration"]))
                );


            permissionRequirement.Func = action;
            //注入授权Handler
            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
            services.AddSingleton(permissionRequirement);
            return services.AddAuthorization(options =>
            {
                options.AddPolicy(config["PolicyName"],
                          policy => policy.Requirements.Add(permissionRequirement));

            })
         .AddAuthentication(options =>
         {
             options.DefaultScheme = config["DefaultScheme"];
         })
         .AddJwtBearer(config["DefaultScheme"], o =>
         {
             //不使用https
             o.RequireHttpsMetadata = bool.Parse(config["IsHttps"]);
             o.TokenValidationParameters = tokenValidationParameters;
         });
        }
        /// <summary>
        /// 注放Token生成器参数，在token生成项目的Startup的ConfigureServices中使用
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="issuer">发行人</param>
        /// <param name="audience">订阅人</param>
        /// <param name="secret">密钥</param>
        /// <param name="deniedUrl">拒绝路由</param>
        /// <returns></returns>
        public static IServiceCollection AddJTokenBuild(this IServiceCollection services)
        {
            var configuration = services.SingleOrDefault(s => s.ServiceType.Name == typeof(IConfiguration).Name)?.ImplementationInstance as IConfiguration;
            if (configuration == null)
            {
                throw new OcelotJwtAuthoizeException("can't find JWTAuthorize section in appsetting.json");
            }
            var config = configuration.GetSection("JWTAuthorize");
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config["Secret"])), SecurityAlgorithms.HmacSha256);
            //如果第三个参数，是ClaimTypes.Role，上面集合的每个元素的Name为角色名称，如果ClaimTypes.Name，即上面集合的每个元素的Name为用户名
            var permissionRequirement = new JWTAuthorizationRequirement(
               config["DeniedUrl"],
               ClaimTypes.Role,
               config["Issuer"],
               config["Audience"],
               signingCredentials,
               expiration: TimeSpan.FromMinutes(double.Parse(config["Expiration"]))
                );
            return services.AddSingleton(permissionRequirement);

        }

    }
}
