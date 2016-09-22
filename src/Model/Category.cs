using Kiss.Validation;
using System;

namespace Kiss.Components.Site
{
    [Serializable, OriginalName("gCms_category")]
    public class Category : QueryObject<Category, string>, IExtendable
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
        /// 栏目所在站点
        /// </summary>
        [NotNull(""), Length(50)]
        public string SiteId { get; set; }

        /// <summary>
        /// 栏目的标题
        /// </summary>
        [NotNull(""), Length(50)]
        public string Title { get; set; }

        /// <summary>
        /// 栏目的URL
        /// </summary>
        [NotNull(""), Length(20)]
        public string Url { get; set; }

        /// <summary>
        /// 栏目的父级ID
        /// </summary>
        [NotNull(""), Length(50)]
        public string ParentId { get; set; }

        /// <summary>
        /// 栏目是否有子集栏目
        /// </summary>
        [NotNull(0)]
        public bool HasChildren { get; set; }

        /// <summary>
        /// 栏目的创建者
        /// </summary>
        [NotNull(""), Length(50)]
        public string UserId { get; set; }

        /// <summary>
        /// 栏目创建时间
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// 栏目序号
        /// </summary>
        [NotNull(0)]
        public int SortOrder { get; set; }

        /// <summary>
        /// 栏目的ID路径，用于查询子集信息
        /// </summary>
        [NotNull(""), Length(2000)]
        public string NodePath { get; set; }

        /// <summary>
        /// 查看栏目下文章需要登录
        /// </summary>
        [NotNull(0)]
        public bool NeedLogin2Read { get; set; }

        /// <summary>
        /// 是否显示在菜单中
        /// </summary>
        [NotNull(0)]
        public bool ShowInMenu { get; set; }

        #region extend props

        [NotNull(""), Length(int.MaxValue)]
        public string PropertyName { get; set; }

        [NotNull(""), Length(int.MaxValue)]
        public string PropertyValue { get; set; }

        private ExtendedAttributes _extAttrs;
        [Ignore]
        public ExtendedAttributes ExtAttrs
        {
            get
            {
                if (_extAttrs == null)
                {
                    _extAttrs = new ExtendedAttributes();
                    _extAttrs.SetData(PropertyName, PropertyValue);
                }
                return _extAttrs;
            }
        }

        [Ignore]
        public string this[string key]
        {
            get
            {
                if (ExtAttrs.ExtendedAttributesCount == 0)
                    ExtAttrs.SetData(PropertyName, PropertyValue);
                return ExtAttrs.GetExtendedAttribute(key);
            }
            set
            {
                ExtAttrs.SetExtendedAttribute(key, value);
            }
        }

        public void SerializeExtAttrs()
        {
            SerializerData sd = ExtAttrs.GetSerializerData();

            PropertyName = sd.Keys;
            PropertyValue = sd.Values;
        }

        #endregion
    }
}