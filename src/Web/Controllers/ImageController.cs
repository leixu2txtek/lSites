using Kiss.Utils;
using Kiss.Web;
using Kiss.Web.Mvc;
using Kiss.Web.Utils;
using System;
using System.IO;
using System.Linq;

namespace Kiss.Components.Site.Web.Controllers
{
    /// <summary>
    /// 图片存储控制器
    /// </summary>
    class ImageController : Controller
    {
        public ImageController()
        {
            BeforeActionExecute += ImageController_BeforeActionExecute;
        }

        private void ImageController_BeforeActionExecute(object sender, BeforeActionExecuteEventArgs e)
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

            #region 校验站点信息

            if (string.IsNullOrEmpty(jc.Params["siteId"]))
            {
                ResponseUtil.OutputJson(httpContext.Response, new { code = 201, msg = "参数列表不正确，缺少SiteId参数" });
                e.PreventDefault = true;
                return;
            }

            var site = Site.Get(jc.Params["siteId"]);

            if (site == null)
            {
                ResponseUtil.OutputJson(httpContext.Response, new { code = 202, msg = "指定的站点不存在" });
                e.PreventDefault = true;
                return;
            }

            #endregion

            #region 校验用户对站点的权限

            var relation = (from q in SiteUsers.CreateContext()
                            where q.UserId == jc.UserName && q.SiteId == site.Id
                            select q).FirstOrDefault();

            if (relation == null)
            {
                ResponseUtil.OutputJson(httpContext.Response, new { code = 403, msg = "没有权限访问" });
                e.PreventDefault = true;
                return;
            }

            #endregion

            jc["site"] = site;
        }

        /// <summary>
        /// 上传单个图片文件
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <returns>
        /// {
        ///     code = 1,           //-1：上传的图片文件不能为空，-2：只能上传 JPEG、JPG、GIF、PNG 图片文件，-3：图片文件的大小不能超过 5MB，-4：上传图片失败，请联系管理员
        ///     msg = "上传成功",
        ///     url = "文件地址"
        /// }
        /// </returns>
        /// leixu
        /// 2016年8月24日11:27:40
        object upload()
        {
            var site = (Site)jc["site"];

            #region 校验图片

            if (jc.Context.Request.Files.Count == 0) return new { code = -1, msg = "上传的图片文件不能为空" };

            var file = jc.Context.Request.Files[0];
            if (file == null) return new { code = -1, msg = "上传的图片文件不能为空" };

            var extension = Path.GetExtension(file.FileName).Substring(1).ToLowerInvariant();

            if (!Config.IMAGE_EXTS.Contains(extension.ToUpperInvariant())) return new { code = -2, msg = "只能上传 JPEG、JPG、GIF、PNG 图片文件" };
            if (file.InputStream.Length > Config.IMAGE_MAX_SIZE) return new { code = -3, msg = "图片文件的大小不能超过 5MB" };

            //TODO 校验图片是否是一个图片

            #endregion

            #region 存储图片文件

            var directory = Path.Combine(Config.IMAGE_PATH, DateTime.Now.ToString("yyyyMMdd"));
            var name = string.Format("{0}.{1}", StringUtil.UniqueId(), extension);

            try
            {
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                file.SaveAs(Path.Combine(directory, name));
            }
            catch (Exception ex)
            {
                logger.Error(ExceptionUtil.WriteException(ex));

                return new { code = -4, msg = "上传图片失败，请联系管理员" };
            }

            #endregion

            #region 设定图片的地址

            var host = jc.Context.Request.Url.Authority;

            //支援 NGINX 反向代理时 配置的外网地址
            if (!string.IsNullOrEmpty(jc.Context.Request.Headers["Host"]))
            {
                host = jc.Context.Request.Headers["Host"];
            }

            #endregion

            return new
            {
                code = 1,
                msg = "上传成功",
                url = string.Format("{0}/open/read?siteId={1}&url={2}-{3}", string.Format("{0}://{1}", jc.Context.Request.Url.Scheme, host), site.Id, DateTime.Now.ToString("yyyyMMdd"), name)
            };
        }
    }
}