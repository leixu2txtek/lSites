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
        /// 根据ids获取挂件内容信息
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="ids">挂件ID数组</param>
        /// <returns>
        /// {
        ///     code = 1,
        ///     widgets = 
        ///     [
        ///         {
        ///             id = "",                //挂件ID
        ///             site_id = "",           //挂件所在站点ID
        ///             name = "",              //挂件的名称
        ///             date_created = "",      //挂件创建时间
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
            var widgets = Widget.Gets(ids);

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
                    id = widget.Id,
                    site_id = widget.SiteId,
                    name = widget.Name,
                    date_created = widget.DateCreated,
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
        ///             parent_id = "",     //父级ID
        ///             node_path = ""      //id路径
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
                                  parent_id = q.ParentId,
                                  node_path = q.NodePath
                              }).ToList();

            return new
            {
                code = 1,
                data = categories
            };
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
        //[HttpPost]
        object get_user_info()
        {
            if (!jc.IsAuth) return new { code = -1, msg = "未登录，没有权限获取信息" };

            User user = User.Get(jc.UserName);

            Site.Get("1");
            SiteUsers.Get("1");

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