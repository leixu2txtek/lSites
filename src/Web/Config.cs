using Kiss.Config;
using Kiss.Utils;
using System.IO;
using System.Xml;

namespace Kiss.Components.Site.Web
{
    [ConfigNode("cms")]
    public class Config : ConfigBase
    {
        #region 定义图片的限制条件

        /// <summary>
        /// 定义图片的默认上传路径
        /// </summary>
        internal string IMAGE_PATH = FileUtil.FormatDirectory(@"../DATA/IMAGES");

        /// <summary>
        /// 定义图片上传的格式
        /// </summary>
        internal string[] IMAGE_EXTS = new string[] { "JPEG", "JPG", "GIF", "PNG" };

        /// <summary>
        /// 定义图片文件上传的最大容量
        /// </summary>
        internal long IMAGE_MAX_SIZE = 1024 * 1024 * 5;

        #endregion

        #region 定义附件的限制条件

        /// <summary>
        /// 定义附件的默认上传路径
        /// </summary>
        internal string FILE_PATH = FileUtil.FormatDirectory(@"../DATA/FILES");

        /// <summary>
        /// 定义附件上传的格式
        /// </summary>
        internal string[] FILE_EXTS = new string[] { "JPEG", "JPG", "GIF", "PNG" };

        /// <summary>
        /// 定义附件文件上传的最大容量
        /// </summary>
        internal long FILE_MAX_SIZE = 1024 * 1024 * 50;

        #endregion

        #region 定义视频的限制条件

        /// <summary>
        /// 定义视频的默认上传路径
        /// </summary>
        internal string VIDEO_PATH = FileUtil.FormatDirectory(@"../DATA/VIDEOS");

        /// <summary>
        /// 定义视频上传的格式
        /// </summary>
        internal string[] VIDEO_EXTS = new string[] { "MP4" };

        /// <summary>
        /// 定义视频文件上传的最大容量
        /// </summary>
        internal long VIDEO_MAX_SIZE = 1024 * 1024 * 200;

        #endregion

        public static Config Instance { get { return GetConfig<Config>(); } }

        protected override void LoadValuesFromConfigurationXml(XmlNode node)
        {
            base.LoadValuesFromConfigurationXml(node);

            #region 读取图片配置信息

            var image = node.SelectSingleNode("image");

            if (image != null)
            {
                var tmp_path = XmlUtil.GetStringAttribute(image, "path", string.Empty);
                if (!string.IsNullOrWhiteSpace(tmp_path)) IMAGE_PATH = FileUtil.FormatDirectory(tmp_path);

                var tmp_exts = XmlUtil.GetStringAttribute(image, "exts", string.Empty);
                if (!string.IsNullOrWhiteSpace(tmp_exts)) IMAGE_EXTS = StringUtil.Split(tmp_exts, ",", true, true);

                IMAGE_MAX_SIZE = XmlUtil.GetLongAttribute(image, "max_size", IMAGE_MAX_SIZE);
            }

            #endregion

            #region 读取文件配置信息

            var file = node.SelectSingleNode("file");

            if (file != null)
            {
                var tmp_path = XmlUtil.GetStringAttribute(file, "path", string.Empty);
                if (!string.IsNullOrWhiteSpace(tmp_path)) FILE_PATH = FileUtil.FormatDirectory(tmp_path);

                var tmp_exts = XmlUtil.GetStringAttribute(file, "exts", string.Empty);
                if (!string.IsNullOrWhiteSpace(tmp_exts)) FILE_EXTS = StringUtil.Split(tmp_exts, ",", true, true);

                FILE_MAX_SIZE = XmlUtil.GetLongAttribute(file, "max_size", FILE_MAX_SIZE);
            }

            #endregion

            #region 读取视频配置信息

            var video = node.SelectSingleNode("video");

            if (video != null)
            {
                var tmp_path = XmlUtil.GetStringAttribute(video, "path", string.Empty);
                if (!string.IsNullOrWhiteSpace(tmp_path)) VIDEO_PATH = FileUtil.FormatDirectory(tmp_path);

                var tmp_exts = XmlUtil.GetStringAttribute(video, "exts", string.Empty);
                if (!string.IsNullOrWhiteSpace(tmp_exts)) VIDEO_EXTS = StringUtil.Split(tmp_exts, ",", true, true);

                VIDEO_MAX_SIZE = XmlUtil.GetLongAttribute(video, "max_size", VIDEO_MAX_SIZE);
            }

            #endregion
        }
    }
}