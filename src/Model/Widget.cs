using Kiss.Validation;
using System;

namespace Kiss.Components.Site
{
    [OriginalName("gWidget"), Serializable]
    public class Widget : QueryObject<Widget, string>, IExtendable
    {
        /// <summary>
        /// 挂件ID
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
        /// 挂件所在站点
        /// </summary>
        [NotNull(""), Length(50)]
        public string SiteId { get; set; }

        /// <summary>
        /// 挂件的名称
        /// </summary>
        [NotNull(""), Length(20)]
        public string Name { get; set; }

        /// <summary>
        /// 挂件的显示标题
        /// </summary>
        [NotNull(""), Length(50)]
        public string Title { get; set; }

        /// <summary>
        /// 挂件的占位标识
        /// </summary>
        [NotNull(""), Length(50)]
        public string ContainerId { get; set; }

        /// <summary>
        /// 创建者
        /// </summary>
        [NotNull(""), Length(50)]
        public string UserId { get; set; }

        /// <summary>
        /// 挂件的创建时间
        /// </summary>
        [NotNull("")]
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int SortOrder { get; set; }

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
