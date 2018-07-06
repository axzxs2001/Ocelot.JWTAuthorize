using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Ocelot.JwtAuthorize
{
    /// <summary>
    /// customer permission policy handler
    /// </summary>
    public class PermissionHandler : AuthorizationHandler<JwtAuthorizationRequirement>
    {
        /// <summary>
        /// authentication scheme provider
        /// </summary>
        readonly IAuthenticationSchemeProvider _schemes;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="schemes"></param>
        public PermissionHandler(IAuthenticationSchemeProvider schemes)
        {
            _schemes = schemes;
        }
        /// <summary>
        /// handle requirement
        /// </summary>
        /// <param name="context">authorization handler context</param>
        /// <param name="jwtAuthorizationRequirement">jwt authorization requirement</param>
        /// <returns></returns>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, JwtAuthorizationRequirement jwtAuthorizationRequirement)
        {
            //convert AuthorizationHandlerContext to HttpContext
            var httpContext = context.Resource.GetType().GetProperty("HttpContext").GetValue(context.Resource) as HttpContext; 
        
            var handlers = httpContext.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
            foreach (var scheme in await _schemes.GetRequestHandlerSchemesAsync())
            {
                var handler = await handlers.GetHandlerAsync(httpContext, scheme.Name) as IAuthenticationRequestHandler;
                if (handler != null && await handler.HandleRequestAsync())
                {
                    context.Fail();
                    return;
                }
            }        
            var defaultAuthenticate = await _schemes.GetDefaultAuthenticateSchemeAsync();
            if (defaultAuthenticate != null)
            {
                var result = await httpContext.AuthenticateAsync(defaultAuthenticate.Name);          
                if (result?.Principal != null)
                {
                    httpContext.User = result.Principal;
                    var invockResult = jwtAuthorizationRequirement.ValidatePermission(httpContext);
                    if (invockResult)
                    {
                        context.Succeed(jwtAuthorizationRequirement);
                    }
                    else
                    {
                        context.Fail();
                    }
                }
                else
                {
                    context.Fail();
                }
            }
            else
            {
                context.Fail();
            }         
        }
    }
}
