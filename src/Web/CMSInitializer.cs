using Kiss.Plugin;
using Kiss.Utils;
using System;
using System.Collections.Generic;

namespace Kiss.Components.Site.Web
{
    [AutoInit]
    public class CMSInitializer : IPluginInitializer
    {
        public void Init(ServiceLocator sl, ref PluginSetting setting)
        {
            Posts.View += Posts_View;

            Posts.BeforeSave += Post_BeforeSave;
        }

        void Posts_View(object sender, Posts.ViewEventArgs e)
        {
            throw new NotImplementedException();
        }

        void Post_BeforeSave(object sender, Posts.BeforeSaveEventArgs e)
        {
            Posts post = sender as Posts;

            if (post == null || e.Properties.Count == 0) return;

            #region 处理扩展字段

            if (!string.IsNullOrEmpty(e.Properties["props"]))
            {
                try
                {
                    post.PropertyName = string.Empty;
                    post.PropertyValue = string.Empty;

                    var extends = new Kiss.Json.JavaScriptSerializer().Deserialize<Dictionary<string, string>>(e.Properties["props"]);

                    foreach (var item in extends.Keys)
                    {
                        post[item] = extends[item];
                    }

                    //序列化扩展字段
                    post.SerializeExtAttrs();
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger<CMSInitializer>().Error(ExceptionUtil.WriteException(ex));
                }
            }

            #endregion
        }
    }
}