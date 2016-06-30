using Kiss.Validation;
using System;
using System.ComponentModel;

namespace Kiss.Components.Site
{
    [Serializable, OriginalName("gSite_users")]
    public class SiteUsers : QueryObject<SiteUsers, string>
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
        /// 站点ID
        /// </summary>
        [NotNull(""), Length(50)]
        public string SiteId { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [NotNull(""), Length(50)]
        public string UserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [NotNull("")]
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// 站点权限
        /// </summary>
        [NotNull(0)]
        public PermissionLevel PermissionLevel { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum PermissionLevel
    {
        /// <summary>
        /// 管理员 0
        /// </summary>
        [Description("管理员")]
        ADMIN = 0,

        /// <summary>
        /// 审核人 1
        /// </summary>
        [Description("审核人")]
        AUDIT = 1,

        /// <summary>
        /// 编辑 2
        /// </summary>
        [Description("编辑")]
        EDIT = 2
    }
}
