
namespace Ocelot.JWTAuthorize
{
    /// <summary>
    /// 用户或角色或其他凭据实体
    /// </summary>
    public interface IPermission
    {
        /// <summary>
        /// 用户或角色或其他凭据名称
        /// </summary>
         string Name
        { get; set; }
        /// <summary>
        /// 请求Url
        /// </summary>
         string Url
        { get; set; }

        /// <summary>
        /// 请求谓词
        /// </summary>
        string Predicate
        { get; set; }
    }
}
