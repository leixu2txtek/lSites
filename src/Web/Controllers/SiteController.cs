﻿using Kiss.Utils;
using Kiss.Web;
using Kiss.Web.Mvc;
using Kiss.Web.Utils;
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kiss.Components.Site.Web.Controllers
{
    /// <summary>
    /// 站点管理的控制器接口
    /// </summary>
    class SiteController : Controller
    {
        public SiteController()
        {
            BeforeActionExecute += SiteController_BeforeActionExecute;
        }

        private void SiteController_BeforeActionExecute(object sender, BeforeActionExecuteEventArgs e)
        {
            JContext jc = e.JContext;

            if (jc == null)
            {
                //服务器错误
                ResponseUtil.OutputJson(httpContext.Response, new { code = 500, msg = "不合法请求" });
                e.PreventDefault = true;
                return;
            }

            //只有管理员角色才能访问该控制器下的接口
            if (!jc.IsAuth || !jc.User.IsInRole("admin"))
            {
                //权限验证失败
                ResponseUtil.OutputJson(httpContext.Response, new { code = 403, msg = "没有权限访问" });
                e.PreventDefault = true;
                return;
            }
        }

        #region 增加 & 修改站点

        /// <summary>
        /// 根据ID获取站点的详细信息
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="id">站点ID</param>
        /// <returns>
        /// {
        ///     code = 1,
        ///     data = 
        ///     {
        ///         id = "",                //站点ID
        ///         title = "",             //站点标题
        ///         logo = "",              //站点logo地址
        ///         domain = "",            //站点域名
        ///         key_words = "",         //站点关键字（SEO）
        ///         description = "",       //站点描述（SEO）
        ///         theme = "",             //站点主题
        ///         sort_order = "",        //站点排序
        ///         need_audit_post = false //是否需要审核文章
        ///     }
        /// }
        /// </returns>
        /// leixu
        /// 2016年6月29日19:45:03
        [HttpPost]
        object detail(string id)
        {
            Site site = Site.Get(id);

            if (site == null) return new { code = -1, msg = "指定的站点不存在" };

            return new
            {
                code = 1,
                data = new
                {
                    id = site.Id,
                    title = site.Title,
                    domain = site.Domain,
                    logo = site.Logo,
                    ico = site.ICO,
                    key_words = site.KeyWords,
                    description = site.Description,
                    theme = site.Theme,
                    sort_order = site.SortOrder,
                    need_audit_post = site.NeedAuditPost ? 1 : 0
                }
            };
        }

        /// <summary>
        /// 添加单个站点
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="id">站点ID</param>
        /// <param name="title">站点标题</param>
        /// <param name="domain">站点域名</param>
        /// <param name="keyWords">站点关键字</param>
        /// <param name="description">站点描述</param>
        /// <param name="theme">站点主题，默认：default</param>
        /// <param name="sortOrder">站点序号</param>
        /// <param name="needAuditPost">是否需要审核文章</param>
        /// <returns>
        /// {
        ///     code = 1,       //-1：站点名称不能为空，-2：站点域名不能为空，-3：站点域名支持大小写英文以及点号，下划线，-4：站点标题不能超过100个字符，-5：站点域名不能超过100个字符，-6：站点关键字不能超过500个字符，-7：站点描述不能超过1000个字符，-8：站点的主题名不能超过20个字符，-9：LOGO文件只能是 JPG、GIF、PNG 图片文件，-10：LOGO文件的大小不能超过 1MB，-11：LOGO 存储失败，请联系管理员，-12：ICO文件只能是 ICO 图片文件，-13：ICO文件的大小不能超过 1MB，-14：ICO 图标存储失败，请联系管理员，-15：已经存在相同的站点名称，请更换其他站点名称，-16：已经存在相同的站点域名，请更换其他站点名称
        ///     msg = ""        //1：保存成功
        /// }
        /// </returns>
        /// leixu
        /// 2016年6月29日19:55:25
        [HttpPost]
        object save(string id, string title, string domain, string keyWords, string description, string theme, int sortOrder, int needAuditPost)
        {
            #region 校验参数 & 校验参数的长度

            if (string.IsNullOrWhiteSpace(title)) return new { code = -1, msg = "站点名称不能为空" };
            if (string.IsNullOrWhiteSpace(domain)) return new { code = -2, msg = "站点域名不能为空" };

            Regex regex = new Regex(@"^[\w\.]+$");

            if (!regex.IsMatch(domain)) return new { code = -3, msg = "站点域名支持大小写英文以及点号，下划线" };

            title = title.Trim();
            domain = domain.Trim();
            keyWords = string.IsNullOrWhiteSpace(keyWords) ? string.Empty : keyWords.Trim();
            description = string.IsNullOrWhiteSpace(description) ? string.Empty : description.Trim();
            theme = string.IsNullOrEmpty(theme) ? "default" : theme.Trim();

            if (title.Length > 100) return new { code = -4, msg = "站点标题不能超过100个字符" };
            if (domain.Length > 100) return new { code = -5, msg = "站点域名不能超过100个字符" };
            if (keyWords.Length > 500) return new { code = -6, msg = "站点关键字不能超过500个字符" };
            if (description.Length > 1000) return new { code = -7, msg = "站点描述不能超过1000个字符" };
            if (theme.Length > 20) return new { code = -8, msg = "站点的主题名不能超过20个字符" };

            #endregion

            #region 校验LOGO文件

            string logo = string.Empty;

            try
            {
                if (jc.Context.Request.Files.Count > 0)
                {
                    var file = jc.Context.Request.Files["logo"];

                    var extension = Path.GetExtension(file.FileName);
                    if (string.IsNullOrEmpty(extension)) return new { code = -9, msg = "LOGO文件只能是 JPG、GIF、PNG 图片文件" };

                    extension = extension.Substring(1).ToLowerInvariant();

                    if (extension != "jpg" && extension != "gif" && extension != "png") return new { code = -9, msg = "LOGO文件只能是 JPG、GIF、PNG 图片文件" };
                    if (file.InputStream.Length > 1024 * 1024) return new { code = -10, msg = "LOGO文件的大小不能超过 1MB" };

                    logo = Convert.ToBase64String(file.InputStream.ToBytes());

                    //存储为 BASE64 格式的
                    logo = string.Format("data:image/{0};base64,{1}", extension, logo);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ExceptionUtil.WriteException(ex));

                return new { code = -11, msg = "LOGO 存储失败，请联系管理员" };
            }

            #endregion

            #region 校验ICO文件

            string ico = string.Empty;

            try
            {
                if (jc.Context.Request.Files.Count > 0)
                {
                    var file = jc.Context.Request.Files["ico"];

                    var extension = Path.GetExtension(file.FileName);
                    if (string.IsNullOrEmpty(extension)) return new { code = -12, msg = "ICO文件只能是 ICO 图片文件" };

                    extension = extension.Substring(1).ToLowerInvariant();

                    if (extension != "ico") return new { code = -12, msg = "ICO文件只能是 ICO 图片文件" };
                    if (file.InputStream.Length > 1024 * 1024) return new { code = -13, msg = "ICO文件的大小不能超过 1MB" };

                    ico = Convert.ToBase64String(file.InputStream.ToBytes());

                    //存储为 BASE64 格式的
                    ico = string.Format("data:image/x-icon;base64,{1}", extension, ico);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ExceptionUtil.WriteException(ex));

                return new { code = -14, msg = "ICO 图标存储失败，请联系管理员" };
            }

            #endregion

            using (ILinqContext<Site> cx = Site.CreateContext())
            using (ILinqContext<SiteUsers> cx_relation = SiteUsers.CreateContext())
            {
                var site = Site.Get(cx, id);

                #region 校验站点数据是否存在相同的

                if (site == null)
                {
                    if (Site.Where("Title = {0}", title).Count() != 0) return new { code = -15, msg = "已经存在相同的站点名称，请更换其他站点名称" };
                    if (Site.Where("Domain = {0}", domain).Count() != 0) return new { code = -16, msg = "已经存在相同的站点域名，请更换其他站点名称" };
                }
                else
                {
                    if (site.Title != title && Site.Where("Title = {0}", title).Count() != 0) return new { code = -15, msg = "已经存在相同的站点名称，请更换其他站点名称" };
                    if (site.Domain != domain && Site.Where("Domain = {0}", domain).Count() != 0) return new { code = -16, msg = "已经存在相同的站点域名，请更换其他站点名称" };
                }

                #endregion

                if (site == null)
                {
                    site = new Site();

                    site.Id = StringUtil.UniqueId();
                    site.DateCreated = DateTime.Now;
                    site.UserId = jc.UserName;

                    cx.Add(site, true);

                    #region 将当前用户加入该站点

                    var relation = new SiteUsers();

                    relation.Id = StringUtil.UniqueId();
                    relation.DateCreated = DateTime.Now;
                    relation.SiteId = site.Id;
                    relation.UserId = jc.UserName;
                    relation.PermissionLevel = PermissionLevel.ADMIN;

                    cx_relation.Add(relation, true);

                    #endregion
                }

                site.Title = title;
                site.Domain = domain;
                site.KeyWords = keyWords;
                site.Description = description;
                site.Theme = theme;
                site.SortOrder = sortOrder;
                site.Logo = logo;
                site.ICO = ico;
                site.NeedAuditPost = needAuditPost.ToBoolean();

                cx.SubmitChanges();
                cx_relation.SubmitChanges();
            }

            return new { code = 1, msg = "保存成功" };
        }

        #endregion

        #region 查询站点

        /// <summary>
        /// 站点列表
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="id">根据站点ID查询（精确查询）</param>
        /// <param name="title">根据站点标题查询（模糊查询）</param>
        /// <param name="domain">根据站点域名查询（模糊查询）</param>
        /// <returns>
        /// {
        ///     code = 1,               //1：查询成功
        ///     data = 
        ///     [
        ///         {
        ///             id = "",                    //站点ID
        ///             title = "",                 //站点标题
        ///             domain = "",                //站点域名
        ///             key_words = "",             //站点关键字
        ///             description = "",           //站点描述文字
        ///             display_name = "",          //站点创建者
        ///             date_created = "",          //站点创建时间
        ///             sort_order = ""             //站点序号
        ///         }
        ///     ],
        ///     totalCount = 0,                     //总数
        ///     page = 0,                           //当前页数
        ///     orderbys = ""                       //排序字段
        /// }
        /// </returns>
        /// leixu
        /// 2016年6月29日20:44:17
        [HttpPost]
        object list(string id, string title, string domain)
        {
            WebQuery q = new WebQuery();
            q.Id = "site.list";
            q.LoadCondidtion();

            if (!string.IsNullOrEmpty(id)) q["siteId"] = id;
            if (!string.IsNullOrEmpty(title)) q["title"] = title;
            if (!string.IsNullOrEmpty(domain)) q["domain"] = domain;

            q.TotalCount = Site.Count(q);
            if (q.PageIndex1 > q.PageCount) q.PageIndex = Math.Max(q.PageCount - 1, 0);

            var dt = Site.GetDataTable(q);
            var data = new ArrayList();

            foreach (DataRow item in dt.Rows)
            {
                data.Add(new
                {
                    id = item["id"].ToString(),
                    title = item["title"].ToString(),
                    domain = item["domain"].ToString(),
                    key_words = item["keyWords"].ToString(),
                    description = item["description"].ToString(),
                    display_name = item["displayName"] is DBNull ? string.Empty : item["displayName"].ToString(),
                    date_created = item["dateCreated"].ToDateTime(),
                    sort_order = item["sortOrder"].ToInt()
                });
            }

            return new
            {
                code = 1,
                data = data,
                paging = new
                {
                    total_count = q.TotalCount,
                    page_size = q.PageSize,
                    page_index = q.PageIndex1
                },
                orderbys = q.orderbys
            };
        }

        #endregion

        #region 删除站点

        /// <summary>
        /// 根据ID删除站点信息，挂件信息会一并删除
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="id">站点ID</param>
        /// <param name="confirmed">确认删除</param>
        /// <returns>
        /// {
        ///     code = 1,           //-1：指定的站点不存在，删除失败，-2：指定的站点下有挂件内容，若确认删除则挂件内容也一并删除，是否确认删除，1：删除成功
        ///     msg = ""            //1：删除成功
        /// }
        /// </returns>
        /// leixu
        /// 2016年6月29日20:35:32
        [HttpPost]
        object delete(string id, bool confirmed)
        {
            using (ILinqContext<Site> cx = Site.CreateContext())
            {
                var site = Site.Get(cx, id);

                if (site == null) return new { code = -1, msg = "指定的站点不存在，删除失败" };

                if (!confirmed && Widget.Where("SiteId = {0}", site.Id).Count() != 0) return new { code = -2, msg = "指定的站点下有挂件内容，若确认删除则挂件内容也一并删除，是否确认删除" };

                Widget.Where("SiteId = {0}", site.Id).Delete();

                cx.Remove(site);
                cx.SubmitChanges();
            }

            return new { code = 1, msg = "删除成功" };
        }

        #endregion

        #region 复制站点



        #endregion
    }
}