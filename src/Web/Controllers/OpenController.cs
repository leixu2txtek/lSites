using Kiss.Web.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
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
    }
}