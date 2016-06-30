using Kiss.Utils;
using Kiss.Web;
using Kiss.Web.Mvc;
using Kiss.Web.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kiss.Components.Site.Web.Controllers
{
    /// <summary>
    /// 栏目管理的控制器接口
    /// </summary>
    class CategoryController : Controller
    {
        public CategoryController()
        {
            BeforeActionExecute += CategoryController_BeforeActionExecute;
        }

        private void CategoryController_BeforeActionExecute(object sender, BeforeActionExecuteEventArgs e)
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
            if (relation == null || (relation.PermissionLevel != PermissionLevel.ADMIN && relation.PermissionLevel != PermissionLevel.AUDIT))
            {
                ResponseUtil.OutputJson(httpContext.Response, new { code = 403, msg = "没有权限访问" });
                e.PreventDefault = true;
                return;
            }

            #endregion

            jc["site"] = site;
        }

        /// <summary>
        /// 获取栏目列表
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="parentId">父级栏目ID</param>
        /// <returns>
        /// {
        ///    code = 1,
        ///    data = 
        ///    [
        ///         {
        ///             id = "",            //栏目ID
        ///             title = "",         //栏目标题
        ///             hasChild = false    //是否有子集
        ///         }
        ///    ]
        /// }
        /// </returns>
        /// leixu
        /// 2016年6月30日16:37:31
        [HttpPost]
        object list(string parentId)
        {
            var site = (Site)jc["site"];

            var data = (from q in Category.CreateContext()
                        where q.ParentId == parentId && q.SiteId == site.Id
                        select new
                        {
                            id = q.Id,
                            title = q.Title,
                            hasChild = q.HasChildren
                        }).ToList();

            return new
            {
                code = 1,
                data = data
            };
        }

        /// <summary>
        /// 获取单个栏目详细信息
        /// </summary>
        /// <param name="id">栏目ID</param>
        /// <returns>
        /// {
        ///     id = "",                //栏目ID
        ///     site_id = "",           //栏目所在站点
        ///     title = "",             //栏目标题
        ///     url = "",               //栏目的URL
        ///     parent_id = "",         //栏目的父级ID
        ///     date_created = "",      //栏目的创建时间
        ///     sort_order = "",        //栏目的排序
        ///     node_path = "",         //栏目的目录路劲
        ///     need_login_read = false //栏目下文章是否需要登录后查看
        /// }
        /// </returns>
        [HttpPost]
        object detail(string id)
        {
            var site = (Site)jc["site"];

            var category = (from q in Category.CreateContext()
                            where q.Id == id && q.SiteId == site.Id
                            select q).FirstOrDefault();

            if (category == null) return new { code = -1, msg = "指定的栏目不存在" };

            return new
            {
                id = category.Id,
                site_id = category.SiteId,
                title = category.Title,
                url = category.Url,
                parent_id = category.ParentId,
                date_created = category.DateCreated,
                sort_order = category.SortOrder,
                node_path = category.NodePath,
                need_login_read = category.NeedLogin2Read
            };
        }

        /// <summary>
        /// 保存栏目信息
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="id">栏目ID</param>
        /// <param name="title">栏目标题</param>
        /// <param name="url">栏目的URL</param>
        /// <param name="parentId">栏目的父级ID</param>
        /// <param name="sortOrder">栏目的序号</param>
        /// <param name="needLogin2Read">栏目下文章是否需要登录后访问</param>
        /// <returns>
        /// {
        ///     code = 1,           //-1：栏目名称不能为空，-2：指定的父级栏目不存在，-3：栏目的标题长度不能大于50个字符，-4：栏目的URL长度不能大于50个字符，-5：已经存在相同的栏目名称，请更换其他栏目名称，-6：已经存在相同的栏目URL，请更换其他栏目URL
        ///     msg = "保存成功"
        /// }
        /// </returns>
        /// leixu
        /// 2016年6月30日15:26:38
        [HttpPost]
        object save(string id, string title, string url, string parentId, int sortOrder, bool needLogin2Read)
        {
            #region 校验参数

            if (string.IsNullOrWhiteSpace(title)) return new { code = -1, msg = "栏目名称不能为空" };

            #region 校验父级栏目是否存在

            Category parent = null;

            if (!string.IsNullOrWhiteSpace(parentId))
            {
                parent = Category.Get(parentId);

                if (parent == null) return new { code = -2, msg = "指定的父级栏目不存在" };
            }

            #endregion

            title = title.Trim();

            if (string.IsNullOrWhiteSpace(url)) url = Kiss.Utils.Pinyin.GetInitials(title);

            if (title.Length > 50) return new { code = -3, msg = "栏目的标题长度不能大于50个字符" };
            if (url.Length > 20) return new { code = -4, msg = "栏目的URL长度不能大于50个字符" };

            var site = (Site)jc["site"];

            #endregion

            using (ILinqContext<Category> cx = Category.CreateContext())
            using (ILinqContext<Category> cx_children = Category.CreateContext())
            {
                var category = (from q in cx
                                where q.Id == id && q.SiteId == site.Id
                                select q).FirstOrDefault();

                if (category == null)
                {
                    if (Category.Where("Title = {0}", title).Where("SiteId = {0}", site.Id).Count() != 0) return new { code = -5, msg = "已经存在相同的栏目名称，请更换其他栏目名称" };
                    if (Category.Where("Url = {0}", url).Where("SiteId = {0}", site.Id).Count() != 0) return new { code = -6, msg = "已经存在相同的栏目URL，请更换其他栏目URL" };
                }
                else
                {
                    if (category.Title != title && Site.Where("Title = {0}", title).Where("SiteId = {0}", site.Id).Count() != 0) return new { code = -5, msg = "已经存在相同的栏目名称，请更换其他栏目名称" };
                    if (category.Url != url && Site.Where("Url = {0}", url).Where("SiteId = {0}", site.Id).Count() != 0) return new { code = -6, msg = "已经存在相同的栏目URL，请更换其他栏目URL" };
                }

                if (category == null)
                {
                    category = new Category();

                    category.Id = StringUtil.UniqueId();
                    category.DateCreated = DateTime.Now;
                    category.UserId = jc.UserName;
                    category.SiteId = site.Id;

                    cx.Add(category, true);
                }

                category.Title = title;
                category.Url = url;
                category.ParentId = parent == null ? string.Empty : parent.Id;
                category.SortOrder = sortOrder;
                category.NeedLogin2Read = needLogin2Read;

                #region 子集栏目信息变更

                var children = (from q in cx_children
                                where q.NodePath.StartsWith(category.NodePath)
                                select q).ToList();

                category.NodePath = parent == null ? category.Id : string.Format("{0}/{1}", parent.NodePath, category.Id);

                foreach (var item in children)
                {
                    item.NodePath = string.Format("{0}/{1}", category.NodePath, item.Id);
                }

                category.HasChildren = children.Count > 0;

                #endregion

                cx.SubmitChanges();
                cx_children.SubmitChanges(true);
            }

            return new { code = 1, msg = "保存成功" };
        }

        /// <summary>
        /// 删除栏目
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="id">栏目ID</param>
        /// <returns>
        /// {
        ///     code = 1,           //-1：指定的栏目不存在，删除失败
        ///     msg = "删除成功"
        /// }
        /// </returns>
        /// leixu
        /// 2016年6月30日16:22:53
        [HttpPost]
        object delete(string id)
        {
            var site = (Site)jc["site"];

            using (ILinqContext<Category> cx = Category.CreateContext())
            {
                var category = (from q in cx
                                where q.Id == id && q.SiteId == site.Id
                                select q).FirstOrDefault();

                if (category == null) return new { code = -1, msg = "指定的栏目不存在，删除失败" };

                if (Category.Where("ParentId = {0}", category.Id).Where("SiteId = {0}", site.Id).Count() > 0) return new { code = -2, msg = "指定的栏目下存在子栏目，不能删除" };

                cx.Remove(category);
                cx.SubmitChanges();
            }

            return new { code = 1, msg = "删除成功" };
        }
    }
}