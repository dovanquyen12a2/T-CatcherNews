using Microsoft.AspNet.Identity;
using System.Security.Claims;

namespace TCatcherClient.Base
{
    public class CommonFn
    {

        /// <summary>
        /// Mã user đăng nhập vào hệ thống
        /// </summary>
        /// Create by: dvthang:19.05.2019
        public static string GetUserName()
        {
            return (System.Web.HttpContext.Current.User.Identity as ClaimsIdentity).FindFirstValue("UserName");
        }


        /// <summary>
        /// Tên người dùng đăng nhập
        /// </summary>
        /// Create by: dvthang:19.05.2019
        public static string GetPermission()
        {
            return (System.Web.HttpContext.Current.User.Identity as ClaimsIdentity).FindFirstValue("Permission");
        }

    }
}