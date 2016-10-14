using CsQuery;
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
            Posts post = sender as Posts;

            if (post == null) return;

            #region 替换为视频播放器

            CQ cq = CQ.Create(string.Format("<div>{0}</div>", post.Content));

            var videos = cq.Select(".cms_preview_video");

            if (videos.Length > 0)
            {
                foreach (var item in videos)
                {
                    var _cq = item.Cq();
                    var url = item.Attributes["data-url"];

                    //替换为播放器标签
                    _cq.ReplaceWith(string.Format("<video style=\"width: 100%; height: 100%;\" src=\"{0}\" controls=\"controls\"></video>", url));
                }

                post.Content = cq.Html();
            }

            #endregion
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