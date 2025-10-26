using System;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace WebXTB
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            WebXTB.ModifyInMemory.ActivateMemoryPatching();
        }
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type");
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
        }
        protected void Application_EndRequest()
        {
            HttpRequestBase request = new HttpRequestWrapper(HttpContext.Current.Request);
            // Bỏ qua cho API grammar-correct hoặc SSE
            var path = request.Url.AbsolutePath.ToLower();
            if (path.Contains("/home/grammarcorrect"))
            {
                return; // không can thiệp
            }

            if (!request.IsAuthenticated)
            {
                if (request.IsAjaxRequest())
                {
                    Response.SuppressFormsAuthenticationRedirect = true;
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    Response.SuppressContent = true;
                }
                else
                {
                    Response.SuppressFormsAuthenticationRedirect = false;
                }
            }
        }
    }


}
