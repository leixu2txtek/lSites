using Kiss.Json;
using Kiss.Utils;
using Kiss.Web;
using Kiss.Web.Mvc;
using Kiss.Web.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

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

            jc["site"] = site;
        }

        /// <summary>
        /// 获取栏目列表
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="parentId">父级栏目ID</param>
        /// <returns>
        /// {
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

            #region 校验用户对站点的权限

            var relation = (from q in SiteUsers.CreateContext()
                            where q.UserId == jc.UserName && q.SiteId == site.Id
                            select q).FirstOrDefault();

            //如果没有站点的管理权限
            if (relation == null || (relation.PermissionLevel != PermissionLevel.ADMIN && relation.PermissionLevel != PermissionLevel.AUDIT)) return new { code = 403, msg = "没有权限访问" };

            #endregion

            var data = (from q in Category.CreateContext()
                        where q.ParentId == (string.IsNullOrWhiteSpace(parentId) ? string.Empty : parentId) && q.SiteId == site.Id
                        orderby q.SortOrder ascending, q.Title ascending
                        select new
                        {
                            id = q.Id,
                            name = q.Title,
                            isParent = q.HasChildren
                        }).ToList();

            return data;
        }

        /// <summary>
        /// 获取栏目列表
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="parentId">父级栏目ID</param>
        /// <returns>
        /// {
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
        object list_with_permission(string parentId)
        {
            var site = (Site)jc["site"];

            #region 校验用户对站点的权限

            var relation = (from q in SiteUsers.CreateContext()
                            where q.UserId == jc.UserName && q.SiteId == site.Id
                            select q).FirstOrDefault();

            //如果没有站点的管理权限
            if (relation == null) return new { code = 403, msg = "没有权限访问" };

            #endregion

            WebQuery qc = new WebQuery();
            qc.Id = "category.list.permission";
            qc.LoadCondidtion();
            qc.NoPaging();

            #region 加载参数

            qc["parentId"] = string.IsNullOrWhiteSpace(parentId) ? string.Empty : parentId;

            qc["site"] = site.Id;
            qc["userId"] = jc.UserName;

            #endregion

            var dt = CategoryUsers.GetDataTable(qc);

            var data = new ArrayList();
            foreach (DataRow item in dt.Rows)
            {
                data.Add(new
                {
                    id = item["id"].ToString(),
                    name = item["title"].ToString(),
                    isParent = item["hasChildren"].ToBoolean()
                });
            }

            return data;
        }

        /// <summary>
        /// 获取单个栏目详细信息
        /// </summary>
        /// <param name="id">栏目ID</param>
        /// <returns>
        /// {
        ///     code = 1,                           //403：没有权限访问
        ///     data = 
        ///     {
        ///         id = "",                        //栏目ID
        ///         site_id = "",                   //栏目所在站点
        ///         title = "",                     //栏目标题
        ///         url = "",                       //栏目的URL
        ///         parent_id = "",                 //栏目的父级ID
        ///         date_created = "",              //栏目的创建时间
        ///         sort_order = "",                //栏目的排序
        ///         node_path = "",                 //栏目的目录路劲
        ///         need_login_read = false         //栏目下文章是否需要登录后查看
        ///     }
        /// }
        /// </returns>
        [HttpPost]
        object detail(string id)
        {
            var site = (Site)jc["site"];

            #region 校验用户对站点的权限

            var relation = (from q in SiteUsers.CreateContext()
                            where q.UserId == jc.UserName && q.SiteId == site.Id
                            select q).FirstOrDefault();

            //如果没有站点的管理权限
            if (relation == null || relation.PermissionLevel != PermissionLevel.ADMIN) return new { code = 403, msg = "没有权限访问" };

            #endregion

            var category = (from q in Category.CreateContext()
                            where q.Id == id && q.SiteId == site.Id
                            select q).FirstOrDefault();

            if (category == null) return new { code = -1, msg = "指定的栏目不存在" };

            object parent = new { id = string.Empty, title = string.Empty };

            if (!string.IsNullOrEmpty(category.ParentId))
            {
                parent = (from q in Category.CreateContext()
                          where q.Id == category.ParentId
                          select new
                          {
                              id = q.Id,
                              title = q.Title
                          }).FirstOrDefault();
            }

            return new
            {
                code = 1,
                data = new
                {
                    id = category.Id,
                    site_id = category.SiteId,
                    title = category.Title,
                    url = category.Url,
                    parent = parent,
                    date_created = category.DateCreated,
                    sort_order = category.SortOrder,
                    node_path = category.NodePath,
                    need_login_read = category.NeedLogin2Read,
                    show_in_menu = category.ShowInMenu
                }
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
        /// <param name="showInMenu">栏目显示在菜单上</param>
        /// <returns>
        /// {
        ///     code = 1,           //-1：栏目名称不能为空，-2：指定的父级栏目不存在，-3：栏目的标题长度不能大于50个字符，-4：栏目的URL长度不能大于50个字符，-5：已经存在相同的栏目名称，请更换其他栏目名称，-6：已经存在相同的栏目URL，请更换其他栏目URL
        ///     msg = "保存成功"
        /// }
        /// </returns>
        /// leixu
        /// 2016年6月30日15:26:38
        [HttpPost]
        object save(string id, string title, string url, string parentId, int sortOrder, bool needLogin2Read, bool showInMenu)
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

            #region 校验用户对站点的权限

            var relation = (from q in SiteUsers.CreateContext()
                            where q.UserId == jc.UserName && q.SiteId == site.Id
                            select q).FirstOrDefault();

            //如果没有站点的管理权限
            if (relation == null || relation.PermissionLevel != PermissionLevel.ADMIN) return new { code = 403, msg = "没有权限访问" };

            #endregion

            #endregion

            using (ILinqContext<Category> cx = Category.CreateContext())
            using (ILinqContext<Category> cx_children = Category.CreateContext())
            {
                var category = (from q in cx
                                where q.Id == id && q.SiteId == site.Id
                                select q).FirstOrDefault();

                if (category == null)
                {
                    if (Category.Where("Title = {0}", title).Where("SiteId = {0}", site.Id).Where("ParentId = {0}", (parent == null ? string.Empty : parent.Id)).Count() != 0) return new { code = -5, msg = "已经存在相同的栏目名称，请更换其他栏目名称" };
                    if (Category.Where("Url = {0}", url).Where("SiteId = {0}", site.Id).Where("ParentId = {0}", (parent == null ? string.Empty : parent.Id)).Count() != 0) return new { code = -6, msg = "已经存在相同的栏目URL，请更换其他栏目URL" };
                }
                else
                {
                    if (category.Title != title && Category.Where("Title = {0}", title).Where("SiteId = {0}", site.Id).Where("ParentId = {0}", (parent == null ? string.Empty : parent.Id)).Count() != 0) return new { code = -5, msg = "已经存在相同的栏目名称，请更换其他栏目名称" };
                    if (category.Url != url && Category.Where("Url = {0}", url).Where("SiteId = {0}", site.Id).Where("ParentId = {0}", (parent == null ? string.Empty : parent.Id)).Count() != 0) return new { code = -6, msg = "已经存在相同的栏目URL，请更换其他栏目URL" };
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
                category.ShowInMenu = showInMenu;

                #region 子集栏目信息变更

                category.NodePath = parent == null ? category.Id : string.Format("{0}/{1}", parent.NodePath, category.Id);

                var children = (from q in cx_children
                                where q.NodePath.StartsWith(string.Format("{0}/", category.NodePath))
                                select q).ToList();

                foreach (var item in children)
                {
                    item.NodePath = string.Format("{0}/{1}", category.NodePath, item.Id);
                }

                category.HasChildren = children.Count > 0;

                #endregion

                cx.SubmitChanges();
                cx_children.SubmitChanges(true);

                //将父级栏目更新为有子集
                if (parent != null) Category.Where("Id = {0}", parent.Id).Set("HasChildren", 1).Update();
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
        ///     code = 1,           //-1：指定的栏目不存在，删除失败，-2：指定的栏目下存在子栏目，不能删除，403：没有权限访问
        ///     msg = "删除成功"
        /// }
        /// </returns>
        /// leixu
        /// 2016年6月30日16:22:53
        [HttpPost]
        object delete(string id)
        {
            var site = (Site)jc["site"];

            #region 校验用户对站点的权限

            var relation = (from q in SiteUsers.CreateContext()
                            where q.UserId == jc.UserName && q.SiteId == site.Id
                            select q).FirstOrDefault();

            //如果没有站点的管理权限
            if (relation == null || relation.PermissionLevel != PermissionLevel.ADMIN) return new { code = 403, msg = "没有权限访问" };

            #endregion

            using (ILinqContext<Category> cx = Category.CreateContext())
            {
                var category = (from q in cx
                                where q.Id == id && q.SiteId == site.Id
                                select q).FirstOrDefault();

                if (category == null) return new { code = -1, msg = "指定的栏目不存在，删除失败" };

                if (Category.Where("ParentId = {0}", category.Id).Where("SiteId = {0}", site.Id).Count() > 0) return new { code = -2, msg = "指定的栏目下存在子栏目，不能删除" };

                cx.Remove(category);
                cx.SubmitChanges();

                //更新父级是否有子集
                Category.Where("Id = {0}", category.ParentId).Set("HasChildren", Category.Where("ParentId = {0}", category.ParentId).Count() > 0).Update();
            }

            return new { code = 1, msg = "删除成功" };
        }

        #region 导入 & 导出

        /// <summary>
        /// 导出栏目数据
        /// </summary>
        /// <remarks>请求方式：GET</remarks>
        /// <returns>json 文件流</returns>
        /// leixu
        /// 2016年12月20日14:27:26
        [HttpGet]
        ActionResult export()
        {
            var site = (Site)jc["site"];

            var categories = (from q in Category.CreateContext()
                              where q.SiteId == site.Id
                              select q).ToList();

            if (categories.Count == 0) return new EmptyResult();

            return new FileContentResult(Encoding.UTF8.GetBytes(new JavaScriptSerializer().Serialize(categories)), "application/json") { FileDownloadName = "categories.json" };
        }

        /// <summary>
        /// 导入栏目数据（ID不变更）
        /// 注：同一个系统不能采用该方法导入
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <returns>
        /// {
        ///     code = 1,           //-1：请选择要上传的JSON数据，-2：必须上传一个JSON文件，-3：指定的JSON文件中没有数据
        ///     msg = "导入成功"
        /// }
        /// </returns>
        /// leixu
        /// 2016年12月20日14:28:19
        [HttpPost]
        object import()
        {
            #region 参数验证

            if (jc.Context.Request.Files.Count == 0) return new { code = -1, msg = "请选择要上传的JSON数据" };

            var file = jc.Context.Request.Files[0];

            if (!file.FileName.EndsWith(".json")) return new { code = -2, msg = "必须上传一个JSON文件" };

            var categories = new JavaScriptSerializer().Deserialize<List<Category>>(Encoding.UTF8.GetString(file.InputStream.ToBytes()));

            if (categories.Count == 0) return new { code = -3, msg = "指定的JSON文件中没有数据" };

            //根据层级排序，按层级由浅至深来处理
            categories = (from q in categories
                          orderby StringUtil.Split(q.NodePath, "/", true, true).Count()
                          select q).ToList();

            #endregion

            var site = (Site)jc["site"];

            using (ILinqContext<Category> cx = Category.CreateContext())
            {
                foreach (var item in categories)
                {
                    var category = new Category();

                    category.Id = item.Id;
                    category.DateCreated = DateTime.Now;
                    category.UserId = jc.UserName;
                    category.SiteId = site.Id;

                    category.Title = item.Title;
                    category.Url = item.Url;
                    category.ParentId = item.ParentId;

                    category.SortOrder = item.SortOrder;
                    category.NeedLogin2Read = item.NeedLogin2Read;
                    category.ShowInMenu = item.ShowInMenu;

                    #region 处理子集数据

                    category.HasChildren = item.HasChildren;
                    category.NodePath = item.NodePath.Replace(item.Id, category.Id);

                    if (category.HasChildren)
                    {
                        var children = categories.Where(a => { return a.ParentId == item.Id; }).ToList();

                        foreach (var child in children)
                        {
                            child.ParentId = category.Id;
                            child.NodePath = child.NodePath.Replace(item.NodePath, category.NodePath);
                        }
                    }

                    #endregion

                    #region 扩展字段

                    foreach (string key in item.ExtAttrs.Keys) category[key] = item.ExtAttrs[key];

                    category.SerializeExtAttrs();

                    #endregion

                    cx.Add(category, true);
                }

                cx.SubmitChanges(true);
            }

            return new { code = 1, msg = "导入成功" };
        }

        #endregion
    }
}