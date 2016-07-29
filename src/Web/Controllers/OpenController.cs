using Kiss.Components.Security;
using Kiss.Security;
using Kiss.Utils;
using Kiss.Web;
using Kiss.Web.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Kiss.Components.Site.Web.Controllers
{
    /// <summary>
    /// 开放的控制器接口
    /// </summary>
    class OpenController : Controller
    {
        /// <summary>
        /// 获取站点信息
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
        /// 2016年7月28日15:08:32
        [HttpPost]
        object get_site_info(string domain)
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
        /// 根据容器ID获取挂件内容信息
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="ids">挂件容器ID数组</param>
        /// <returns>
        /// {
        ///     code = 1,                       //-1：指定的挂件容器ID不能为空
        ///     widgets = 
        ///     [
        ///         {
        ///             name = "",              //挂件ID
        ///             title = "",             //挂件的显示名称
        ///             container_id = "",      //挂件所在位置的占位符
        ///             props = ""              //挂件的扩展属性，json 字符串
        ///         }
        ///     ]
        /// }
        /// </returns>
        /// leixu
        /// 2016年6月30日11:19:05
        [HttpPost]
        object get_widgets(string[] ids)
        {
            if (ids.Length == 0) return new { code = -1, msg = "指定的挂件容器ID不能为空" };

            var widgets = (from q in Widget.CreateContext()
                           where new List<string>(ids).Contains(q.ContainerId)
                           select q).ToList();

            var data = new ArrayList();

            foreach (var widget in widgets)
            {
                var props = new Dictionary<string, string>();

                foreach (string item in widget.ExtAttrs.Keys)
                {
                    props.Add(item, widget[item]);
                }

                data.Add(new
                {
                    name = widget.Name,
                    title = widget.Title,
                    container_id = widget.ContainerId,
                    props = new Kiss.Json.JavaScriptSerializer().Serialize(props)
                });
            }

            return new
            {
                code = 1,
                widgets = data
            };
        }

        /// <summary>
        /// 获取站点下的栏目详细信息
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="siteId">站点ID</param>
        /// <returns>
        /// {
        ///     code = 1,                   //-1：指定的站点不存在，1：成功获取
        ///     data = 
        ///     [
        ///         {
        ///             id = "",            //栏目的ID
        ///             title = "",         //栏目的标题
        ///             url = "",           //栏目的Url
        ///             show_in_menu = ""   //栏目是否在菜单上显示
        ///         }
        ///     ]
        /// }
        /// </returns>
        /// leixu
        /// 2016年7月4日10:12:47
        [HttpPost]
        object get_categories(string siteId)
        {
            var site = Site.Get(siteId);

            if (site == null) return new { code = -1, msg = "指定的站点不存在" };

            var categories = (from q in Category.CreateContext()
                              where q.SiteId == site.Id
                              orderby q.SortOrder ascending
                              select new
                              {
                                  id = q.Id,
                                  title = q.Title,
                                  url = q.Url,
                                  show_in_menu = q.ShowInMenu
                              }).ToList();

            return new
            {
                code = 1,
                data = categories
            };
        }

        /// <summary>
        /// 获取站点下最新文章
        /// 注：取创建时间最新的文章，无栏目约束
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="siteId">站点ID</param>
        /// <returns>
        /// {
        ///     code = 1,                           //1：获取成功
        ///     data = 
        ///     [
        ///         {
        ///             id = "",                    //文章ID
        ///             display_name = "",          //文章创建者显示名
        ///             title = "",                 //文章标题
        ///             date_created = "",          //文章创建时间
        ///             sub_title = "",             //文章子标题
        ///             summary = "",               //文章的摘要
        ///             text = "",                  //文章内容（纯文字）
        ///             view_count = "",            //文章的查看次数
        ///             image_url = ""              //文章的第一个图片
        ///         }
        ///     ],
        ///     paging = 
        ///     {
        ///         total_count = 0,            //总数
        ///         page_size = 10,             //分页大小
        ///         page_index = 1              //当前页码
        ///     }
        /// }
        /// </returns>
        /// leixu
        /// 2016年7月28日15:16:343
        [HttpPost]
        object get_latest_posts(string siteId)
        {
            var site = Site.Get(siteId);
            if (site == null) return new { code = -1, msg = "指定的站点不存在" };

            WebQuery q = new WebQuery();
            q.Id = "posts.latest.list";
            q.LoadCondidtion();

            q["siteId"] = site.Id;

            q.TotalCount = Posts.Count(q);
            if (q.PageIndex1 > q.PageCount) q.PageIndex = Math.Max(q.PageCount - 1, 0);

            var dt = Posts.GetDataTable(q);
            var data = new ArrayList();

            foreach (DataRow item in dt.Rows)
            {
                data.Add(new
                {
                    id = item["id"].ToString(),
                    display_name = item["displayName"].ToString(),
                    title = item["title"].ToString(),
                    date_published = item["datePublished"].ToDateTime(),
                    sub_title = item["subTitle"].ToString(),
                    summary = item["summary"].ToString(),
                    text = item["text"].ToString(),
                    view_count = item["viewCount"].ToInt(),
                    image_url = item["imageUrl"].ToString()
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
                }
            };
        }

        /// <summary>
        /// 获取站点下指定栏目下文章
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="siteId">站点ID</param>
        /// <param name="categoryId">栏目ID</param>
        /// <returns>
        /// {
        ///     code = 1,                           //1：获取成功
        ///     data = 
        ///     [
        ///         {
        ///             id = "",                    //文章ID
        ///             display_name = "",          //文章创建者显示名
        ///             title = "",                 //文章标题
        ///             date_created = "",          //文章创建时间
        ///             sub_title = "",             //文章子标题
        ///             summary = "",               //文章的摘要
        ///             text = "",                  //文章内容（纯文字）
        ///             view_count = "",            //文章的查看次数
        ///             image_url = ""              //文章的第一个图片
        ///         }
        ///     ],
        ///     paging = 
        ///     {
        ///         total_count = 0,            //总数
        ///         page_size = 10,             //分页大小
        ///         page_index = 1              //当前页码
        ///     }
        /// }
        /// </returns>
        /// leixu
        /// 2016年3月15日19:56:19
        [HttpPost]
        object get_posts_by_category(string siteId, string categoryId)
        {
            var site = Site.Get(siteId);
            if (site == null) return new { code = -1, msg = "指定的站点不存在" };

            var category = Category.Get(categoryId);
            if (category == null) return new { code = -1, msg = "指定的栏目不存在" };

            WebQuery q = new WebQuery();
            q.Id = "posts.category.list";
            q.LoadCondidtion();

            q["siteId"] = site.Id;
            q["categoryId"] = category.Id;

            q.TotalCount = Posts.Count(q);
            if (q.PageIndex1 > q.PageCount) q.PageIndex = Math.Max(q.PageCount - 1, 0);

            var dt = Posts.GetDataTable(q);
            var data = new ArrayList();

            foreach (DataRow item in dt.Rows)
            {
                data.Add(new
                {
                    id = item["id"].ToString(),
                    display_name = item["displayName"].ToString(),
                    title = item["title"].ToString(),
                    date_published = item["datePublished"].ToDateTime(),
                    sub_title = item["subTitle"].ToString(),
                    summary = item["summary"].ToString(),
                    text = item["text"].ToString(),
                    view_count = item["viewCount"].ToInt(),
                    image_url = item["imageUrl"].ToString()
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
                }
            };
        }

        /// <summary>
        /// 文章详细信息
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="postId">文章ID</param>
        /// <returns>
        /// {
        ///     code = 1,           //1：获取成功，-1：指定的站点不存在，-2：指定的文章不存在
        ///     post = new
        ///     {
        ///         id = "",                  //ID
        ///         display_name = "",        //作者
        ///         title = "",               //标题
        ///         date_created = "",        //创建时间
        ///         sub_title = "",           //子标题
        ///         summary = "",             //概述
        ///         content = "",             //内容，包含HTML
        ///         view_count = "",          //查看数
        ///         category = 
        ///         {
        ///             id = "",              //栏目的ID
        ///             title = "",           //栏目的标题
        ///             url = ""              //栏目的URL
        ///         }   
        ///      }      
        /// }
        /// </returns>
        [HttpPost]
        object get_post_info(string siteId, string postId)
        {
            var site = Site.Get(siteId);
            if (site == null) return new { code = -1, msg = "指定的站点不存在" };

            using (ILinqContext<Posts> cx = Posts.CreateContext())
            {
                var post = Posts.Get(cx, postId);

                if (post == null) return new { code = -2, msg = "指定的文章不存在" };

                #region 获取栏目信息

                Category category = null;

                if (!string.IsNullOrEmpty(post.CategoryId))
                {
                    category = Category.Get(post.CategoryId);
                }

                #endregion

                post.ViewCount = post.ViewCount + 1;

                cx.SubmitChanges();

                return new
                {
                    code = 1,
                    post = new
                    {
                        id = post.Id,
                        display_name = post.DisplayName,
                        title = post.Title,
                        date_created = post.DateCreated,
                        sub_title = post.SubTitle,
                        summary = post.Summary,
                        content = post.Content,
                        view_count = post.ViewCount,
                        category = new
                        {
                            id = category.Id,
                            title = category.Title,
                            url = category.Url
                        }
                    }
                };
            }
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <returns>
        /// {
        ///     code = 1,                   //-1：未登录，没有权限获取信息，-2：登录用户信息不存在
        ///     msg = "",
        ///     return_url = ""             //跳转的页面
        ///     display_name = "",          //用户名称
        ///     avatar = "",                //用户头像
        ///     sites = 
        ///     [
        ///         {
        ///             id = "",            //站点ID
        ///             title = ""          //站点标题
        ///         }
        ///     ]
        /// }
        /// </returns>
        /// leixu
        /// 2016年7月7日09:37:06
        [HttpPost]
        object get_user_info()
        {
            if (!jc.IsAuth) return new { code = -1, msg = "未登录，没有权限获取信息" };

            User user = User.Get(jc.UserName);

            if (user == null) return new { code = -2, msg = "登录用户信息不存在", return_url = jc.url("~/") };

            #region 查询用户和站点关系

            WebQuery q = new WebQuery();
            q.Id = "user.sites";

            q.NoPaging();
            q["userId"] = user.Id;

            var sites = SiteUsers.GetDataTable(q);

            var result = new ArrayList();

            foreach (DataRow item in sites.Rows)
            {
                result.Add(new
                {
                    id = item["id"].ToString(),
                    title = item["title"].ToString()
                });
            }

            #endregion

            return new
            {
                code = 1,
                display_name = user.DisplayName,
                avatar = StringUtil.templatestring(jc.ViewData, "$!security.avatorUrl($!jc.user.info,'50')"),
                sites = result
            };
        }
    }
}