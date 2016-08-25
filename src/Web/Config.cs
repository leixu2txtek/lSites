using Kiss.Utils;
using System.IO;

namespace Kiss.Components.Site.Web
{
    public class Config
    {
        #region 定义图片的限制条件

        /// <summary>
        /// 定义图片的默认上传路径
        /// </summary>
        internal static readonly string IMAGE_PATH = FileUtil.FormatDirectory(@"../DATA/IMAGES");

        /// <summary>
        /// 定义图片上传的格式
        /// </summary>
        internal static readonly string[] IMAGE_EXTS = new string[] { "JPEG", "JPG", "GIF", "PNG" };

        /// <summary>
        /// 定义图片文件上传的最大容量
        /// </summary>
        internal static readonly long IMAGE_MAX_SIZE = 1024 * 1024 * 5;

        /// <summary>
        /// 定义404图片
        /// </summary>
        internal static readonly string IMAGE_NOT_FOUND = Path.Combine(IMAGE_PATH, "404.JPG");

        #endregion
    }
}