using Kiss.Web.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kiss.Components.Site.Web.Controllers
{
    /// <summary>
    /// 无需权限校验的控制器接口
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
        object gets(string[] ids)
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
    }
}