using Kiss;
using Kiss.Utils;
using Kiss.Web;
using Kiss.Web.Mvc;
using Kiss.Web.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Kiss.Components.Site.Web.Controllers
{
    /// <summary>
    /// 挂件管理的控制器接口
    /// </summary>
    class WidgetController : Controller
    {
        public WidgetController()
        {
            BeforeActionExecute += WidgetController_BeforeActionExecute;
        }

        private void WidgetController_BeforeActionExecute(object sender, BeforeActionExecuteEventArgs e)
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
            if (relation == null || relation.PermissionLevel != PermissionLevel.ADMIN)
            {
                ResponseUtil.OutputJson(httpContext.Response, new { code = 403, msg = "没有权限访问" });
                e.PreventDefault = true;
                return;
            }

            #endregion

            jc["site"] = site;
        }

        #region 查询挂件

        /// <summary>
        /// 获取挂件的详细信息
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="id">挂件ID</param>
        /// <returns>
        /// {
        ///     code = 1,
        ///     widget = 
        ///     {
        ///         id = "",                //挂件ID
        ///         site_id = "",           //挂件所在站点ID
        ///         name = "",              //挂件的名称
        ///         date_created = "",      //挂件创建时间
        ///         props = ""              //挂件的扩展属性，json 字符串
        ///     }
        /// }
        /// </returns>
        /// leixu
        /// 2016年6月30日11:14:41
        [HttpPost]
        object detail(string id)
        {
            var widget = Widget.Get(id);
            if (widget == null) return new { code = -1, msg = "指定的挂件不存在" };

            var props = new Dictionary<string, string>();

            foreach (string item in widget.ExtAttrs.Keys)
            {
                props.Add(item, widget[item]);
            }

            return new
            {
                code = 1,
                widget = new
                {
                    id = widget.Id,
                    site_id = widget.SiteId,
                    name = widget.Name,
                    date_created = widget.DateCreated,
                    props = new Kiss.Json.JavaScriptSerializer().Serialize(props)
                }
            };
        }

        /// <summary>
        /// 挂件列表
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="name">挂件名称（模糊匹配）</param>
        /// <returns></returns>
        /// leixu 
        /// 2016年6月30日11:18:57
        [HttpPost]
        object list(string name)
        {
            WebQuery q = new WebQuery();
            q.Id = "widget.list";
            q.LoadCondidtion();

            if (!string.IsNullOrEmpty(name)) q["name"] = name;

            var dt = Site.GetDataTable(q);
            var data = new ArrayList();

            foreach (DataRow item in dt.Rows)
            {
                data.Add(new
                {
                    id = item["id"].ToString(),
                    site_id = item["siteId"].ToString(),
                    name = item["name"].ToString(),
                    date_created = item["dateCreated"].ToDateTime(),
                    display_name = item["displayName"] is DBNull ? string.Empty : item["displayName"].ToString()
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

        #endregion

        #region 增加 & 修改挂件

        /// <summary>
        /// 保存挂件信息
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="id">挂件ID，修改时使用</param>
        /// <param name="name">挂件名称</param>
        /// <param name="props">扩展字段，例如：{"categoryId": "xxxxxx"}</param>
        /// <returns>
        /// {
        ///     code = 1,       //-1：挂件名称不能为空，-2：挂件名称长度不能超过20个字符，-3，扩展字段格式不正确
        ///     msg = "保存成功"
        /// }
        /// </returns>
        [HttpPost]
        object save(string id, string name, string props)
        {
            #region 参数校验

            if (string.IsNullOrEmpty(name)) return new { code = -1, msg = "挂件名称不能为空" };

            name = name.Trim();
            if (name.Length > 20) return new { code = -2, msg = "挂件名称长度不能超过20个字符" };

            #endregion

            var site = (Site)jc["site"];

            using (ILinqContext<Widget> cx = Widget.CreateContext())
            {
                var widget = Widget.Get(cx, id);

                if (widget == null)
                {
                    widget = new Widget();

                    widget.Id = StringUtil.UniqueId();
                    widget.DateCreated = DateTime.Now;
                    widget.UserId = jc.UserName;
                    widget.SiteId = site.Id;

                    cx.Add(widget, true);
                }

                widget.Name = name;

                #region 处理扩展字段信息

                if (!string.IsNullOrEmpty(props))
                {
                    try
                    {
                        var extends = new Kiss.Json.JavaScriptSerializer().Deserialize<Dictionary<string, string>>(props);

                        foreach (var item in extends.Keys)
                        {
                            widget[item] = extends[item];
                        }

                        //序列化扩展字段
                        widget.SerializeExtAttrs();
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ExceptionUtil.WriteException(ex));

                        return new { code = -3, msg = "扩展字段格式不正确" };
                    }
                }

                #endregion

                cx.SubmitChanges();
            }

            return new { code = 1, msg = "保存成功" };
        }

        #endregion

        #region 删除挂件

        /// <summary>
        /// 根据ID删除挂件
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="id">挂件ID</param>
        /// <returns>
        /// {
        ///     code = 1,           //-1：指定的挂件不存在，删除失败
        ///     msg = "删除成功"
        /// }
        /// </returns>
        /// leixu
        /// 2016年6月30日10:20:50
        [HttpPost]
        object delete(string id)
        {
            using (ILinqContext<Widget> cx = Widget.CreateContext())
            {
                var widget = Widget.Get(cx, id);

                if (widget == null) return new { code = -1, msg = "指定的挂件不存在，删除失败" };

                cx.Remove(widget);
                cx.SubmitChanges();
            }

            return new { code = 1, msg = "删除成功" };
        }

        #endregion
    }
}