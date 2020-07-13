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
    /// <summary>语言</summary>
    [Serializable]
    [DataObject]
    [Description("语言")]
    [BindIndex("IU_Languages_LanguageCultureName", true, "LanguageCultureName")]
    [BindTable("Languages", Description = "语言", ConnName = "EasyAdmin", DbType = DatabaseType.SqlServer)]
    public partial class Languages<TEntity> : ILanguages
    {
        #region 属性
        private Int32 _ID;
        /// <summary>编号</summary>
        [DisplayName("编号")]
        [Description("编号")]
        [DataObjectField(true, true, false, 0)]
        [BindColumn("ID", "编号", "")]
        public Int32 ID { get => _ID; set { if (OnPropertyChanging(__.ID, value)) { _ID = value; OnPropertyChanged(__.ID); } } }

        private String _LanguageCultureName;
        /// <summary>语言文化名称</summary>
        [DisplayName("语言文化名称")]
        [Description("语言文化名称")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("LanguageCultureName", "语言文化名称", "")]
        public String LanguageCultureName { get => _LanguageCultureName; set { if (OnPropertyChanging(__.LanguageCultureName, value)) { _LanguageCultureName = value; OnPropertyChanged(__.LanguageCultureName); } } }

        private String _DisplayName;
        /// <summary>语言展示名</summary>
        [DisplayName("语言展示名")]
        [Description("语言展示名")]
        [DataObjectField(false, false, false, 50)]
        [BindColumn("DisplayName", "语言展示名", "", Master = true)]
        public String DisplayName { get => _DisplayName; set { if (OnPropertyChanging(__.DisplayName, value)) { _DisplayName = value; OnPropertyChanged(__.DisplayName); } } }

        private String _EnglishName;
        /// <summary>英语区域名</summary>
        [DisplayName("英语区域名")]
        [Description("英语区域名")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("EnglishName", "英语区域名", "")]
        public String EnglishName { get => _EnglishName; set { if (OnPropertyChanging(__.EnglishName, value)) { _EnglishName = value; OnPropertyChanged(__.EnglishName); } } }

        private String _NativeName;
        /// <summary>区域性名称</summary>
        [DisplayName("区域性名称")]
        [Description("区域性名称")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("NativeName", "区域性名称", "")]
        public String NativeName { get => _NativeName; set { if (OnPropertyChanging(__.NativeName, value)) { _NativeName = value; OnPropertyChanged(__.NativeName); } } }

        private String _ISO639xValue;
        /// <summary>全球语言标准码</summary>
        [DisplayName("全球语言标准码")]
        [Description("全球语言标准码")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("ISO639xValue", "全球语言标准码", "")]
        public String ISO639xValue { get => _ISO639xValue; set { if (OnPropertyChanging(__.ISO639xValue, value)) { _ISO639xValue = value; OnPropertyChanged(__.ISO639xValue); } } }

        private Boolean _Enable;
        /// <summary>启用</summary>
        [DisplayName("启用")]
        [Description("启用")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("Enable", "启用", "")]
        public Boolean Enable { get => _Enable; set { if (OnPropertyChanging(__.Enable, value)) { _Enable = value; OnPropertyChanged(__.Enable); } } }
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
                    case __.LanguageCultureName: return _LanguageCultureName;
                    case __.DisplayName: return _DisplayName;
                    case __.EnglishName: return _EnglishName;
                    case __.NativeName: return _NativeName;
                    case __.ISO639xValue: return _ISO639xValue;
                    case __.Enable: return _Enable;
                    default: return base[name];
                }
            }
            set
            {
                switch (name)
                {
                    case __.ID: _ID = value.ToInt(); break;
                    case __.LanguageCultureName: _LanguageCultureName = Convert.ToString(value); break;
                    case __.DisplayName: _DisplayName = Convert.ToString(value); break;
                    case __.EnglishName: _EnglishName = Convert.ToString(value); break;
                    case __.NativeName: _NativeName = Convert.ToString(value); break;
                    case __.ISO639xValue: _ISO639xValue = Convert.ToString(value); break;
                    case __.Enable: _Enable = value.ToBoolean(); break;
                    default: base[name] = value; break;
                }
            }
        }
        #endregion

        #region 字段名
        /// <summary>取得语言字段信息的快捷方式</summary>
        public partial class _
        {
            /// <summary>编号</summary>
            public static readonly Field ID = FindByName(__.ID);

            /// <summary>语言文化名称</summary>
            public static readonly Field LanguageCultureName = FindByName(__.LanguageCultureName);

            /// <summary>语言展示名</summary>
            public static readonly Field DisplayName = FindByName(__.DisplayName);

            /// <summary>英语区域名</summary>
            public static readonly Field EnglishName = FindByName(__.EnglishName);

            /// <summary>区域性名称</summary>
            public static readonly Field NativeName = FindByName(__.NativeName);

            /// <summary>全球语言标准码</summary>
            public static readonly Field ISO639xValue = FindByName(__.ISO639xValue);

            /// <summary>启用</summary>
            public static readonly Field Enable = FindByName(__.Enable);

            static Field FindByName(String name) => Meta.Table.FindByName(name);
        }

        /// <summary>取得语言字段名称的快捷方式</summary>
        public partial class __
        {
            /// <summary>编号</summary>
            public const String ID = "ID";

            /// <summary>语言文化名称</summary>
            public const String LanguageCultureName = "LanguageCultureName";

            /// <summary>语言展示名</summary>
            public const String DisplayName = "DisplayName";

            /// <summary>英语区域名</summary>
            public const String EnglishName = "EnglishName";

            /// <summary>区域性名称</summary>
            public const String NativeName = "NativeName";

            /// <summary>全球语言标准码</summary>
            public const String ISO639xValue = "ISO639xValue";

            /// <summary>启用</summary>
            public const String Enable = "Enable";
        }
        #endregion
    }

    /// <summary>语言接口</summary>
    public partial interface ILanguages
    {
        #region 属性
        /// <summary>编号</summary>
        Int32 ID { get; set; }

        /// <summary>语言文化名称</summary>
        String LanguageCultureName { get; set; }

        /// <summary>语言展示名</summary>
        String DisplayName { get; set; }

        /// <summary>英语区域名</summary>
        String EnglishName { get; set; }

        /// <summary>区域性名称</summary>
        String NativeName { get; set; }

        /// <summary>全球语言标准码</summary>
        String ISO639xValue { get; set; }

        /// <summary>启用</summary>
        Boolean Enable { get; set; }
        #endregion

        #region 获取/设置 字段值
        /// <summary>获取/设置 字段值</summary>
        /// <param name="name">字段名</param>
        /// <returns></returns>
        Object this[String name] { get; set; }
        #endregion
    }
}