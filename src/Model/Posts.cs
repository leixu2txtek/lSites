using Kiss.Validation;
using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Kiss.Components.Site
{
    [Serializable, OriginalName("gCms_posts")]
    public class Posts : QueryObject<Posts, string>, IExtendable
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
        /// 文章所属站点
        /// </summary>
        [NotNull(""), Length(50)]
        public string SiteId { get; set; }

        #region post props

        /// <summary>
        /// 文章标题
        /// </summary>
        [NotNull(""), Length(50)]
        public string Title { get; set; }

        /// <summary>
        /// 文章子标题
        /// </summary>
        [NotNull(""), Length(100)]
        public string SubTitle { get; set; }

        /// <summary>
        /// 文章内容
        /// </summary>
        [NotNull(""), Length(int.MaxValue)]
        public string Content { get; set; }

        /// <summary>
        /// 文章内容的纯文本版本
        /// </summary>
        [NotNull(""), Length(int.MaxValue)]
        public string Text { get; set; }

        /// <summary>
        /// 文章的简介内容
        /// </summary>
        [NotNull(""), Length(2000)]
        public string Summary { get; set; }

        /// <summary>
        /// 文章栏目ID
        /// </summary>
        [NotNull(""), Length(50)]
        public string CategoryId { get; set; }

        /// <summary>
        /// 文章的创建时间
        /// </summary>
        [NotNull("")]
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// 文章的查看次数
        /// </summary>
        [NotNull(0)]
        public int ViewCount { get; set; }

        /// <summary>
        /// 文章序号
        /// </summary>
        [NotNull(0)]
        public int SortOrder { get; set; }

        /// <summary>
        /// 文章创建者
        /// </summary>
        [NotNull(""), Length(50)]
        public string UserId { get; set; }

        /// <summary>
        /// 文章创建者显示名
        /// </summary>
        [NotNull(""), Length(50)]
        public string DisplayName { get; set; }

        /// <summary>
        /// 文章状态
        /// </summary>
        [NotNull(0)]
        public Status Status { get; set; }

        /// <summary>
        /// 文章发布时间
        /// </summary>
        [NotNull("")]
        public DateTime DatePublished { get; set; }

        /// <summary>
        /// 文章发布者
        /// </summary>
        [NotNull(""), Length(50)]
        public string PublishUserId { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        [NotNull(0)]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 文章的第一个图片
        /// </summary>
        [NotNull(""), Length(500)]
        public string ImageUrl { get; set; }

        #endregion

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

        #region events

        public class ViewEventArgs : EventArgs
        {
            public static readonly new ViewEventArgs Empty = new ViewEventArgs();
        }

        public static event EventHandler<ViewEventArgs> View;

        public void OnView(ViewEventArgs e)
        {
            var handler = View;

            if (handler != null) handler(this, e);
        }

        public class AfterSaveEventArgs : EventArgs
        {
            public static readonly new BeforeSaveEventArgs Empty = new BeforeSaveEventArgs();
        }

        public static event EventHandler<AfterSaveEventArgs> AfterSave;

        public void OnAfterSave(AfterSaveEventArgs e)
        {
            var handler = AfterSave;

            if (handler != null) handler(this, e);
        }

        public class BeforeSaveEventArgs : EventArgs
        {
            public static readonly new BeforeSaveEventArgs Empty = new BeforeSaveEventArgs();

            public NameValueCollection Properties { get; set; }
        }

        public static event EventHandler<BeforeSaveEventArgs> BeforeSave;

        public void OnBeforeSave(BeforeSaveEventArgs e)
        {
            var handler = BeforeSave;

            if (handler != null) handler(this, e);
        }

        #endregion
    }

    public enum Status
    {
        /// <summary>
        /// 草稿 0
        /// </summary>
        [Description("草稿")]
        DRAFT = 0,

        /// <summary>
        /// 待审核 1
        /// </summary>
        [Description("待审核")]
        PENDING = 1,

        /// <summary>
        /// 审核失败 -1
        /// </summary>
        [Description("审核失败")]
        AUDIT_FAILD = -1,

        /// <summary>
        /// 已发布 2
        /// </summary>
        [Description("已发布")]
        PUBLISHED = 2
    }
}
