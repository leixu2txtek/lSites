using ImageMagick;
using Kiss.Components.Security;
using Kiss.Utils;
using Kiss.Web;
using Kiss.Web.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Kiss.Components.Site.Web.Controllers
{
    /// <summary>
    /// 开放的控制器接口
    /// </summary>
    class OpenController : Controller
    {
        /// <summary>
        /// 首页跳转
        /// </summary>
        /// <returns></returns>
        ActionResult index()
        {
            #region 动态获取首页地址

            var host = jc.Context.Request.Url.Authority;

            //支援 NGINX 反向代理时 配置的外网地址
            if (!string.IsNullOrEmpty(jc.Context.Request.Headers["ORI_HOST"])) host = jc.Context.Request.Headers["ORI_HOST"];

            #endregion

            if (!jc.IsAuth) return new RedirectResult(jc.url(string.Format("/users/login?returnUrl={0}", string.Format("{0}://{1}", jc.Context.Request.Url.Scheme, host))));

            var relation = (from q in SiteUsers.CreateContext()
                            where q.UserId == jc.UserName
                            orderby q.PermissionLevel
                            select q).FirstOrDefault();

            if (relation == null) return new EmptyResult();

            return new RedirectResult(string.Format("{0}/themes/default/html/posts/index.html?siteId={1}", string.Format("{0}://{1}", jc.Context.Request.Url.Scheme, host), relation.SiteId));
        }

        #region 站点信息

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

        #endregion

        #region 挂件信息

        /// <summary>
        /// 根据容器ID获取挂件内容信息
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="siteId">站点ID</param>
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
        object get_widgets(string siteId, string[] ids)
        {
            if (ids.Length == 0) return new { code = -1, msg = "指定的挂件容器ID不能为空" };

            var widgets = (from q in Widget.CreateContext()
                           where new List<string>(ids).Contains(q.ContainerId) && q.SiteId == siteId
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

        #endregion

        #region 栏目信息

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
        /// 获取指定栏目下的子栏目详细信息
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="siteId">站点ID</param>
        /// <param name="parentId">栏目ID</param>
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
        /// 2016年10月9日11:38:19
        [HttpPost]
        object get_categories_by_parent(string siteId, string parentId)
        {
            var site = Site.Get(siteId);

            if (site == null) return new { code = -1, msg = "指定的站点不存在" };

            var categories = (from q in Category.CreateContext()
                              where q.SiteId == site.Id && q.ParentId == parentId
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

        #endregion

        #region 文章信息

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
                #region 处理扩展字段

                var props = new Dictionary<string, string>();

                if (!(item["propertyName"] is DBNull) && !(item["propertyValue"] is DBNull))
                {
                    var attributes = new ExtendedAttributes();
                    attributes.SetData(item["propertyName"].ToString(), item["propertyValue"].ToString());

                    foreach (string key in attributes.Keys)
                    {
                        props.Add(key, attributes[key]);
                    }
                }

                #endregion

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
                    image_url = item["imageUrl"].ToString(),
                    props = new Kiss.Json.JavaScriptSerializer().Serialize(props)
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
        /// 获取指定栏目下最新一篇文章
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="siteId">站点ID</param>
        /// <param name="categoryId">栏目ID</param>
        /// <returns>
        /// {
        ///     code = 1,                       //-1：指定的站点不存在，-2：指定的栏目不存，-3：指定栏目下没有文章
        ///     post = 
        ///     {
        ///         id = "",                    //文章ID
        ///         display_name = "",          //文章创建者显示名
        ///         title = "",                 //文章标题
        ///         date_created = "",          //文章创建时间
        ///         sub_title = "",             //文章子标题
        ///         summary = "",               //文章的摘要
        ///         content = "",               //文章内容（纯文字）
        ///         view_count = "",            //文章的查看次数
        ///         image_url = "",             //文章的第一个图片
        ///         props = "",                 //扩展字段（JSON字符串）
        ///         category =
        ///         {
        ///             id = "",                //栏目ID
        ///             title = "",             //栏目标题
        ///             url = ""                //栏目URL
        ///         }
        ///     }
        /// }
        /// </returns>
        /// leixu
        /// 2016年9月22日16:00:35
        [HttpPost]
        object get_first_post_by_category(string siteId, string categoryId)
        {
            var site = Site.Get(siteId);
            if (site == null) return new { code = -1, msg = "指定的站点不存在" };

            var category = Category.Get(categoryId);
            if (category == null) return new { code = -2, msg = "指定的栏目不存在" };

            var post = (from q in Posts.CreateContext()
                        where q.CategoryId == category.Id
                        orderby q.DatePublished descending
                        select q).FirstOrDefault();

            if (post == null) return new { code = -3, msg = "指定栏目下没有文章" };

            #region 处理扩展字段

            var props = new Dictionary<string, string>();

            if (post.ExtAttrs.Keys.Count > 0)
            {
                foreach (string key in post.ExtAttrs.Keys) props.Add(key, post[key]);
            }

            #endregion

            return new
            {
                code = 1,
                post = new
                {
                    id = post.Id,
                    display_name = post.DisplayName,
                    title = post.Title,
                    date_created = post.DateCreated.ToUniversalTime(),
                    sub_title = post.SubTitle,
                    summary = post.Summary,
                    content = post.Content,
                    view_count = post.ViewCount,
                    props = new Kiss.Json.JavaScriptSerializer().Serialize(props),
                    category = new
                    {
                        id = category.Id,
                        title = category.Title,
                        url = category.Url
                    }
                }
            };
        }

        /// <summary>
        /// 获取站点下指定栏目下文章
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="siteId">站点ID</param>
        /// <param name="categoryId">栏目ID</param>
        /// <param name="withChildren">包含子栏目</param>
        /// <returns>
        /// {
        ///     code = 1,                           //1：获取成功，-1：指定的站点不存在，-2：指定的栏目不存在
        ///     category = 
        ///     {
        ///         id = "",                       //栏目的ID
        ///         title = ""                     //栏目的标题
        ///     },
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
        ///             image_url = "",             //文章的第一个图片
        ///             props = ""                  //扩展字段（JSON字符串）
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
        object get_posts_by_category(string siteId, string categoryId, bool withChildren)
        {
            var site = Site.Get(siteId);
            if (site == null) return new { code = -1, msg = "指定的站点不存在" };

            var category = Category.Get(categoryId);
            if (category == null) return new { code = -2, msg = "指定的栏目不存在" };

            WebQuery q = new WebQuery();
            q.Id = "posts.category.list";
            q.LoadCondidtion();

            #region 构造查询参数

            q["siteId"] = site.Id;

            var categoryIds = new List<string>();

            if (withChildren)
            {
                categoryIds = (from c in Category.CreateContext()
                               where c.NodePath.StartsWith(category.NodePath)
                               select c.Id).ToList();
            }

            categoryIds.Add(category.Id);

            q["categoryIds"] = StringUtil.CollectionToDelimitedString(categoryIds, ",", "'");

            #endregion

            q.TotalCount = Posts.Count(q);
            if (q.PageIndex1 > q.PageCount) q.PageIndex = Math.Max(q.PageCount - 1, 0);

            var dt = Posts.GetDataTable(q);
            var data = new ArrayList();

            foreach (DataRow item in dt.Rows)
            {
                #region 处理扩展字段

                var props = new Dictionary<string, string>();

                if (!(item["propertyName"] is DBNull) && !(item["propertyValue"] is DBNull))
                {
                    var attributes = new ExtendedAttributes();
                    attributes.SetData(item["propertyName"].ToString(), item["propertyValue"].ToString());

                    foreach (string key in attributes.Keys)
                    {
                        props.Add(key, attributes[key]);
                    }
                }

                #endregion

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
                    image_url = item["imageUrl"].ToString(),
                    props = new Kiss.Json.JavaScriptSerializer().Serialize(props)
                });
            }

            return new
            {
                code = 1,
                data = data,
                category = new
                {
                    id = category.Id,
                    title = category.Title
                },
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

                //触发查看事件
                post.OnView(new Posts.ViewEventArgs());

                return new
                {
                    code = 1,
                    post = new
                    {
                        id = post.Id,
                        display_name = post.DisplayName,
                        title = post.Title,
                        date_created = post.DateCreated.ToUniversalTime(),
                        sub_title = post.SubTitle,
                        summary = post.Summary,
                        content = post.Content,
                        view_count = post.ViewCount,
                        category = new
                        {
                            id = category == null ? string.Empty : category.Id,
                            title = category == null ? string.Empty : category.Title,
                            url = category == null ? string.Empty : category.Url
                        }
                    }
                };
            }
        }

        /// <summary>
        /// 站内搜索
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="siteId">站点ID</param>
        /// <param name="search">关键字</param>
        /// <returns>
        /// {
        ///     code = 1,                           //1：获取成功，-1：指定的站点不存在
        ///     data = 
        ///     [
        ///         {
        ///             id = "",                    //文章ID
        ///             display_name = "",          //文章创建者显示名
        ///             title = "",                 //文章标题
        ///             date_created = "",          //文章创建时间
        ///             text = "",                  //文章内容（纯文字）
        ///             view_count = "",            //文章的查看次数
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
        /// 2016年10月12日09:50:02
        [HttpPost]
        object search(string siteId, string search)
        {
            var site = Site.Get(siteId);
            if (site == null) return new { code = -1, msg = "指定的站点不存在" };

            WebQuery q = new WebQuery();
            q.Id = "posts.list.search";
            q.LoadCondidtion();

            q["siteId"] = site.Id;

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                q["search"] = search;
            }

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
                    title = item["title"].ToString().Replace(search, string.Format("<b style='color:red'>{0}</b>", search)),
                    date_published = item["datePublished"].ToDateTime(),
                    text = item["text"].ToString().Replace(search, string.Format("<b style='color:red'>{0}</b>", search)),
                    view_count = item["viewCount"].ToInt()
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

        #endregion

        #region 用户信息

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="siteId">站点ID</param>
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
        ///     ],
        ///     current = 
        ///     {
        ///         id = "",                //站点ID
        ///         title = "",             //站点标题
        ///         role = ""               //用户所在站点角色
        ///     }
        /// }
        /// </returns>
        /// leixu
        /// 2016年7月7日09:37:06
        [HttpPost]
        object get_user_info(string siteId)
        {
            if (!jc.IsAuth) return new { code = -1, msg = "未登录，没有权限获取信息" };

            User user = User.Get(jc.UserName);

            if (user == null) return new { code = -2, msg = "登录用户信息不存在", return_url = jc.url("~/") };

            var relation = (from q in SiteUsers.CreateContext()
                            where q.SiteId == siteId && q.UserId == user.Id
                            select q).FirstOrDefault();

            if (relation == null) return new { code = -3, msg = "登录用户并不属于站点管理人员", return_url = jc.url("~/") };

            #region 查询用户和站点关系

            WebQuery qc = new WebQuery();
            qc.Id = "user.sites";

            qc.NoPaging();
            qc["userId"] = user.Id;

            var sites = SiteUsers.GetDataTable(qc);

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

            if (result.Count == 0) return new { code = -3, msg = "用户当前没有需要管理的站点" };

            #region 获取用户对当前站点的角色

            var current = (from DataRow dr in sites.Rows
                           where dr["id"].ToString() == relation.SiteId
                           select dr).FirstOrDefault();

            string role = current["permission"].ToString();
            role = StringEnum<PermissionLevel>.ToString(StringEnum<PermissionLevel>.SafeParse(role));

            #endregion

            return new
            {
                code = 1,
                display_name = user.DisplayName,
                avatar = StringUtil.templatestring(jc.ViewData, "$!security.avatorUrl($!jc.user.info,'50')"),
                sites = result,
                current = new
                {
                    id = current["id"],
                    title = current["title"],
                    role = role
                }
            };
        }

        /// <summary>
        /// 更新密码
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="oriPassword">原始密码</param>
        /// <param name="newPassword">新密码</param>
        /// <returns>
        /// {
        ///     code  = 1,          //-1：未登录，不允许修改密码，-2：原始密码不能为空，-3：新密码不能为空，-4：数据错误，请重新登录，-5：原始密码有误，请输入正确的原始密码
        ///     msg = "更新成功"
        /// }
        /// </returns>
        /// leixu
        /// 2016年10月10日09:56:25
        [HttpPost]
        object update_password(string oriPassword, string newPassword)
        {
            if (!jc.IsAuth) return new { code = -1, msg = "未登录，不允许修改密码" };
            if (string.IsNullOrEmpty(oriPassword)) return new { code = -2, msg = "原始密码不能为空" };
            if (string.IsNullOrEmpty(newPassword)) return new { code = -3, msg = "新密码不能为空" };

            using (ILinqContext<User> cx = User.CreateContext())
            {
                var user = User.Get(cx, jc.UserName);

                if (user == null) return new { code = -4, msg = "数据错误，请重新登录" };

                //MD5 加密
                var schema = DictSchema.GetByName("users", "config");
                if (schema != null && schema["md5"].ToBoolean()) oriPassword = SecurityUtil.MD5_Hash(oriPassword + schema["md5code"]);

                if (user.Password != oriPassword) return new { code = -5, msg = "原始密码有误，请输入正确的原始密码" };

                user.UpdatePassword(newPassword);

                cx.SubmitChanges();
            }

            return new { code = 1, msg = "更新成功" };
        }

        #endregion

        #region 图片、附件、视频信息

        /// <summary>
        /// 访问图片文件
        /// </summary>
        /// <remarks>请求方式：GET</remarks>
        /// <param name="url">图片地址</param>
        /// <param name="width">图片的宽度</param>
        /// <param name="height">图片的高度</param>
        /// <returns>图片文件流</returns>
        /// leixu
        /// 2016年8月24日14:29:50
        [HttpGet(60)]
        ActionResult read(string url, int width, int height)
        {
            var IMAGE_NOT_FOUND = Path.Combine(Config.Instance.IMAGE_PATH, "404.JPG");

            try
            {
                #region 校验文件是否存在

                if (string.IsNullOrEmpty(url)) return new FileContentResult(File.OpenRead(IMAGE_NOT_FOUND).ToBytes(), "image/jpeg");

                //校验URL中是否有时间
                var index = url.IndexOf('-');
                if (index == -1) return new FileContentResult(File.OpenRead(IMAGE_NOT_FOUND).ToBytes(), "image/jpeg");

                //校验文件夹是否存在
                var directory = Path.Combine(Config.Instance.IMAGE_PATH, url.Substring(0, index));
                if (!Directory.Exists(directory)) return new FileContentResult(File.OpenRead(IMAGE_NOT_FOUND).ToBytes(), "image/jpeg");

                //校验文件是否存在
                var file = Path.Combine(directory, url.Substring(index + 1));
                if (!File.Exists(file)) return new FileContentResult(File.OpenRead(IMAGE_NOT_FOUND).ToBytes(), "image/jpeg");

                #endregion

                var extension = Path.GetExtension(file).Substring(1).ToLowerInvariant();
                var thumbnail = string.Empty;

                //输出源图片
                if (width == 0 && height == 0) return new FileContentResult(File.OpenRead(file).ToBytes(), string.Format("image/{0}", extension == "jpg" ? "jpeg" : extension));

                #region 读取缩略图

                if (width > 0 && height > 0)
                {
                    thumbnail = file.Insert(file.LastIndexOf('.'), string.Format(".{0}.{1}", width, height));
                }
                else if (width > 0)
                {
                    thumbnail = file.Insert(file.LastIndexOf('.'), string.Format(".{0}.0", width));
                }
                else
                {
                    thumbnail = file.Insert(file.LastIndexOf('.'), string.Format(".0.{0}", width));
                }

                //缩略图已经存在，直接输出
                if (File.Exists(thumbnail)) return new FileContentResult(File.OpenRead(thumbnail).ToBytes(), string.Format("image/{0}", extension == "jpg" ? "jpeg" : extension));

                using (FileStream fs = File.OpenRead(file))
                using (MagickImage image = new MagickImage(fs))
                {
                    #region 判定宽高

                    if (width > 0 || height > 0)
                    {
                        MagickGeometry g = null;

                        if (width > 0 && height > 0)
                        {
                            g = new MagickGeometry(width, height);
                        }
                        else if (width > 0)
                        {
                            g = new MagickGeometry(width);
                        }
                        else
                        {
                            g = new MagickGeometry(height);
                        }

                        g.Greater = true;

                        if (width > 0 && height > 0)
                        {
                            g.IgnoreAspectRatio = false;
                            g.FillArea = true;

                            image.Resize(g);
                            image.Crop(width, height);
                        }
                        else
                        {
                            g.IgnoreAspectRatio = false;

                            image.Thumbnail(g);
                        }
                    }

                    #endregion

                    image.Strip();
                    image.AutoOrient();
                    image.Interlace = Interlace.Plane;

                    image.Write(thumbnail);

                    return new FileContentResult(image.ToByteArray(), string.Format("image/{0}", extension == "jpg" ? "jpeg" : extension));
                }

                #endregion
            }
            catch (Exception ex)
            {
                logger.Error(ExceptionUtil.WriteException(ex));

                return new FileContentResult(File.OpenRead(IMAGE_NOT_FOUND).ToBytes(), "image/jpeg");
            }
        }

        /// <summary>
        /// 访问附件文件
        /// </summary>
        /// <remarks>请求方式：GET</remarks>
        /// <param name="url">附件地址</param>
        /// <param name="name">文件名</param>
        /// <returns>文件流</returns>
        /// leixu
        /// 2016年10月12日15:27:40
        [HttpGet(60)]
        ActionResult file(string url, string name)
        {
            try
            {
                #region 校验文件是否存在

                if (string.IsNullOrWhiteSpace(url)) return new EmptyResult();
                if (string.IsNullOrWhiteSpace(name)) return new EmptyResult();

                //校验URL中是否有时间
                var index = url.IndexOf('-');
                if (index == -1) return new EmptyResult();

                //校验文件夹是否存在
                var directory = Path.Combine(Config.Instance.FILE_PATH, url.Substring(0, index));
                if (!Directory.Exists(directory)) return new EmptyResult();

                //校验文件是否存在
                var file = Path.Combine(directory, url.Substring(index + 1));
                if (!File.Exists(file)) return new EmptyResult();

                #endregion

                jc.Context.Response.ContentType = "application/octet-stream";
                jc.Context.Response.ContentEncoding = Encoding.UTF8;
                jc.Context.Response.AddHeader("Connection", "Keep-Alive");

                #region 构造文件下载名称

                var donwload_name = Path.GetFileNameWithoutExtension(name);

                //remove invalide filename char
                foreach (var _c in Path.GetInvalidFileNameChars()) donwload_name = donwload_name.Replace(_c, new char());

                donwload_name = Uri.EscapeDataString(donwload_name);
                donwload_name = donwload_name.Trim();

                jc.Context.Response.AddHeader("Content-Disposition", string.Format("attachment; filename*=UTF-8''{0}{1}", donwload_name, Path.GetExtension(name)));

                #endregion

                using (var content = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    jc.Context.Response.AddHeader("Content-Length", content.Length.ToString());

                    byte[] buffer = new byte[1024 * 64];

                    while (true)
                    {
                        if (!jc.Context.Response.IsClientConnected) break;

                        var read = content.Read(buffer, 0, buffer.Length);

                        if (read == 0) break;

                        jc.Context.Response.OutputStream.Write(buffer, 0, read);
                        jc.Context.Response.Flush();
                    }
                }

                jc.Context.Response.End();
            }
            catch (Exception ex)
            {
                logger.Error(ExceptionUtil.WriteException(ex));
            }
            finally
            {
                jc.Context.Response.OutputStream.Close();
            }

            return new EmptyResult();
        }

        /// <summary>
        /// 访问视频文件
        /// </summary>
        /// <remarks>请求方式：GET</remarks>
        /// <param name="url">附件地址</param>
        /// <returns>文件流</returns>
        /// leixu
        /// 2016年10月12日15:27:40
        [HttpGet(60)]
        ActionResult video(string url)
        {
            try
            {
                #region 校验文件是否存在

                if (string.IsNullOrEmpty(url)) return new EmptyResult();

                //校验URL中是否有时间
                var index = url.IndexOf('-');
                if (index == -1) return new EmptyResult();

                //校验文件夹是否存在
                var directory = Path.Combine(Config.Instance.VIDEO_PATH, url.Substring(0, index));
                if (!Directory.Exists(directory)) return new EmptyResult();

                //校验文件是否存在
                var file = Path.Combine(directory, url.Substring(index + 1));
                if (!File.Exists(file)) return new EmptyResult();

                #endregion

                #region 输出视频文件流

                jc.Context.Response.ContentType = "video/mp4";
                jc.Context.Response.AddHeader("Accept-Ranges", "bytes");

                using (var video = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var range = jc.Context.Request.Headers["Range"];

                    if (!string.IsNullOrEmpty(range))
                    {
                        var ranges = range.Split(new char[] { '=', '-' });
                        var start = Int32.Parse(ranges[1]);
                        video.Seek(start, SeekOrigin.Begin);

                        jc.Context.Response.StatusCode = 206;
                        jc.Context.Response.AddHeader("Content-Range", String.Format(" bytes {0}-{1}/{2}", start, video.Length - 1, video.Length));
                    }

                    byte[] buffer = new byte[1024 * 64];

                    while (true)
                    {
                        if (!jc.Context.Response.IsClientConnected) break;

                        var read = video.Read(buffer, 0, buffer.Length);

                        if (read == 0) break;

                        jc.Context.Response.OutputStream.Write(buffer, 0, read);
                        jc.Context.Response.Flush();
                    }
                }

                jc.Context.Response.End();

                #endregion
            }
            catch (Exception ex)
            {
                logger.Error(ExceptionUtil.WriteException(ex));
            }
            finally
            {
                jc.Context.Response.OutputStream.Close();
            }

            return new EmptyResult();
        }

        #endregion
    }
}