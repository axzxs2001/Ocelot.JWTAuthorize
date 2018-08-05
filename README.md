# Ocelot.JWTAuthorize
<img src="https://github.com/axzxs2001/Ocelot.JWTAuthorize/blob/master/Ocelot.JWTAuthorize/Ocelot.JWTAuthorize/githublogo.png" alt="GitHub" title="Ocelot.JwtAuthorize" width="360" height="200" />

[![NuGet Badge](https://buildstats.info/nuget/Ocelot.JwtAuthorize)](https://www.nuget.org/packages/Ocelot.JwtAuthorize/)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/axzxs2001/Ocelot.JWTAuthorize/blob/master/LICENSE)


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
Startup.cs,In ConfigureServices method
```c#
services.AddApiJwtAuthorize((context) =>
{    
    return true;//validate permissions return(permit) true or false(denied)
});
 
```
API Controller,  "permission" is PolicyName of appsettion.json
```C#
[Authorize("permission")]
public class ValuesController : Controller
```
### 3. Authorize Project

>#### PM>Install-Package Ocelot.JWTAuthorize
startup.cs,In ConfigureServices method
```C#
services.AddTokenJwtAuthorize();
```
LoginController.cs  _tokenBuilder is dependency injection in AddTokenJwtAuthorize,so it's ITokenBuilder
```C#
[HttpPost]
public IActionResult Login([FromBody]LoginModel loginModel)
{        
        if (loginModel.UserName == "gsw" && loginModel.Password == "111111")
        {
             var claims = new Claim[] {
                 new Claim(ClaimTypes.Name, "gsw"),
                 new Claim(ClaimTypes.Role, "admin")                  
             };     
             //DateTime.Now.AddSeconds(1200) is expiration time
             var ip =HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString();
             var token = _tokenBuilder.BuildJwtToken(claims,ip, DateTime.UtcNow, DateTime.Now.AddSeconds(1200));      
             return new JsonResult(new { Result = true, Data = token });
         }
         else
         {
             return new JsonResult(new
             {
                 Result = false,
                 Message = "Authentication Failure"
             });
         }
 }
```
### 4. Ocelot Project

>#### PM>Install-Package Ocelot.JWTAuthorize
Startup.cs,In ConfigureServices method
```C#
services.AddOcelotJwtAuthorize();
```

## TODO
Token Invalid
