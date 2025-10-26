using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Windows.Forms;
using WebsiteMonitoring;

namespace TCatcherClient.Controllers
{
    public class AccountController : Controller
    {
        WebXTB.Support.Support sp = new WebXTB.Support.Support();
        // GET: Account
        public ActionResult Login()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            string permission = "";
            var db = csGetMongoData.GetDatabase(csGlobal.strMongoDatabase);
            string strPasswordAccount = csSupport.SHAEncrypt(password);

            var collection = db.GetCollection<Mongo_Account>("Account");
            var result = collection.AsQueryable().Where(p => p.Username == username.Trim().ToLower() && p.Password == strPasswordAccount).
                                                 Select(p => new { p.Permission });
            if (result.Count() > 0)
            {
                foreach (var row in result)
                {
                    permission = row.Permission;
                    break;
                }

                DateTime dtNow = DateTime.Now;
                string strLastLoginTime = dtNow.Day.ToString().PadLeft(2, '0') + "/" + dtNow.Month.ToString().PadLeft(2, '0') + "/" + dtNow.Year.ToString() + " " +
                    dtNow.Hour.ToString().PadLeft(2, '0') + ":" + dtNow.Minute.ToString().PadLeft(2, '0') + ":" + dtNow.Second.ToString().PadLeft(2, '0');
                var filter = Builders<BsonDocument>.Filter.Eq("Username", username.Trim().ToLower());
                var update = Builders<BsonDocument>.Update.Set("LastLoginTime", strLastLoginTime);
                csGetMongoData.UpdateOne(filter, update, csGlobal.strMongoDatabase, "Account");
               
                // 2. Tạo ClaimsIdentity
                var claims = new List<Claim>
            {
                new Claim("UserName", username),
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim("Permission", permission),
                new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "CustomProvider")

            };

                var identity = new ClaimsIdentity(claims, "CustomAuthentication");

                // 3. Đăng nhập người dùng
                var authManager = HttpContext.GetOwinContext().Authentication;
                authManager.SignOut(); // Đảm bảo các phiên trước bị đăng xuất
                authManager.SignIn(new Microsoft.Owin.Security.AuthenticationProperties
                {
                    IsPersistent = true // Tùy chọn duy trì đăng nhập sau khi đóng trình duyệt
                }, identity);

                // 4. Điều hướng đến trang khác sau khi đăng nhập thành công
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["Error"] = "Sai tên tài khoản hoặc mật khẩu";
                return View();
            }

        }

        public ActionResult Logout()
        {

            var authManager = HttpContext.GetOwinContext().Authentication;
            authManager.SignOut(); // Đảm bảo các phiên trước bị đăng xuất

            return RedirectToAction("Login", "Account");


        }
        
    }

}

