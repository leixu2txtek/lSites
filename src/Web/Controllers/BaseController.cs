using Kiss.Web;
using Kiss.Web.Mvc;
using Kiss.Web.Utils;

namespace Kiss.Components.Site.Web.Controllers
{
    class BaseController : Controller
    {
        public BaseController()
        {
            BeforeActionExecute += BaseController_BeforeActionExecute;
        }

        private void BaseController_BeforeActionExecute(object sender, BeforeActionExecuteEventArgs e)
        {
            JContext jc = e.JContext;

            if (jc == null)
            {
                //服务器错误
                ResponseUtil.OutputJson(httpContext.Response, new { code = 500, msg = "不合法请求" });
                e.PreventDefault = true;
                return;
            }

            if (!jc.IsAuth)
            {
                //权限验证失败
                ResponseUtil.OutputJson(httpContext.Response, new { code = 403, msg = "没有权限访问" });
                e.PreventDefault = true;
                return;
            }

            //TODO
        }
    }
}