# Ocelot.JWTAuthorize
This library is used in the verification project when Ocelot is used as an API gateway. In the Ocelot project, the API project, the verification project, and the injection function can be used.

## 1. appsetting.json

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
