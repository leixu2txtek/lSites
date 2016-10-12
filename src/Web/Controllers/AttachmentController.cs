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
    /// 附件存储控制器
    /// </summary>
    class AttachmentController : Controller
    {
        public AttachmentController()
        {
            BeforeActionExecute += AttachmentController_BeforeActionExecute;
        }

        private void AttachmentController_BeforeActionExecute(object sender, BeforeActionExecuteEventArgs e)
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
        /// 上传图片
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <returns>
        /// {
        ///     code = 1,           //-1：上传的图片文件不能为空，-2：只能上传 XX 图片文件，-3：图片文件的大小不能超过 XXMB，-4：上传图片失败，请联系管理员
        ///     msg = "上传成功",
        ///     url = "文件地址"
        /// }
        /// </returns>
        /// leixu
        /// 2016年8月24日11:27:40
        [HttpPost]
        object upload_image()
        {
            var site = (Site)jc["site"];

            #region 校验图片

            if (jc.Context.Request.Files.Count == 0) return new { code = -1, msg = "上传的图片文件不能为空" };

            var file = jc.Context.Request.Files[0];
            if (file == null) return new { code = -1, msg = "上传的图片文件不能为空" };

            var extension = Path.GetExtension(file.FileName).Substring(1).ToLowerInvariant();

            if (!Config.Instance.IMAGE_EXTS.Contains(extension.ToUpperInvariant())) return new { code = -2, msg = string.Format("只能上传 {0} 图片文件", StringUtil.CollectionToDelimitedString(Config.Instance.IMAGE_EXTS, "、", string.Empty)) };
            if (file.InputStream.Length > Config.Instance.IMAGE_MAX_SIZE) return new { code = -3, msg = string.Format("图片文件的大小不能超过 {0}MB", (Config.Instance.IMAGE_MAX_SIZE / (1024 * 1024))) };

            //TODO 校验图片是否是一个图片

            #endregion

            #region 存储图片文件

            var directory = Path.Combine(Config.Instance.IMAGE_PATH, DateTime.Now.ToString("yyyyMMdd"));
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
            if (!string.IsNullOrEmpty(jc.Context.Request.Headers["ORI_HOST"]))
            {
                host = jc.Context.Request.Headers["ORI_HOST"];
            }

            #endregion

            return new
            {
                code = 1,
                msg = "上传成功",
                url = string.Format("{0}/open/read?siteId={1}&url={2}-{3}", string.Format("{0}://{1}", jc.Context.Request.Url.Scheme, host), site.Id, DateTime.Now.ToString("yyyyMMdd"), name)
            };
        }

        /// <summary>
        /// 上传附件
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <returns>
        /// {
        ///     code = 1,           //-1：上传的文件不能为空，-2：只能上传 XX 文件，-3：文件的大小不能超过 XXMB，-4：上传文件失败，请联系管理员
        ///     msg = "上传成功",
        ///     url = "文件地址"
        /// }
        /// </returns>
        /// leixu
        /// 2016年10月12日14:55:23
        [HttpPost]
        object upload_file()
        {
            var site = (Site)jc["site"];

            #region 校验文件

            if (jc.Context.Request.Files.Count == 0) return new { code = -1, msg = "上传的文件不能为空" };

            var file = jc.Context.Request.Files[0];
            if (file == null) return new { code = -1, msg = "上传的文件不能为空" };

            var fileName = file.FileName;
            var extension = Path.GetExtension(fileName).Substring(1).ToLowerInvariant();

            //替换不正确的文件名
            foreach (var item in Path.GetInvalidFileNameChars()) fileName = fileName.Replace(item, new char());

            if (!Config.Instance.FILE_EXTS.Contains(extension.ToUpperInvariant())) return new { code = -2, msg = string.Format("只能上传 {0} 文件", StringUtil.CollectionToDelimitedString(Config.Instance.FILE_EXTS, "、", string.Empty)) };
            if (file.InputStream.Length > Config.Instance.FILE_MAX_SIZE) return new { code = -3, msg = string.Format("文件的大小不能超过 {0}MB", (Config.Instance.FILE_MAX_SIZE / (1024 * 1024))) };

            #endregion

            #region 存储文件

            var directory = Path.Combine(Config.Instance.FILE_PATH, DateTime.Now.ToString("yyyyMMdd"));
            var name = string.Format("{0}.{1}", StringUtil.UniqueId(), extension);

            try
            {
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                file.SaveAs(Path.Combine(directory, name));
            }
            catch (Exception ex)
            {
                logger.Error(ExceptionUtil.WriteException(ex));

                return new { code = -4, msg = "上传文件失败，请联系管理员" };
            }

            #endregion

            #region 设定文件的地址

            var host = jc.Context.Request.Url.Authority;

            //支援 NGINX 反向代理时 配置的外网地址
            if (!string.IsNullOrEmpty(jc.Context.Request.Headers["ORI_HOST"])) host = jc.Context.Request.Headers["ORI_HOST"];

            #endregion

            return new
            {
                code = 1,
                msg = "上传成功",
                url = string.Format("{0}/open/file?siteId={1}&url={2}-{3}&name={4}", string.Format("{0}://{1}", jc.Context.Request.Url.Scheme, host), site.Id, DateTime.Now.ToString("yyyyMMdd"), name, fileName)
            };
        }

        /// <summary>
        /// 上传视频
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <returns>
        /// {
        ///     code = 1,           //-1：上传的视频不能为空，-2：只能上传 XX 视频，-3：视频的大小不能超过 XXMB，-4：上传视频失败，请联系管理员
        ///     msg = "上传成功",
        ///     url = "文件地址"
        /// }
        /// </returns>
        /// leixu
        /// 2016年10月12日14:55:26
        [HttpPost]
        object upload_video()
        {
            var site = (Site)jc["site"];

            #region 校验文件

            if (jc.Context.Request.Files.Count == 0) return new { code = -1, msg = "上传的视频不能为空" };

            var file = jc.Context.Request.Files[0];
            if (file == null) return new { code = -1, msg = "上传的视频不能为空" };

            var extension = Path.GetExtension(file.FileName).Substring(1).ToLowerInvariant();

            if (!Config.Instance.VIDEO_EXTS.Contains(extension.ToUpperInvariant())) return new { code = -2, msg = string.Format("只能上传 {0} 视频", StringUtil.CollectionToDelimitedString(Config.Instance.VIDEO_EXTS, "、", string.Empty)) };
            if (file.InputStream.Length > Config.Instance.VIDEO_MAX_SIZE) return new { code = -3, msg = string.Format("视频的大小不能超过 {0}MB", (Config.Instance.VIDEO_MAX_SIZE / (1024 * 1024))) };

            #endregion

            #region 存储文件

            var directory = Path.Combine(Config.Instance.VIDEO_PATH, DateTime.Now.ToString("yyyyMMdd"));
            var name = string.Format("{0}.{1}", StringUtil.UniqueId(), extension);

            try
            {
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                file.SaveAs(Path.Combine(directory, name));
            }
            catch (Exception ex)
            {
                logger.Error(ExceptionUtil.WriteException(ex));

                return new { code = -4, msg = "上传视频失败，请联系管理员" };
            }

            #endregion

            #region 设定文件的地址

            var host = jc.Context.Request.Url.Authority;

            //支援 NGINX 反向代理时 配置的外网地址
            if (!string.IsNullOrEmpty(jc.Context.Request.Headers["ORI_HOST"])) host = jc.Context.Request.Headers["ORI_HOST"];

            #endregion

            return new
            {
                code = 1,
                msg = "上传成功",
                url = string.Format("{0}/open/video?siteId={1}&url={2}-{3}", string.Format("{0}://{1}", jc.Context.Request.Url.Scheme, host), site.Id, DateTime.Now.ToString("yyyyMMdd"), name)
            };
        }
    }
}