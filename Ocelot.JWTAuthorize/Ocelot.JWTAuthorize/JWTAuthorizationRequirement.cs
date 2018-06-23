using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;

namespace Ocelot.JWTAuthorize
{
    /// <summary>
    /// 必要参数类
    /// </summary>
    public class JWTAuthorizationRequirement : IAuthorizationRequirement
    {
        IEnumerable<IPermission> _permissions;
        public JWTAuthorizationRequirement(IEnumerable<IPermission> permissions)
        {
            _permissions = permissions;
        }

        public bool Validate(HttpContext  httpContext)
        {

            return true;
        }

        /// <summary>
        /// 无权限action
        /// </summary>
        public string DeniedAction { get; set; }

        /// <summary>
        /// 认证授权类型
        /// </summary>
        public string ClaimType { internal get; set; }
        /// <summary>
        /// 请求路径
        /// </summary>
        public string LoginPath { get; set; }
        /// <summary>
        /// 发行人
        /// </summary>
        public string Issuer { get; set; }
        /// <summary>
        /// 订阅人
        /// </summary>
        public string Audience { get; set; }
        /// <summary>
        /// 过期时间
        /// </summary>
        public TimeSpan Expiration { get; set; }
        /// <summary>
        /// 签名验证
        /// </summary>
        public SigningCredentials SigningCredentials { get; set; }


        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="deniedAction">拒约请求的url</param>      
        /// <param name="claimType">声明类型</param>
        /// <param name="issuer">发行人</param>
        /// <param name="audience">订阅人</param>
        /// <param name="signingCredentials">签名验证实体</param>
        public JWTAuthorizationRequirement(string deniedAction, string claimType, string issuer, string audience, SigningCredentials signingCredentials, TimeSpan expiration)
        {
            ClaimType = claimType;
            DeniedAction = deniedAction;    
            Issuer = issuer;
            Audience = audience;
            Expiration = expiration;
            SigningCredentials = signingCredentials;
        }
    }
}
