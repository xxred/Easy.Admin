using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using XCode;
using XCode.Configuration;
using XCode.DataAccessLayer;

namespace Easy.Admin.Localization.Models
{
    /// <summary>本地化记录</summary>
    [Serializable]
    [DataObject]
    [Description("本地化记录")]
    [BindIndex("IU_LocalizationRecords_Key_ResourceKey_LocalizationCulture", true, "Key,ResourceKey,LocalizationCulture")]
    [BindIndex("IX_LocalizationRecords_ResourceKey", false, "ResourceKey")]
    [BindTable("LocalizationRecords", Description = "本地化记录", ConnName = "EasyAdmin", DbType = DatabaseType.SqlServer)]
    public partial class LocalizationRecords<TEntity> : ILocalizationRecords
    {
        #region 属性
        private Int32 _ID;
        /// <summary>编号</summary>
        [DisplayName("编号")]
        [Description("编号")]
        [DataObjectField(true, true, false, 0)]
        [BindColumn("ID", "编号", "")]
        public Int32 ID { get => _ID; set { if (OnPropertyChanging(__.ID, value)) { _ID = value; OnPropertyChanged(__.ID); } } }

        private String _Key;
        /// <summary>关键字(根据此值换取相应语言文字)</summary>
        [DisplayName("关键字")]
        [Description("关键字(根据此值换取相应语言文字)")]
        [DataObjectField(false, false, false, 50)]
        [BindColumn("Key", "关键字(根据此值换取相应语言文字)", "", Master = true)]
        public String Key { get => _Key; set { if (OnPropertyChanging(__.Key, value)) { _Key = value; OnPropertyChanged(__.Key); } } }

        private String _ResourceKey;
        /// <summary>资源关键字(将资源分类，方便管理)</summary>
        [DisplayName("资源关键字")]
        [Description("资源关键字(将资源分类，方便管理)")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("ResourceKey", "资源关键字(将资源分类，方便管理)", "")]
        public String ResourceKey { get => _ResourceKey; set { if (OnPropertyChanging(__.ResourceKey, value)) { _ResourceKey = value; OnPropertyChanged(__.ResourceKey); } } }

        private String _Text;
        /// <summary>文本</summary>
        [DisplayName("文本")]
        [Description("文本")]
        [DataObjectField(false, false, true, 2000)]
        [BindColumn("Text", "文本", "")]
        public String Text { get => _Text; set { if (OnPropertyChanging(__.Text, value)) { _Text = value; OnPropertyChanged(__.Text); } } }

        private String _LocalizationCulture;
        /// <summary>语言文化名称</summary>
        [DisplayName("语言文化名称")]
        [Description("语言文化名称")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("LocalizationCulture", "语言文化名称", "")]
        public String LocalizationCulture { get => _LocalizationCulture; set { if (OnPropertyChanging(__.LocalizationCulture, value)) { _LocalizationCulture = value; OnPropertyChanged(__.LocalizationCulture); } } }

        private DateTime _CreateTime;
        /// <summary>添加时间</summary>
        [DisplayName("添加时间")]
        [Description("添加时间")]
        [DataObjectField(false, false, true, 0)]
        [BindColumn("CreateTime", "添加时间", "")]
        public DateTime CreateTime { get => _CreateTime; set { if (OnPropertyChanging(__.CreateTime, value)) { _CreateTime = value; OnPropertyChanged(__.CreateTime); } } }

        private DateTime _UpdateTime;
        /// <summary>更新时间</summary>
        [DisplayName("更新时间")]
        [Description("更新时间")]
        [DataObjectField(false, false, true, 0)]
        [BindColumn("UpdateTime", "更新时间", "")]
        public DateTime UpdateTime { get => _UpdateTime; set { if (OnPropertyChanging(__.UpdateTime, value)) { _UpdateTime = value; OnPropertyChanged(__.UpdateTime); } } }
        #endregion

        #region 获取/设置 字段值
        /// <summary>获取/设置 字段值</summary>
        /// <param name="name">字段名</param>
        /// <returns></returns>
        public override Object this[String name]
        {
            get
            {
                switch (name)
                {
                    case __.ID: return _ID;
                    case __.Key: return _Key;
                    case __.ResourceKey: return _ResourceKey;
                    case __.Text: return _Text;
                    case __.LocalizationCulture: return _LocalizationCulture;
                    case __.CreateTime: return _CreateTime;
                    case __.UpdateTime: return _UpdateTime;
                    default: return base[name];
                }
            }
            set
            {
                switch (name)
                {
                    case __.ID: _ID = value.ToInt(); break;
                    case __.Key: _Key = Convert.ToString(value); break;
                    case __.ResourceKey: _ResourceKey = Convert.ToString(value); break;
                    case __.Text: _Text = Convert.ToString(value); break;
                    case __.LocalizationCulture: _LocalizationCulture = Convert.ToString(value); break;
                    case __.CreateTime: _CreateTime = value.ToDateTime(); break;
                    case __.UpdateTime: _UpdateTime = value.ToDateTime(); break;
                    default: base[name] = value; break;
                }
            }
        }
        #endregion

        #region 字段名
        /// <summary>取得本地化记录字段信息的快捷方式</summary>
        public partial class _
        {
            /// <summary>编号</summary>
            public static readonly Field ID = FindByName(__.ID);

            /// <summary>关键字(根据此值换取相应语言文字)</summary>
            public static readonly Field Key = FindByName(__.Key);

            /// <summary>资源关键字(将资源分类，方便管理)</summary>
            public static readonly Field ResourceKey = FindByName(__.ResourceKey);

            /// <summary>文本</summary>
            public static readonly Field Text = FindByName(__.Text);

            /// <summary>语言文化名称</summary>
            public static readonly Field LocalizationCulture = FindByName(__.LocalizationCulture);

            /// <summary>添加时间</summary>
            public static readonly Field CreateTime = FindByName(__.CreateTime);

            /// <summary>更新时间</summary>
            public static readonly Field UpdateTime = FindByName(__.UpdateTime);

            static Field FindByName(String name) => Meta.Table.FindByName(name);
        }

        /// <summary>取得本地化记录字段名称的快捷方式</summary>
        public partial class __
        {
            /// <summary>编号</summary>
            public const String ID = "ID";

            /// <summary>关键字(根据此值换取相应语言文字)</summary>
            public const String Key = "Key";

            /// <summary>资源关键字(将资源分类，方便管理)</summary>
            public const String ResourceKey = "ResourceKey";

            /// <summary>文本</summary>
            public const String Text = "Text";

            /// <summary>语言文化名称</summary>
            public const String LocalizationCulture = "LocalizationCulture";

            /// <summary>添加时间</summary>
            public const String CreateTime = "CreateTime";

            /// <summary>更新时间</summary>
            public const String UpdateTime = "UpdateTime";
        }
        #endregion
    }

    /// <summary>本地化记录接口</summary>
    public partial interface ILocalizationRecords
    {
        #region 属性
        /// <summary>编号</summary>
        Int32 ID { get; set; }

        /// <summary>关键字(根据此值换取相应语言文字)</summary>
        String Key { get; set; }

        /// <summary>资源关键字(将资源分类，方便管理)</summary>
        String ResourceKey { get; set; }

        /// <summary>文本</summary>
        String Text { get; set; }

        /// <summary>语言文化名称</summary>
        String LocalizationCulture { get; set; }

        /// <summary>添加时间</summary>
        DateTime CreateTime { get; set; }

        /// <summary>更新时间</summary>
        DateTime UpdateTime { get; set; }
        #endregion

        #region 获取/设置 字段值
        /// <summary>获取/设置 字段值</summary>
        /// <param name="name">字段名</param>
        /// <returns></returns>
        Object this[String name] { get; set; }
        #endregion
    }
}