using Kiss.Utils;
using Kiss.Web;
using Kiss.Web.Mvc;
using Kiss.Web.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Kiss.Components.Site.Web.Controllers
{
    /// <summary>
    /// 文章管理的控制器接口
    /// </summary>
    class PostsController : Controller
    {
        public PostsController()
        {
            BeforeActionExecute += PostsController_BeforeActionExecute;
        }

        private void PostsController_BeforeActionExecute(object sender, BeforeActionExecuteEventArgs e)
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

        #region 文章

        /// <summary>
        /// 文章列表
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="key">关键字</param>
        /// <param name="status">文章状态，为空则表示查询所有状态文章，0：草稿，1：待审核，2：已发布</param>
        /// <returns>
        /// {
        ///     code = 1,                           //1：获取成功
        ///     data = 
        ///     [
        ///         {
        ///             id = "",                    //文章ID
        ///             title = "",                 //文章标题
        ///             summary = "",               //文章的摘要
        ///             category = "",              //文章的分类信息
        ///             date_created = "",          //文章的创建时间
        ///             view_count = "",            //文章的查看数
        ///             sort_order = "",            //文章的排序
        ///             status = "",                //文章的状态
        ///             date_published = ""         //文章发布时间
        ///         }
        ///     ],
        ///     totalCount = q.TotalCount,
        ///     page = q.PageIndex1,
        ///     orderbys = q.orderbys
        /// }
        /// </returns>
        /// leixu
        /// 2016年7月1日17:37:13
        [HttpPost]
        object list(string key, string status)
        {
            var site = (Site)jc["site"];

            WebQuery q = new WebQuery();
            q.Id = "posts.list";
            q.LoadCondidtion();

            if (!string.IsNullOrEmpty(key)) q["key"] = key;
            if (!string.IsNullOrEmpty(status)) q["status"] = status;

            q["userId"] = jc.UserName;
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
                    site_id = site.Id,
                    title = item["title"].ToString(),
                    category = item["category"] is DBNull ? string.Empty : item["category"].ToString(),
                    date_created = item["dateCreated"].ToDateTime(),
                    view_count = item["viewCount"].ToInt(),
                    sort_order = item["sortOrder"].ToInt(),
                    status = StringEnum<Status>.ToString(StringEnum<Status>.SafeParse(item["status"].ToString())),
                    date_published = item["datePublished"].ToDateTime()
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

        /// <summary>
        /// 文章详情
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="id">文章ID</param>
        /// <returns>
        /// {
        ///     code = 1,       //-1：指定的文章不存在，1：成功获取文章内容
        ///     post = 
        ///     {
        ///         id = "",                    //文章ID
        ///         title = "",                 //文章标题
        ///         sub_title = "",             //文章子标题
        ///         content = "",               //文章内容
        ///         text = "",                  //文章纯文本
        ///         summary = "",               //文章的摘要
        ///         category = "",              //文章的分类信息
        ///         date_created = "",          //文章的创建时间
        ///         view_count = "",            //文章的查看数
        ///         sort_order = "",            //文章的排序
        ///         is_pending = "",            //文章是否待审核
        ///         is_published = "",          //文章是否发布
        ///         date_published = ""         //文章发布时间
        ///     }
        /// }
        /// </returns>
        /// leixu
        /// 2016年6月30日19:57:31
        [HttpPost]
        object detail(string id)
        {
            var site = (Site)jc["site"];

            Posts post = (from q in Posts.CreateContext()
                          where q.Id == id && q.SiteId == site.Id
                          select q).FirstOrDefault();

            if (post == null) return new { code = -1, msg = "指定的文章不存在" };

            #region 获取文章的所属栏目信息

            var category = (from q in Category.CreateContext()
                            where q.Id == post.CategoryId
                            select new
                            {
                                id = q.Id,
                                title = q.Title
                            }).FirstOrDefault();

            #endregion

            #region 扩展属性

            var props = new Dictionary<string, string>();

            foreach (string item in post.ExtAttrs.Keys)
            {
                props.Add(item, post[item]);
            }

            #endregion

            return new
            {
                code = 1,
                post = new
                {
                    id = post.Id,
                    title = post.Title,
                    sub_title = post.SubTitle,
                    content = post.Content,
                    text = post.Text,
                    summary = post.Summary,
                    category = category ?? new object() { },
                    date_created = post.DateCreated.ToUniversalTime(),
                    view_count = post.ViewCount,
                    sort_order = post.SortOrder,
                    status = StringEnum<Status>.ToString(post.Status),
                    date_published = post.DatePublished.ToUniversalTime(),
                    image_url = post.ImageUrl,
                    is_top = post.IsTop.ToString().ToLowerInvariant(),
                    props = new Kiss.Json.JavaScriptSerializer().Serialize(props)
                }
            };
        }

        /// <summary>
        /// 添加/修改文章内容
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="id">文章ID</param>
        /// <param name="title">文章标题</param>
        /// <param name="subTitle">文章子标题</param>
        /// <param name="content">文章内容</param>
        /// <param name="summary">文章摘要</param>
        /// <param name="categoryId">文章分类ID</param>
        /// <param name="viewCount">文章查看次数</param>
        /// <param name="sortOrder">文章序号</param>
        /// <param name="imageUrl">引导图片</param>
        /// <param name="dateCreated">创建时间</param>
        /// <param name="isTop">是否置顶</param>
        /// <param name="publish">是否发布</param>
        /// <returns>
        /// {
        ///     code = 1,       //-1：文章标题不能为空，-2：文章内容不能为空，-3：指定的分类不存在，-4：摘要的长度不能超过2000个字符，-5：文章标题的长度不能超过50个字符，-6：文章副标题的长度不能超过100个字符，-7：文章的查看次数不能设置为小于0，-8：扩展字段格式不正确
        ///     id = "",        //文章的存储的ID
        ///     msg = "保存成功"
        /// }
        /// </returns>
        /// leixu
        /// 2016年6月30日20:19:36
        [HttpPost]
        object save(string id, string title, string subTitle, string content, string summary, string categoryId, int viewCount, int sortOrder, string imageUrl, string dateCreated, bool isTop, bool publish)
        {
            #region 校验参数

            if (string.IsNullOrWhiteSpace(title)) return new { code = -1, msg = "文章标题不能为空" };
            if (string.IsNullOrWhiteSpace(content)) return new { code = -2, msg = "文章内容不能为空" };

            title = title.Trim();
            content = content.Trim();

            if (!string.IsNullOrWhiteSpace(subTitle)) subTitle = subTitle.Trim();

            if (title.Length > 50) return new { code = -5, msg = "文章标题的长度不能超过50个字符" };
            if (!string.IsNullOrWhiteSpace(subTitle) && subTitle.Length > 100) return new { code = -6, msg = "文章副标题的长度不能超过100个字符" };
            if (viewCount < 0) return new { code = -7, msg = "文章的查看次数不能设置为小于0" };

            //过滤文章内容中是否有脚本
            content = Regex.Replace(content, @"<script[\s\S]+</script *>", string.Empty, RegexOptions.IgnoreCase);
            content = Regex.Replace(content, @"<[^>]*(src|href)[\s\S]*=[\s\S]*script:[^>]*>", string.Empty, RegexOptions.IgnoreCase);

            //获取纯文本
            var text = StringUtil.TrimHtml(content);

            //如果摘要为空，则默认提供一份100字的摘要
            if (string.IsNullOrWhiteSpace(summary)) summary = StringUtil.Trim(text, 100);
            if (summary.Length > 2000) return new { code = -4, msg = "摘要的长度不能超过2000个字符" };

            #region 获取文章所属分类

            Category category = null;

            if (!string.IsNullOrWhiteSpace(categoryId))
            {
                category = Category.Get(categoryId);

                if (categoryId == null) return new { code = -3, msg = "指定的分类不存在" };
            }

            #endregion

            #endregion

            var site = (Site)jc["site"];

            using (ILinqContext<Posts> cx = Posts.CreateContext())
            {
                var post = (from q in cx
                            where q.Id == id && q.SiteId == site.Id
                            select q).FirstOrDefault();

                if (post == null)
                {
                    post = new Posts();

                    post.Id = StringUtil.UniqueId();
                    post.DateCreated = !string.IsNullOrEmpty(dateCreated) ? dateCreated.ToDateTime() : DateTime.Now;
                    post.UserId = jc.UserName;
                    post.SiteId = site.Id;

                    cx.Add(post, true);
                }

                post.Title = title;
                post.SubTitle = title;
                post.Content = content;
                post.Text = text;
                post.Summary = summary;

                post.CategoryId = category == null ? string.Empty : category.Id;
                post.SortOrder = sortOrder;
                post.ViewCount = viewCount;
                post.DisplayName = jc.User.Info.DisplayName;

                //同一个栏目下只能有一个文章被置顶
                if (isTop) Posts.Where("CategoryId = {0}", post.CategoryId).Set("IsTop", false).Update();

                post.IsTop = isTop;

                #region 处理图片

                post.ImageUrl = imageUrl ?? string.Empty;

                if (string.IsNullOrEmpty(post.ImageUrl))
                {
                    Match match = new Regex(@"(?i)<img\b(?:(?!src=).)*src=(['""]?)(?<src>[^'""\s>]+)\1[^>]*>").Match(post.Content);

                    if (match.Success) post.ImageUrl = match.Groups["src"].Value;
                }

                #endregion

                #region 审核

                //只有第一次发布更新发布时间
                if (post.Status == Status.DRAFT && publish)
                {
                    post.Status = site.NeedAuditPost ? Status.PENDING : Status.PUBLISHED;

                    if (post.Status == Status.PUBLISHED)
                    {
                        post.PublishUserId = jc.UserName;
                        post.DatePublished = DateTime.Now;
                    }
                }

                #endregion

                //OnBeforeSave
                post.OnBeforeSave(new Posts.BeforeSaveEventArgs { Properties = jc.Params });

                cx.SubmitChanges();

                //OnAfterSave
                post.OnAfterSave(new Posts.AfterSaveEventArgs());

                return new { code = 1, id = post.Id, msg = "保存成功", is_pending = post.Status == Status.PENDING };
            }
        }

        /// <summary>
        /// 文章移至回收站
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="ids">文章ID数组</param>
        /// <returns>
        /// {
        ///     code = 1,           //-1：要删除的文章ID不能为空，-3：指定的文章不存在，2：文章已被发布，是否确认删除
        ///     msg = "移至回收站成功"
        /// }
        /// </returns>
        /// leixu
        /// 2016年6月30日20:30:59
        [HttpPost]
        object delete(string[] ids, bool confirmed)
        {
            if (ids.Length == 0) return new { code = -1, msg = "要删除的文章ID不能为空" };

            var site = (Site)jc["site"];

            using (ILinqContext<Posts> cx = Posts.CreateContext())
            {
                var posts = (from q in cx
                             where new List<string>(ids).Contains(q.Id) && q.SiteId == site.Id && q.IsDeleted == false
                             select q).ToList();

                if (posts.Count == 1 && !confirmed && posts[0].Status == Status.PUBLISHED) return new { code = 2, msg = "文章已被发布，是否确认删除" };

                foreach (var post in posts)
                {
                    post.IsDeleted = true;
                }

                cx.SubmitChanges();
            }

            return new { code = 1, msg = "移至回收站成功" };
        }

        /// <summary>
        /// 置顶文章
        /// 注：一个栏目下仅仅有一个置顶文章
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="id">文章ID</param>
        /// <returns>
        /// {
        ///     code = 1,           //-1：要置顶的文章ID不能为空，-2：要置顶的文章未找到，置顶失败
        ///     msg = "置顶成功"
        /// }
        /// </returns>
        /// leixu
        /// 2016年10月9日11:29:21
        [HttpPost]
        object top(string id)
        {
            if (string.IsNullOrEmpty(id)) return new { code = -1, msg = "要置顶的文章ID不能为空" };

            var site = (Site)jc["site"];

            using (ILinqContext<Posts> cx = Posts.CreateContext())
            {
                var post = (from q in cx
                            where q.Id == id && q.SiteId == site.Id && q.IsDeleted == false
                            select q).FirstOrDefault();

                if (post == null) return new { code = -2, msg = "要置顶的文章未找到，置顶失败" };

                //设置当前栏目下置顶文章为不置顶，再更新当前文章为置顶状态
                Posts.Where("CategoryId = {0}", post.CategoryId).Set("IsTop", false).Update();

                post.IsTop = true;

                cx.SubmitChanges();
            }

            return new { code = 1, msg = "置顶成功" };
        }

        /// <summary>
        /// 取消置顶文章
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="id">文章ID</param>
        /// <returns>
        /// {
        ///     code = 1,           //-1：要取消置顶的文章ID不能为空，-2：要取消置顶的文章未找到，取消置顶失败
        ///     msg = "取消成功"
        /// }
        /// </returns>
        /// leixu
        /// 2016年10月9日11:29:21
        [HttpPost]
        object un_top(string id)
        {
            if (string.IsNullOrEmpty(id)) return new { code = -1, msg = "要取消置顶的文章ID不能为空" };

            var site = (Site)jc["site"];

            using (ILinqContext<Posts> cx = Posts.CreateContext())
            {
                var post = (from q in cx
                            where q.Id == id && q.SiteId == site.Id && q.IsDeleted == false
                            select q).FirstOrDefault();

                if (post == null) return new { code = -2, msg = "要取消置顶的文章未找到，取消置顶失败" };

                post.IsTop = false;

                cx.SubmitChanges();
            }

            return new { code = 1, msg = "取消置顶成功" };
        }

        #endregion

        #region 发布

        /// <summary>
        /// 已发布文章列表
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="key">关键字</param>
        /// <param name="category">栏目ID</param>
        /// <returns>
        /// {
        ///     code = 1,                           //1：获取成功
        ///     data = 
        ///     [
        ///         {
        ///             id = "",                    //文章ID
        ///             title = "",                 //文章标题
        ///             summary = "",               //文章的摘要
        ///             category = "",              //文章的分类信息
        ///             date_created = "",          //文章的创建时间
        ///             view_count = "",            //文章的查看数
        ///             sort_order = "",            //文章的排序
        ///             date_published = ""         //文章发布时间
        ///         }
        ///     ],
        ///     totalCount = q.TotalCount,
        ///     page = q.PageIndex1,
        ///     orderbys = q.orderbys
        /// }
        /// </returns>
        /// leixu
        /// 2016年7月1日17:37:13
        [HttpPost]
        object publish_list(string key, string category)
        {
            var site = (Site)jc["site"];

            WebQuery q = new WebQuery();
            q.Id = "posts.publish_list";
            q.LoadCondidtion();

            if (!string.IsNullOrEmpty(key)) q["key"] = key;
            if (!string.IsNullOrEmpty(category)) q["category"] = category;

            q["status"] = (int)Status.PUBLISHED;

            q.TotalCount = Posts.Count(q);
            if (q.PageIndex1 > q.PageCount) q.PageIndex = Math.Max(q.PageCount - 1, 0);

            q["siteId"] = site.Id;

            var dt = Posts.GetDataTable(q);
            var data = new ArrayList();

            foreach (DataRow item in dt.Rows)
            {
                data.Add(new
                {
                    id = item["id"].ToString(),
                    site_id = site.Id,
                    title = item["title"].ToString(),
                    category = item["category"] is DBNull ? string.Empty : item["category"].ToString(),
                    date_created = item["dateCreated"].ToDateTime(),
                    view_count = item["viewCount"].ToInt(),
                    sort_order = item["sortOrder"].ToInt(),
                    date_published = item["datePublished"].ToDateTime()
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

        /// <summary>
        /// 发布文章
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="ids">文章IDS</param>
        /// <returns>
        /// {
        ///     code = 1,           //-1：要发布的文章不能为空，-2：指定的文章未查询到
        ///     msg = "发布成功"
        /// }
        /// </returns>
        /// leixu
        /// 2016年6月30日20:44:37
        [HttpPost]
        object publish(string[] ids)
        {
            if (ids.Length == 0) return new { code = -1, msg = "要发布的文章不能为空" };

            var site = (Site)jc["site"];

            using (ILinqContext<Posts> cx = Posts.CreateContext())
            {
                var posts = (from q in cx
                             where new List<string>(ids).Contains(q.Id) && q.SiteId == site.Id && q.Status == Status.DRAFT && q.IsDeleted == false
                             select q).ToList();

                if (posts.Count == 0) return new { code = -2, msg = "指定的未发布文章未查询到，文章可能已经被删除或者已发布" };

                foreach (var item in posts)
                {
                    item.Status = Status.PUBLISHED;
                    item.PublishUserId = jc.UserName;
                    item.DatePublished = DateTime.Now;
                }

                cx.SubmitChanges(true);
            }

            return new { code = 1, msg = "发布成功" };
        }

        /// <summary>
        /// 取消发布文章
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="ids">文章IDS</param>
        /// <returns>
        /// {
        ///     code = 1,           //-1：要取消发布的文章不能为空，-2：指定的文章未查询到
        ///     msg = "发布成功"
        /// }
        /// </returns>
        /// leixu
        /// 2016年6月30日20:46:26
        [HttpPost]
        object unpublish(string[] ids)
        {
            if (ids.Length == 0) return new { code = -1, msg = "要取消发布的文章不能为空" };

            var site = (Site)jc["site"];

            using (ILinqContext<Posts> cx = Posts.CreateContext())
            {
                var posts = (from q in cx
                             where new List<string>(ids).Contains(q.Id) && q.SiteId == site.Id && q.Status == Status.PUBLISHED
                             select q).ToList();

                if (posts.Count == 0) return new { code = -2, msg = "指定的文章未查询到" };

                foreach (var item in posts)
                {
                    item.Status = Status.DRAFT;
                    item.PublishUserId = string.Empty;
                    item.DatePublished = DateTime.MinValue;
                }

                cx.SubmitChanges(true);
            }

            return new { code = 1, msg = "发布成功" };
        }

        #endregion

        #region 审核

        /// <summary>
        /// 待审核的文章列表
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="key"></param>
        /// <returns>
        /// {
        ///     code = 1,                           //1：获取成功
        ///     data = 
        ///     [
        ///         {
        ///             id = "",                    //文章ID
        ///             title = "",                 //文章标题
        ///             category = "",              //文章的分类信息
        ///             date_created = "",          //文章的创建时间
        ///             view_count = "",            //文章的查看数
        ///             sort_order = "",            //文章的排序
        ///             display_name = "",          //文章创建者
        ///         }
        ///     ],
        ///     totalCount = q.TotalCount,
        ///     page = q.PageIndex1,
        ///     orderbys = q.orderbys
        /// }
        /// </returns>
        /// leixu
        /// 2016年7月14日10:14:54
        [HttpPost]
        object audit(string key)
        {
            var site = (Site)jc["site"];

            WebQuery q = new WebQuery();
            q.Id = "audit.list";
            q.LoadCondidtion();

            if (!string.IsNullOrEmpty(key)) q["key"] = key;

            q.TotalCount = Posts.Count(q);
            if (q.PageIndex1 > q.PageCount) q.PageIndex = Math.Max(q.PageCount - 1, 0);

            q["siteId"] = site.Id;

            var dt = Posts.GetDataTable(q);
            var data = new ArrayList();

            foreach (DataRow item in dt.Rows)
            {
                data.Add(new
                {
                    id = item["id"].ToString(),
                    title = item["title"].ToString(),
                    category = item["category"] is DBNull ? string.Empty : item["category"].ToString(),
                    date_created = item["dateCreated"].ToDateTime(),
                    view_count = item["viewCount"].ToInt(),
                    sort_order = item["sortOrder"].ToInt(),
                    display_name = item["displayName"] is DBNull ? "未知用户" : item["displayName"].ToString()
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

        /// <summary>
        ///  审核文章
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="ids">要审核的文章ID，数组</param>
        /// <param name="pass">1：审核通过，0：审核不通过</param>
        /// <returns>
        /// {
        ///     code = 1,                      //-1：要审核的文章ID不能为空，-2：指定的文章ID在当前站点下未找到，可能文章已经审核过，请刷新后尝试
        ///     msg = "审核成功",
        ///     count = 0                      //审核成功的文章数量
        /// }
        /// </returns>
        /// leixu
        /// 2016年7月14日10:14:50
        [HttpPost]
        object update_audit(string[] ids, bool pass)
        {
            if (ids.Length == 0) return new { code = -1, msg = "要审核的文章ID不能为空" };

            var site = (Site)jc["site"];

            using (ILinqContext<Posts> cx = Posts.CreateContext())
            {
                var posts = (from q in cx
                             where q.SiteId == site.Id && new List<string>(ids).Contains(q.Id) && q.Status == Status.PENDING
                             select q).ToList();

                if (posts.Count == 0) return new { code = -2, msg = "指定的文章ID在当前站点下未找到，可能文章已经审核过，请刷新后尝试" };

                foreach (var item in posts)
                {
                    if (pass)
                    {
                        item.Status = Status.PUBLISHED;
                        item.DatePublished = DateTime.Now;
                        item.PublishUserId = jc.UserName;
                    }
                    else
                    {
                        //TODO 

                        item.Status = Status.AUDIT_FAILD;
                    }
                }

                cx.SubmitChanges(true);

                return new { code = 1, msg = "审核成功", count = posts.Count };
            }
        }

        #endregion

        #region 回收站

        /// <summary>
        /// 回收站列表
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="key">关键字</param>
        /// <returns>
        /// {
        ///     code = 1,                           //1：获取成功
        ///     data = 
        ///     [
        ///         {
        ///             id = "",                    //文章ID
        ///             title = "",                 //文章标题
        ///             category = "",              //文章的分类信息
        ///             date_created = "",          //文章的创建时间
        ///             view_count = "",            //文章的查看数
        ///             sort_order = "",            //文章的排序
        ///             status = "",                //文章状态
        ///             display_name = ""           //文章创建者
        ///             date_published = ""         //文章发布时间
        ///         }
        ///     ],
        ///     totalCount = q.TotalCount,
        ///     page = q.PageIndex1,
        ///     orderbys = q.orderbys
        /// }
        /// </returns>
        /// leixu
        /// 2016年7月9日14:47:50
        [HttpPost]
        object trash(string key)
        {
            var site = (Site)jc["site"];

            WebQuery q = new WebQuery();
            q.Id = "trash.list";
            q.LoadCondidtion();

            if (!string.IsNullOrEmpty(key)) q["key"] = key;

            q.TotalCount = Posts.Count(q);
            if (q.PageIndex1 > q.PageCount) q.PageIndex = Math.Max(q.PageCount - 1, 0);

            q["siteId"] = site.Id;

            var dt = Posts.GetDataTable(q);
            var data = new ArrayList();

            foreach (DataRow item in dt.Rows)
            {
                data.Add(new
                {
                    id = item["id"].ToString(),
                    title = item["title"].ToString(),
                    category = item["category"] is DBNull ? string.Empty : item["category"].ToString(),
                    date_created = item["dateCreated"].ToDateTime(),
                    view_count = item["viewCount"].ToInt(),
                    sort_order = item["sortOrder"].ToInt(),
                    status = StringEnum<Status>.ToString(StringEnum<Status>.SafeParse(item["status"].ToString())),
                    display_name = item["displayName"] is DBNull ? "未知用户" : item["displayName"].ToString(),
                    date_published = item["datePublished"].ToDateTime()
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

        /// <summary>
        /// 彻底删除文章
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="ids">要删除的文章ID数组，如果不传递该参数，则默认清空回收站所有文章</param>
        /// <returns>
        /// {
        ///     code = 1,                       //-1：指定的文章未查询到，文章可能已经彻底删除或者未放置回收站
        ///     msg = "成功将指定的文章删除"
        /// }
        /// </returns>
        /// leixu
        /// 2016年7月9日14:52:14
        [HttpPost]
        object delete_completely(string[] ids)
        {
            var site = (Site)jc["site"];

            using (ILinqContext<Posts> cx = Posts.CreateContext())
            {
                var query = (from q in cx
                             where q.SiteId == site.Id && q.IsDeleted == true
                             select q);

                if (ids.Length > 0)
                {
                    query = (from q in cx
                             where new List<string>(ids).Contains(q.Id) && q.SiteId == site.Id && q.IsDeleted == true
                             select q);
                }

                var posts = query.ToList();

                if (posts.Count == 0) return new { code = -2, msg = "指定的文章未查询到，文章可能已经彻底删除或者未放置回收站" };

                foreach (var item in posts)
                {
                    cx.Remove(item);
                }

                cx.SubmitChanges(true);
            }

            return new { code = 1, msg = "成功将指定的文章删除" };
        }

        /// <summary>
        /// 恢复回收站的文章
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="ids">要恢复的文章ID，数组</param>
        /// <returns>
        /// {
        ///     code = 1,               //-1：要恢复的文章ID不能为空，-2：指定的文章ID在当前站点下未找到，请刷新后尝试
        ///     msg = "恢复成功",
        ///     count = 0               //恢复成功的数据数量
        /// }
        /// </returns>
        /// leixu
        /// 2016年7月14日10:17:11
        [HttpPost]
        object restore(string[] ids)
        {
            if (ids.Length == 0) return new { code = -1, msg = "要恢复的文章ID不能为空" };

            var site = (Site)jc["site"];

            using (ILinqContext<Posts> cx = Posts.CreateContext())
            {
                var posts = (from q in cx
                             where q.SiteId == site.Id && new List<string>(ids).Contains(q.Id)
                             select q).ToList();

                if (posts.Count == 0) return new { code = -2, msg = "指定的文章ID在当前站点下未找到，请刷新后尝试" };

                foreach (var item in posts)
                {
                    item.IsDeleted = false;
                }

                cx.SubmitChanges(true);

                return new { code = 1, msg = "恢复成功", count = posts.Count };
            }
        }

        #endregion
    }
}