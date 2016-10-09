using Kiss.Validation;
using System;

namespace Kiss.Components.Site
{
    [Serializable, OriginalName("gSite_category_users")]
    public class CategoryUsers : QueryObject<CategoryUsers, string>
    {
        [PK(AutoGen = false), Length(50)]
        public override string Id
        {
            get
            {
                return base.Id;
            }

            set
            {
                base.Id = value;
            }
        }

        /// <summary>
        /// 用户与栏目关系所在站点
        /// </summary>
        [NotNull(""), Length(50)]
        public string SiteId { get; set; }

        /// <summary>
        /// 栏目的ID
        /// </summary>
        [NotNull(""), Length(50)]
        public string CategoryId { get; set; }

        /// <summary>
        /// 管理者的ID
        /// </summary>
        [NotNull(""), Length(50)]
        public string UserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime DateCreated { get; set; }
    }
}