using Kiss.Web.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Kiss.Components.Site.Web.Controllers
{
    class ConfigController : Controller
    {
        /// <summary>
        /// 获取图片，文件，视频上传配置信息
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="siteId">站点ID</param>
        /// <returns>
        /// {
        ///     code = 1,       //-1：参数不正确，-2：指定的站点不存在，-3：没有权限访问该地址
        ///     image = 
        ///     {
        ///         imageActionName = ""                //图片上传地址
        ///         imageAllowFiles = [],               //允许上传的图片格式
        ///         imageMaxSize = 0,                   //最大支持上传的图片大小
        ///         imageCompressBorder = 0,            
        ///         imageCompressEnable = false,        //是否压缩图片
        ///         imageInsertAlign = "none",          //插入图片时的对齐方式
        ///         imageUrlPrefix = ""                 //图片地址的前缀
        ///     },
        ///     file = 
        ///     {
        ///         fileActionName = ""                 //文件上传地址
        ///         fileMaxSize = 0,                    //最大支持上传的文件大小
        ///         fileAllowFiles = []                 //允许上传的附件格式
        ///     },
        ///     video = 
        ///     {
        ///         videoActionName = ""                 //视频上传地址
        ///         videoMaxSize = 0,                    //最大支持上传的视频大小
        ///         videoAllowFiles = []                 //允许上传的视频格式
        ///     }
        /// }
        /// </returns>
        /// leixu
        /// 2016年10月13日16:23:59
        object get(string siteId)
        {
            if (string.IsNullOrEmpty(siteId)) return new { code = -1, msg = "参数不正确" };

            var site = Site.Get(siteId);
            if (site == null) return new { code = -2, msg = "指定的站点不存在" };

            var relation = (from q in SiteUsers.CreateContext()
                            where q.UserId == jc.UserName && q.SiteId == site.Id
                            select q).FirstOrDefault();

            if (relation == null) return new { code = -3, msg = "没有权限访问该地址" };

            var host = jc.Context.Request.Url.Authority;

            //支援 NGINX 反向代理时 配置的外网地址
            if (!string.IsNullOrEmpty(jc.Context.Request.Headers["ORI_HOST"])) host = jc.Context.Request.Headers["ORI_HOST"];

            #region 构造允许的扩展名

            var image_exts = new List<string>();
            foreach (var item in Config.Instance.IMAGE_EXTS)
            {
                image_exts.Add(string.Format(".{0}", item.ToLowerInvariant()));
            }

            var file_exts = new List<string>();
            foreach (var item in Config.Instance.FILE_EXTS)
            {
                file_exts.Add(string.Format(".{0}", item.ToLowerInvariant()));
            }

            var video_exts = new List<string>();
            foreach (var item in Config.Instance.VIDEO_EXTS)
            {
                video_exts.Add(string.Format(".{0}", item.ToLowerInvariant()));
            }

            #endregion

            return new
            {
                code = 1,
                image = new
                {
                    imageActionName = string.Format("{0}/attachment/upload_image?siteId={1}", string.Format("{0}://{1}", jc.Context.Request.Url.Scheme, host), site.Id),
                    imageAllowFiles = image_exts,
                    imageMaxSize = Config.Instance.IMAGE_MAX_SIZE,
                    imageCompressBorder = 1600,
                    imageCompressEnable = false,
                    imageInsertAlign = "none",
                    imageUrlPrefix = string.Empty
                },
                file = new
                {
                    fileActionName = string.Format("{0}/attachment/upload_file?siteId={1}", string.Format("{0}://{1}", jc.Context.Request.Url.Scheme, host), site.Id),
                    fileMaxSize = Config.Instance.FILE_MAX_SIZE,
                    fileAllowFiles = file_exts
                },
                video = new
                {
                    videoActionName = string.Format("{0}/attachment/upload_video?siteId={1}", string.Format("{0}://{1}", jc.Context.Request.Url.Scheme, host), site.Id),
                    videoMaxSize = Config.Instance.VIDEO_MAX_SIZE,
                    videoAllowFiles = video_exts
                }
            };
        }
    }
}