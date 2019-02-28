using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Text;

namespace Ocelot.JwtAuthorize
{
    /// <summary>
    /// Ocelot.JwtAuthorize extension
    /// </summary>
    public static class JwtBearerExtension
    {
        /// <summary>
        /// In the Ocelot Project, the Startup. Cs class ConfigureServices method is called
        /// </summary>
        /// <param name="services">Service Collection</param>  
        /// <returns></returns>
        public static AuthenticationBuilder AddOcelotJwtAuthorize(this IServiceCollection services)
        {
            var configuration = services.SingleOrDefault(s => s.ServiceType.Name == typeof(IConfiguration).Name)?.ImplementationInstance as IConfiguration;
            if (configuration == null)
            {
                throw new OcelotJwtAuthorizeException("can't find JwtAuthorize section in appsetting.json");
            }
            var config = configuration.GetSection("JwtAuthorize");
            var keyByteArray = Encoding.ASCII.GetBytes(config["Secret"]);
            var signingKey = new SymmetricSecurityKey(keyByteArray);
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = config["Issuer"],
                ValidateAudience = true,
                ValidAudience = config["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = bool.Parse(config["RequireExpirationTime"])
            };
            return services.AddAuthentication(options =>
            {
                options.DefaultScheme = config["DefaultScheme"];
            })
             .AddJwtBearer(config["DefaultScheme"], opt =>
             {
                 opt.RequireHttpsMetadata = bool.Parse(config["IsHttps"]);
                 opt.TokenValidationParameters = tokenValidationParameters;
             });
        }

        /// <summary>
        /// In the API Project, the Startup. Cs class ConfigureServices method is called
        /// </summary>
        /// <param name="services">Service Collection</param>
        /// <param name="validatePermission">validate permission action</param>
        /// <returns></returns>
        public static AuthenticationBuilder AddApiJwtAuthorize(this IServiceCollection services, Func<HttpContext, bool> validatePermission)
        {
            var configuration = services.SingleOrDefault(s => s.ServiceType.Name == typeof(IConfiguration).Name)?.ImplementationInstance as IConfiguration;
            if (configuration == null)
            {
                throw new OcelotJwtAuthorizeException("can't find JwtAuthorize section in appsetting.json");
            }
            var config = configuration.GetSection("JwtAuthorize");

            var keyByteArray = Encoding.ASCII.GetBytes(config["Secret"]);
            var signingKey = new SymmetricSecurityKey(keyByteArray);
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = config["Issuer"],
                ValidateAudience = true,
                ValidAudience = config["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = bool.Parse(config["RequireExpirationTime"])
            };
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var permissionRequirement = new JwtAuthorizationRequirement(
                config["Issuer"],
                config["Audience"],
                signingCredentials
                );

            permissionRequirement.ValidatePermission = validatePermission;

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
             o.RequireHttpsMetadata = bool.Parse(config["IsHttps"]);
             o.TokenValidationParameters = tokenValidationParameters;
         });
        }
        /// <summary>
        /// In the Authorize Project, the Startup. Cs class ConfigureServices method is called
        /// </summary>
        /// <param name="services">Service Collection</param>
        /// <returns></returns>
        public static IServiceCollection AddTokenJwtAuthorize(this IServiceCollection services)
        {
            var configuration = services.SingleOrDefault(s => s.ServiceType.Name == typeof(IConfiguration).Name)?.ImplementationInstance as IConfiguration;
            if (configuration == null)
            {
                throw new OcelotJwtAuthorizeException("can't find JwtAuthorize section in appsetting.json");
            }
            var config = configuration.GetSection("JwtAuthorize");
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config["Secret"])), SecurityAlgorithms.HmacSha256);
            var permissionRequirement = new JwtAuthorizationRequirement(
               config["Issuer"],
               config["Audience"],
               signingCredentials
                );
            services.AddSingleton<ITokenBuilder, TokenBuilder>();
            return services.AddSingleton(permissionRequirement);
        }
    }
}
