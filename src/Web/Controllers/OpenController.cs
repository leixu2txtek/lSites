using ImageMagick;
using Kiss.Components.Security;
using Kiss.Security;
using Kiss.Utils;
using Kiss.Web;
using Kiss.Web.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
        /// 首页跳转
        /// </summary>
        /// <returns></returns>
        ActionResult index()
        {
            if (!jc.IsAuth) return new RedirectResult(jc.url("~users/login"));

            var relation = (from q in SiteUsers.CreateContext()
                            where q.UserId == jc.UserName
                            orderby q.PermissionLevel
                            select q).FirstOrDefault();

            if (relation == null) return new EmptyResult();

            return new RedirectResult(jc.url(string.Format("~/themes/default/html/posts/index.html?siteId={0}", relation.SiteId)));
        }

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
        ///         text = "",                  //文章内容（纯文字）
        ///         view_count = "",            //文章的查看次数
        ///         image_url = "",             //文章的第一个图片
        ///         props = ""                  //扩展字段（JSON字符串）
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
                    date_published = post.DatePublished,
                    sub_title = post.SubTitle,
                    summary = post.Summary,
                    text = post.Text,
                    view_count = post.ViewCount,
                    image_url = post.ImageUrl,
                    props = new Kiss.Json.JavaScriptSerializer().Serialize(props)
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
        object get_posts_by_category(string siteId, string categoryId)
        {
            var site = Site.Get(siteId);
            if (site == null) return new { code = -1, msg = "指定的站点不存在" };

            var category = Category.Get(categoryId);
            if (category == null) return new { code = -2, msg = "指定的栏目不存在" };

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
        ///     ]
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
        /// 访问图片文件
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="width">图片的宽度</param>
        /// <param name="height">图片的高度</param>
        /// <returns>图片文件流</returns>
        /// leixu
        /// 2016年8月24日14:29:50
        [HttpGet(60)]
        ActionResult read(string url, int width, int height)
        {
            try
            {
                #region 校验文件是否存在

                if (string.IsNullOrEmpty(url)) return new FileContentResult(File.OpenRead(Config.IMAGE_NOT_FOUND).ToBytes(), "image/jpeg");

                //校验URL中是否有时间
                var index = url.IndexOf('-');
                if (index == -1) return new FileContentResult(File.OpenRead(Config.IMAGE_NOT_FOUND).ToBytes(), "image/jpeg");

                //校验文件夹是否存在
                var directory = Path.Combine(Config.IMAGE_PATH, url.Substring(0, index));
                if (!Directory.Exists(directory)) return new FileContentResult(File.OpenRead(Config.IMAGE_NOT_FOUND).ToBytes(), "image/jpeg");

                //校验文件是否存在
                var file = Path.Combine(directory, url.Substring(index + 1));
                if (!File.Exists(file)) return new FileContentResult(File.OpenRead(Config.IMAGE_NOT_FOUND).ToBytes(), "image/jpeg");

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

                return new FileContentResult(File.OpenRead(Config.IMAGE_NOT_FOUND).ToBytes(), "image/jpeg");
            }
        }
    }
}