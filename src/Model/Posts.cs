﻿using Kiss.Validation;
using System;

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
        /// 是否发布
        /// </summary>
        [NotNull(0)]
        public bool IsPublished { get; set; }

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
    }
}