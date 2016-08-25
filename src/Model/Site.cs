using Kiss.Validation;
using System;

namespace Kiss.Components.Site
{
    [OriginalName("gSite"), Serializable]
    public class Site : QueryObject<Site, string>
    {
        /// <summary>
        /// 站点ID
        /// </summary>
        [PK(AutoGen = false)]
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
        /// 站点的标题
        /// </summary>
        [NotNull("a simple site"), Length(100)]
        public string Title { get; set; }

        /// <summary>
        /// 站点的域名
        /// </summary>
        [NotNull("domain"), Length(100)]
        public string Domain { get; set; }

        /// <summary>
        /// 站点的关键字，用于SEO
        /// </summary>
        [NotNull(""), Length(500)]
        public string KeyWords { get; set; }

        /// <summary>
        /// 站点的描述，用户SEO
        /// </summary>
        [NotNull(""), Length(1000)]
        public string Description { get; set; }

        /// <summary>
        /// 站点logo地址
        /// </summary>
        [NotNull(""), Length(int.MaxValue)]
        public string Logo { get; set; }

        /// <summary>
        /// 站点的主题
        /// </summary>
        [NotNull("default"), Length(20)]
        public string Theme { get; set; }

        /// <summary>
        /// 站点的创建时间
        /// </summary>
        [NotNull("")]
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// 站点的创建人
        /// </summary>
        [NotNull(""), Length(50)]
        public string UserId { get; set; }

        /// <summary>
        /// 站点的序号
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 是否要审核文章
        /// </summary>
        [NotNull(0)]
        public bool NeedAuditPost { get; set; }
    }
}
