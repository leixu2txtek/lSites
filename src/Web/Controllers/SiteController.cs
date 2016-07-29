using Kiss.Utils;
using Kiss.Web;
using Kiss.Web.Mvc;
using Kiss.Web.Utils;
using System;
using System.Collections;
using System.Data;
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

            if (!jc.IsAuth)
            {
                //权限验证失败
                ResponseUtil.OutputJson(httpContext.Response, new { code = 403, msg = "没有权限访问" });
                e.PreventDefault = true;
                return;
            }

            //TODO
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
                    key_words = site.KeyWords,
                    description = site.Description,
                    theme = site.Theme,
                    sort_order = site.SortOrder,
                    need_audit_post = site.NeedAuditPost
                }
            };
        }

        /// <summary>
        /// 根据域名获取站点的详细信息
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="domain">站点域名</param>
        /// <returns>
        /// {
        ///     code = 1,
        ///     data = 
        ///     {
        ///         id = "",                //站点ID
        ///         title = "",             //站点标题
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
        /// 2016年6月29日19:52:43
        [HttpPost]
        object detail_with_domain(string domain)
        {
            Site site = (from q in Site.CreateContext()
                         where q.Domain == domain
                         select q).FirstOrDefault();

            if (site == null) return new { code = -1, msg = "指定的站点不存在" };

            return new
            {
                code = 1,
                data = new
                {
                    id = site.Id,
                    title = site.Title,
                    domain = site.Domain,
                    key_words = site.KeyWords,
                    description = site.Description,
                    theme = site.Theme,
                    sort_order = site.SortOrder,
                    need_audit_post = site.NeedAuditPost
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
        ///     code = 1,       //-1：站点名称不能为空，-2：站点域名不能为空，-3：站点域名支持大小写英文以及点号，下划线，-4：已经存在相同的站点名称，请更换其他站点名称，-5：已经存在相同的站点域名，请更换其他站点名称，-6：站点标题不能超过100个字符，-6：站点域名不能超过100个字符，-6：站点关键字不能超过500个字符，-6：站点描述不能超过1000个字符，-6：站点的主题名不能超过20个字符
        ///     msg = ""        //1：保存成功
        /// }
        /// </returns>
        /// leixu
        /// 2016年6月29日19:55:25
        [HttpPost]
        object save(string id, string title, string domain, string keyWords, string description, string theme, int sortOrder, bool needAuditPost)
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

            if (title.Length > 100) return new { code = -6, msg = "站点标题不能超过100个字符" };
            if (domain.Length > 100) return new { code = -6, msg = "站点域名不能超过100个字符" };
            if (keyWords.Length > 500) return new { code = -6, msg = "站点关键字不能超过500个字符" };
            if (description.Length > 1000) return new { code = -6, msg = "站点描述不能超过1000个字符" };
            if (theme.Length > 20) return new { code = -6, msg = "站点的主题名不能超过20个字符" };

            #endregion

            using (ILinqContext<Site> cx = Site.CreateContext())
            {
                var site = Site.Get(cx, id);

                #region 校验站点数据是否存在相同的

                if (site == null)
                {
                    if (Site.Where("Title = {0}", title).Count() != 0) return new { code = -4, msg = "已经存在相同的站点名称，请更换其他站点名称" };
                    if (Site.Where("Domain = {0}", domain).Count() != 0) return new { code = -5, msg = "已经存在相同的站点域名，请更换其他站点名称" };
                }
                else
                {
                    if (site.Title != title && Site.Where("Title = {0}", title).Count() != 0) return new { code = -4, msg = "已经存在相同的站点名称，请更换其他站点名称" };
                    if (site.Domain != domain && Site.Where("Domain = {0}", domain).Count() != 0) return new { code = -5, msg = "已经存在相同的站点域名，请更换其他站点名称" };
                }

                #endregion

                if (site == null)
                {
                    site = new Site();

                    site.Id = StringUtil.UniqueId();
                    site.DateCreated = DateTime.Now;
                    site.UserId = jc.UserName;

                    cx.Add(site, true);
                }

                site.Title = title;
                site.Domain = domain;
                site.KeyWords = keyWords;
                site.Description = description;
                site.Theme = theme;
                site.SortOrder = sortOrder;
                site.NeedAuditPost = needAuditPost;

                cx.SubmitChanges();
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
            if (!string.IsNullOrEmpty(title)) q["title"] = id;
            if (!string.IsNullOrEmpty(domain)) q["domain"] = id;

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
    }
}