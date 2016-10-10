using Kiss.Components.Security;
using Kiss.Utils;
using Kiss.Web;
using Kiss.Web.Mvc;
using Kiss.Web.Utils;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kiss.Components.Site.Web.Controllers
{
    class UserController : Controller
    {
        public UserController()
        {
            BeforeActionExecute += UsersController_BeforeActionExecute;
        }

        private void UsersController_BeforeActionExecute(object sender, BeforeActionExecuteEventArgs e)
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
        }

        #region 用户信息管理

        /// <summary>
        /// 查询站点下用户信息
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="siteId">站点ID</param>
        /// <param name="displayName">用户姓名</param>
        /// <param name="permission">用户角色</param>
        /// <returns>
        /// {
        ///     code = 1,                           //1：获取成功，-1：指定的站点不存在，-2：指定的站点，没有权限操作
        ///     data = 
        ///     [
        ///         {
        ///             id = "",                    //用户ID
        ///             display_name = "",          //用户显示名称
        ///             mobile = "",                //用户手机
        ///             email = "",                 //用户邮箱
        ///             date_created = "",          //用户创建时间
        ///             permission = "",            //用户的在站点角色
        ///         }
        ///     ],
        ///     totalCount = q.TotalCount,
        ///     page = q.PageIndex1,
        ///     orderbys = q.orderbys
        /// }
        /// </returns>
        /// leixu
        /// 2016年7月27日10:42:09
        [HttpPost]
        object list(string siteId, string displayName, string permission)
        {
            var site = Site.Get(siteId);

            if (site == null) return new { code = -1, msg = "指定的站点不存在" };

            if (!jc.User.IsInRole("admin") && SiteUsers.Where("SiteId = {0}", site.Id)
                                                       .Where("UserId = {0}", jc.UserName)
                                                       .Where("PermissionLevel = {0}", (int)PermissionLevel.ADMIN).Count() == 0)
            {
                return new { code = -2, msg = "指定的站点，没有权限操作" };
            }

            WebQuery q = new WebQuery();
            q.Id = "users.list";
            q.LoadCondidtion();

            if (!string.IsNullOrEmpty(displayName)) q["displayName"] = displayName;
            if (!string.IsNullOrEmpty(displayName)) q["permission"] = permission;

            q.TotalCount = SiteUsers.Count(q);
            if (q.PageIndex1 > q.PageCount) q.PageIndex = Math.Max(q.PageCount - 1, 0);

            var dt = SiteUsers.GetDataTable(q);
            var data = new ArrayList();

            foreach (DataRow item in dt.Rows)
            {
                data.Add(new
                {
                    id = item["userId"].ToString(),
                    user_name = item["userName"] is DBNull ? "用户不存在" : item["userName"].ToString(),
                    display_name = item["displayName"] is DBNull ? "用户不存在" : item["displayName"].ToString(),
                    mobile = item["mobile"] is DBNull ? "用户不存在" : item["mobile"].ToString(),
                    email = item["email"] is DBNull ? "用户不存在" : item["email"].ToString(),
                    post_count = item["postCount"].ToInt(),
                    permission = StringEnum<PermissionLevel>.ToString(StringEnum<PermissionLevel>.SafeParse(item["permission"].ToString())),
                    date_created = item["dateCreated"].ToDateTime(),
                    date_last_visit = item["dateLastVisit"].ToDateTime()
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
        /// 查询站点下的用户信息
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="siteId">站点ID</param>
        /// <param name="userId">用户ID</param>
        /// <returns>
        /// {
        ///     code = 1,                           //-1：指定的站点不存在，-2：指定的站点，没有权限操作，-3：指定的用户在该站点下不存在
        ///     data = new
        ///     {
        ///         id = "",                        //用户ID
        ///         user_name = "",                 //用户名
        ///         display_name = "",              //用户显示名
        ///         mobile = "",                    //用户手机号
        ///         email = "",                     //用户邮箱
        ///         permission = ""                 //用户在站点的角色
        ///     }
        /// }
        /// </returns>
        /// leixu
        /// 2016年7月27日10:50:51
        [HttpPost]
        object detail(string siteId, string userId)
        {
            var site = Site.Get(siteId);

            if (site == null) return new { code = -1, msg = "指定的站点不存在" };

            if (!jc.User.IsInRole("admin") && SiteUsers.Where("SiteId = {0}", site.Id)
                                                       .Where("UserId = {0}", jc.UserName)
                                                       .Where("PermissionLevel = {0}", (int)PermissionLevel.ADMIN).Count() == 0)
            {
                return new { code = -2, msg = "指定的站点，没有权限操作" };
            }

            var relation = (from q in SiteUsers.CreateContext()
                            where q.SiteId == site.Id && q.UserId == userId
                            select q).FirstOrDefault();

            if (relation == null) return new { code = -3, msg = "指定的用户在该站点下不存在" };

            var user = User.Get(relation.UserId);

            if (user == null) return new { code = -3, msg = "指定的用户在该站点下不存在" };

            return new
            {
                code = 1,
                data = new
                {
                    id = user.Id,
                    user_name = user.UserName,
                    display_name = user.DisplayName,
                    mobile = user.Mobile,
                    email = user.Email,
                    permission = (int)relation.PermissionLevel
                }
            };
        }

        /// <summary>
        /// 新增用户至站点
        /// 注：密码默认为 111111
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="siteId">站点ID</param>
        /// <param name="userId">用户ID</param>
        /// <param name="userName">用户名称</param>
        /// <param name="displayName">用户显示名</param>
        /// <param name="mobile">用户手机号</param>
        /// <param name="email">用户邮箱</param>
        /// <param name="permission">用户权限</param>
        /// <returns>
        /// {
        ///     code = 1,                   //-1：指定的站点不存在，-2：指定的站点，没有权限操作，-3：用户名不能为空，-4：显示名称不能为空，-5：用户名字符不能超过50，-6：显示名字符符不能超过50，-7：用户名只能是 英文/数字/下划线 组成，-8：指定的用户名已经存在，请更换其他用户名
        ///     msg = "用户添加成功"
        /// }
        /// </returns>
        /// leixu
        /// 2016年7月27日10:57:26
        [HttpPost]
        object save(string siteId, string userId, string userName, string displayName, string mobile, string email, string permission)
        {
            #region 校验数据

            var site = Site.Get(siteId);

            if (site == null) return new { code = -1, msg = "指定的站点不存在" };

            if (!jc.User.IsInRole("admin") && SiteUsers.Where("SiteId = {0}", site.Id)
                                                       .Where("UserId = {0}", jc.UserName)
                                                       .Where("PermissionLevel = {0}", (int)PermissionLevel.ADMIN).Count() == 0)
            {
                return new { code = -2, msg = "指定的站点，没有权限操作" };
            }

            if (string.IsNullOrWhiteSpace(userName)) return new { code = -3, msg = "用户名不能为空" };
            if (string.IsNullOrWhiteSpace(displayName)) return new { code = -4, msg = "显示名称不能为空" };

            userName = userName.Trim();
            displayName = displayName.Trim();
            mobile = string.IsNullOrWhiteSpace(mobile) ? string.Empty : mobile.Trim();
            email = string.IsNullOrWhiteSpace(email) ? string.Empty : email.Trim();

            if (userName.Length > 50) return new { code = -5, msg = "用户名字符不能超过50" };
            if (displayName.Length > 50) return new { code = -6, msg = "显示名字符符不能超过50" };

            if (!Regex.IsMatch(userName, "^[a-zA-Z0-9_]+$")) return new { code = -7, msg = "用户名只能是 英文/数字/下划线 组成" };

            #endregion

            if (User.Where("UserName = {0}", userName).Count() > 0) return new { code = -8, msg = "指定的用户名已经存在，请更换其他用户名" };

            using (ILinqContext<User> cx = User.CreateContext())
            using (ILinqContext<SiteUsers> cx_relation = SiteUsers.CreateContext())
            {
                #region 构造用户信息

                User user = User.Get(cx, userId);

                if (user == null)
                {
                    user = new User();

                    user.Id = StringUtil.UniqueId();
                    user.DateCreate = DateTime.Now;
                    user.IsValid = true;
                    user.DateLastVisit = DateTime.Now;

                    DictSchema schema = DictSchema.GetByName("users", "config");

                    if (schema != null && schema["first_login_resetpwd"] != null && schema["first_login_resetpwd"].ToBoolean()) user["needmodifyPwd"] = true.ToString();

                    cx.Add(user, true);
                }

                user.UserName = userName;
                user.DisplayName = displayName;
                user.Mobile = mobile;
                user.Email = email;

                //update password
                user.UpdatePassword("111111");

                #endregion

                #region 构造站点用户关系数据

                var relation = (from q in cx_relation
                                where q.SiteId == site.Id && q.UserId == user.Id
                                select q).FirstOrDefault();

                if (relation == null)
                {
                    relation = new SiteUsers();

                    relation.Id = StringUtil.UniqueId();
                    relation.SiteId = site.Id;
                    relation.DateCreated = DateTime.Now;
                    relation.UserId = user.Id;

                    cx_relation.Add(relation, true);
                }

                relation.PermissionLevel = StringEnum<PermissionLevel>.SafeParse(permission);

                #endregion

                cx.SubmitChanges();
                cx_relation.SubmitChanges();
            }

            return new { code = 1, msg = "用户添加成功" };
        }

        /// <summary>
        /// 删除站点下用户
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="siteId">站点ID</param>
        /// <param name="userId">用户ID</param>
        /// <returns>
        /// {
        ///     code = 1,               //-1：指定的站点不存在，-2：指定的站点，没有权限操作，-3：指定的用户在该站点下不存在，-4：不能删除自己的账号
        ///     msg = "删除成功"
        /// }
        /// </returns>
        /// leixu
        /// 2016年7月27日13:57:54
        [HttpPost]
        object delete(string siteId, string userId)
        {
            #region 校验参数

            var site = Site.Get(siteId);

            if (site == null) return new { code = -1, msg = "指定的站点不存在" };

            if (!jc.User.IsInRole("admin") && SiteUsers.Where("SiteId = {0}", site.Id)
                                                       .Where("UserId = {0}", jc.UserName)
                                                       .Where("PermissionLevel = {0}", (int)PermissionLevel.ADMIN).Count() == 0)
            {
                return new { code = -2, msg = "指定的站点，没有权限操作" };
            }

            #endregion

            using (ILinqContext<SiteUsers> cx = SiteUsers.CreateContext())
            {
                var relation = (from q in cx
                                where q.SiteId == site.Id && q.UserId == userId
                                select q).FirstOrDefault();

                if (relation == null) return new { code = -3, msg = "指定的用户在该站点下不存在" };

                if (relation.UserId == jc.UserName) return new { code = -4, msg = "不能删除自己的账号" };

                //删除用户信息
                User.Where("Id = {0}", userId).Delete();

                //删除站点用户关系

                cx.Remove(relation);
                cx.SubmitChanges();
            }

            return new { code = 1, msg = "删除成功" };
        }

        /// <summary>
        /// 更新密码
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="oriPassword">原始密码</param>
        /// <param name="newPassword">新密码</param>
        /// <returns>
        /// {
        ///     code  = 1,          //-1：原始密码不能为空，-2：新密码不能为空，-3：数据错误，请重新登录，-4：原始密码有误，请输入正确的原始密码
        ///     msg = "更新成功"
        /// }
        /// </returns>
        /// leixu
        /// 2016年10月10日09:56:25
        [HttpPost]
        object update_pwd(string oriPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(oriPassword)) return new { code = -1, msg = "原始密码不能为空" };
            if (string.IsNullOrEmpty(newPassword)) return new { code = -2, msg = "新密码不能为空" };

            using (ILinqContext<User> cx = User.CreateContext())
            {
                var user = User.Get(cx, jc.UserName);

                if (user == null) return new { code = -3, msg = "数据错误，请重新登录" };

                //MD5 加密
                var schema = DictSchema.GetByName("users", "config");
                if (schema != null && schema["md5"].ToBoolean()) oriPassword = SecurityUtil.MD5_Hash(oriPassword + schema["md5code"]);

                if (user.Password != oriPassword) return new { code = -4, msg = "原始密码有误，请输入正确的原始密码" };

                user.UpdatePassword(newPassword);

                cx.SubmitChanges();
            }

            return new { code = 1, msg = "更新成功" };
        }

        #endregion

        #region 用户与栏目关系

        /// <summary>
        /// 获取指定用户所管理的栏目信息
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="siteId">站点ID</param>
        /// <param name="userId">用户ID</param>
        /// <returns>
        /// {
        ///     code = 1,                           //1：获取成功，-1：指定的站点不存在，-2：指定的用户不存在，-3：指定的站点，没有权限操作
        ///     data = 
        ///     [
        ///         {
        ///             id = "",                    //栏目与用户关系ID
        ///             category_title = "",        //栏目名称
        ///             date_created = "",          //创建时间
        ///             post_count = "",            //栏目下文章数量
        ///         }
        ///     ],
        ///     totalCount = q.TotalCount,
        ///     page = q.PageIndex1,
        ///     orderbys = q.orderbys
        /// }
        /// </returns>
        /// leixu
        /// 2016年10月10日10:09:02
        [HttpPost]
        object category_list(string siteId, string userId)
        {
            var site = Site.Get(siteId);
            if (site == null) return new { code = -1, msg = "指定的站点不存在" };

            var user = User.Get(userId);
            if (user == null) return new { code = -2, msg = "指定的用户不存在" };

            if (!jc.User.IsInRole("admin") && SiteUsers.Where("SiteId = {0}", site.Id)
                                                       .Where("UserId = {0}", jc.UserName)
                                                       .Where("PermissionLevel = {0}", (int)PermissionLevel.ADMIN).Count() == 0)
            {
                return new { code = -3, msg = "指定的站点，没有权限操作" };
            }

            WebQuery q = new WebQuery();
            q.Id = "users.category.list";
            q.LoadCondidtion();

            q["siteId"] = site.Id;
            q["userId"] = user.Id;

            q.TotalCount = CategoryUsers.Count(q);
            if (q.PageIndex1 > q.PageCount) q.PageIndex = Math.Max(q.PageCount - 1, 0);

            var dt = CategoryUsers.GetDataTable(q);
            var data = new ArrayList();

            foreach (DataRow item in dt.Rows)
            {
                data.Add(new
                {
                    id = item["id"].ToString(),
                    category_title = item["title"].ToString(),
                    post_count = item["postCount"].ToInt(),
                    date_created = item["dateCreated"].ToDateTime(),
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
        /// 增加栏目与用户的关系
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="siteId">站点ID</param>
        /// <param name="userId">用户ID</param>
        /// <param name="categoryId">栏目ID</param>
        /// <returns>
        /// {
        ///     code = 1,               //-1：指定的站点不存在，-2：指定的用户不存在，-3：指定的栏目不存在，-4：指定的站点，没有权限操作
        ///     msg = "保存成功"
        /// }
        /// </returns>
        /// leixu
        /// 2016年10月10日10:45:38
        [HttpPost]
        object add_category_user(string siteId, string userId, string categoryId)
        {
            #region 校验参数

            var site = Site.Get(siteId);
            if (site == null) return new { code = -1, msg = "指定的站点不存在" };

            var user = User.Get(userId);
            if (user == null) return new { code = -2, msg = "指定的用户不存在" };

            var category = (from q in Category.CreateContext()
                            where q.SiteId == site.Id && q.Id == categoryId
                            select q).FirstOrDefault();

            if (category == null) return new { code = -3, msg = "指定的栏目不存在" };

            if (!jc.User.IsInRole("admin") && SiteUsers.Where("SiteId = {0}", site.Id)
                                                       .Where("UserId = {0}", jc.UserName)
                                                       .Where("PermissionLevel = {0}", (int)PermissionLevel.ADMIN).Count() == 0)
            {
                return new { code = -4, msg = "指定的站点，没有权限操作" };
            }

            #endregion

            using (ILinqContext<CategoryUsers> cx = CategoryUsers.CreateContext())
            {
                var relation = (from q in cx
                                where q.SiteId == site.Id && q.UserId == user.Id && q.CategoryId == category.Id
                                select q).FirstOrDefault();

                if (relation == null)
                {
                    relation = new CategoryUsers();

                    relation.Id = StringUtil.UniqueId();
                    relation.CategoryId = category.Id;
                    relation.UserId = user.Id;
                    relation.SiteId = site.Id;
                    relation.DateCreated = DateTime.Now;

                    cx.Add(relation, true);
                }

                cx.SubmitChanges();
            }

            return new { code = 1, msg = "保存成功" };
        }

        /// <summary>
        /// 删除栏目与用户的关系
        /// </summary>
        /// <remarks>请求方式：POST</remarks>
        /// <param name="siteId">站点ID</param>
        /// <param name="id">栏目与用户关系ID</param>
        /// <returns>
        /// {
        ///     code = 1,           //-1：指定的站点不存在，-2：指定的站点，没有权限操作，-3：指定的栏目与用户的关系不存在
        ///     msg = "删除成功"
        /// }
        /// </returns>
        [HttpPost]
        object delete_category_user(string siteId, string id)
        {
            #region 校验参数

            var site = Site.Get(siteId);
            if (site == null) return new { code = -1, msg = "指定的站点不存在" };

            if (!jc.User.IsInRole("admin") && SiteUsers.Where("SiteId = {0}", site.Id)
                                                       .Where("UserId = {0}", jc.UserName)
                                                       .Where("PermissionLevel = {0}", (int)PermissionLevel.ADMIN).Count() == 0)
            {
                return new { code = -2, msg = "指定的站点，没有权限操作" };
            }

            #endregion

            using (ILinqContext<CategoryUsers> cx = CategoryUsers.CreateContext())
            {
                var relation = (from q in cx
                                where q.SiteId == site.Id && q.Id == id
                                select q).FirstOrDefault();

                if (relation == null) return new { code = -3, msg = "指定的栏目与用户的关系不存在" };

                //TODO 之前创建的文章的作者如何处理？

                cx.Remove(relation);
                cx.SubmitChanges();
            }

            return new { code = 1, msg = "删除成功" };
        }

        #endregion
    }
}