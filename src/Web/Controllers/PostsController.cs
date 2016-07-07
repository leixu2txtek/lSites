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
                ResponseUtil.OutputJson(httpContext.Response, new { code = 200, msg = "参数列表不正确，缺少SiteId参数" });
                e.PreventDefault = true;
                return;
            }

            var site = Site.Get(jc.Params["siteId"]);

            if (site == null)
            {
                ResponseUtil.OutputJson(httpContext.Response, new { code = 200, msg = "指定的站点不存在" });
                e.PreventDefault = true;
                return;
            }

            #endregion

            #region 校验用户对站点的权限

            var relation = (from q in SiteUsers.CreateContext()
                            where q.UserId == jc.UserName && q.SiteId == site.Id
                            select q).FirstOrDefault();

            //只有管理人员才可以对站点的挂件进行编辑
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
        /// 文章列表
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="key">关键字</param>
        /// <param name="status">文章状态，为空则表示查询所有状态文章</param>
        /// <returns>
        /// {
        ///         id = "",                    //文章ID
        ///         title = "",                 //文章标题
        ///         summary = "",               //文章的摘要
        ///         category = "",              //文章的分类信息
        ///         date_created = "",          //文章的创建时间
        ///         view_count = "",            //文章的查看数
        ///         sort_order = "",            //文章的排序
        ///         is_published = "",          //文章是否发布
        ///         date_published = ""         //文章发布时间
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

            if (!string.IsNullOrEmpty(key)) q["title"] = key;
            if (!string.IsNullOrEmpty(status)) q["status"] = status;

            q["siteId"] = site.Id;

            var dt = Posts.GetDataTable(q);
            var data = new ArrayList();

            foreach (DataRow item in dt.Rows)
            {
                data.Add(new
                {
                    id = item["id"].ToString(),
                    title = item["title"].ToString(),
                    summary = item["summary"].ToString(),
                    category = item["category"] is DBNull ? string.Empty : item["category"].ToString(),
                    date_created = item["dateCreated"].ToDateTime(),
                    view_count = item["viewCount"].ToInt(),
                    sort_order = item["sortOrder"].ToInt(),
                    is_published = item["isPublished"].ToBoolean(),
                    date_published = item["datePublished"].ToDateTime()
                });
            }

            return new
            {
                code = 1,
                data = data,
                totalCount = q.TotalCount,
                page = q.PageIndex1,
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

            //获取文章的所属栏目信息
            var category = (from q in Category.CreateContext()
                            where q.Id == post.CategoryId
                            select new
                            {
                                id = q.Id,
                                title = q.Title
                            }).FirstOrDefault();

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
                    category = category,
                    date_created = post.DateCreated,
                    view_count = post.ViewCount,
                    sort_order = post.SortOrder,
                    is_published = post.IsPublished,
                    date_published = post.DatePublished
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
        /// <param name="props">扩展字段</param>
        /// <returns>
        /// {
        ///     code = 1,       //-1：文章标题不能为空，-2：文章内容不能为空，-3：指定的分类不存在，-4：摘要的长度不能超过2000个字符，-5：文章标题的长度不能超过50个字符，-6：文章副标题的长度不能超过100个字符，-7：文章的查看次数不能设置为小于0，-8：扩展字段格式不正确
        ///     msg = "保存成功"
        /// }
        /// </returns>
        /// leixu
        /// 2016年6月30日20:19:36
        [HttpPost]
        object save(string id, string title, string subTitle, string content, string summary, string categoryId, int viewCount, int sortOrder, string props)
        {
            #region 校验参数

            if (string.IsNullOrWhiteSpace(title)) return new { code = -1, msg = "文章标题不能为空" };
            if (string.IsNullOrWhiteSpace(content)) return new { code = -2, msg = "文章内容不能为空" };

            title = title.Trim();
            content = content.Trim();
            subTitle = subTitle.Trim();

            if (title.Length > 50) return new { code = -5, msg = "文章标题的长度不能超过50个字符" };
            if (subTitle.Length > 100) return new { code = -6, msg = "文章副标题的长度不能超过100个字符" };
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
                    post.DateCreated = DateTime.Now;
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

                //OnBeforeSave
                post.OnBeforeSave(new Posts.BeforeSaveEventArgs { Properties = props });
                post.SerializeExtAttrs();

                cx.SubmitChanges();

                //OnBeforeSave
                post.OnAfterSave(new Posts.AfterSaveEventArgs());
            }

            return new { code = 1, msg = "保存成功" };
        }

        /// <summary>
        /// 删除文章
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="id">文章ID</param>
        /// <returns>
        /// {
        ///     code = 1,           //-1：指定的文章不存在，-2：文章已被发布，是否确认删除
        ///     msg = "删除成功"
        /// }
        /// </returns>
        /// leixu
        /// 2016年6月30日20:30:59
        [HttpPost]
        object delete(string id, bool confirmed)
        {
            var site = (Site)jc["site"];

            using (ILinqContext<Posts> cx = Posts.CreateContext())
            {
                var post = (from q in cx
                            where q.Id == id && q.SiteId == site.Id
                            select q).FirstOrDefault();

                if (post == null) return new { code = -1, msg = "指定的文章不存在" };

                if (!confirmed && post.IsPublished) return new { code = -2, msg = "文章已被发布，是否确认删除" };

                cx.Remove(post);
                cx.SubmitChanges();
            }

            return new { code = 1, msg = "删除成功" };
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
                             where new List<string>(ids).Contains(q.Id) && q.SiteId == site.Id && q.IsPublished == false
                             select q).ToList();

                if (posts.Count == 0) return new { code = -2, msg = "指定的文章未查询到" };

                foreach (var item in posts)
                {
                    item.IsPublished = true;
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
        object unPublish(string[] ids)
        {
            if (ids.Length == 0) return new { code = -1, msg = "要取消发布的文章不能为空" };

            var site = (Site)jc["site"];

            using (ILinqContext<Posts> cx = Posts.CreateContext())
            {
                var posts = (from q in cx
                             where new List<string>(ids).Contains(q.Id) && q.SiteId == site.Id && q.IsPublished == true
                             select q).ToList();

                if (posts.Count == 0) return new { code = -2, msg = "指定的文章未查询到" };

                foreach (var item in posts)
                {
                    item.IsPublished = false;
                    item.PublishUserId = string.Empty;
                    item.DatePublished = DateTime.MinValue;
                }

                cx.SubmitChanges(true);
            }

            return new { code = 1, msg = "发布成功" };
        }
    }
}