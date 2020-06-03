using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using XCode;
using XCode.Configuration;
using XCode.DataAccessLayer;

namespace Easy.Admin.Areas.Admin.Models
{
    /// <summary></summary>
    [Serializable]
    [DataObject]
    [BindTable("User", Description = "", ConnName = "Membership", DbType = DatabaseType.None)]
    public partial class ApplicationUser<TEntity> : IApplicationUser
    {
        #region 属性
        private DateTime _Birthday;
        /// <summary>生日</summary>
        [DisplayName("生日")]
        [Description("生日")]
        [DataObjectField(false, false, true, 0)]
        [BindColumn("Birthday", "生日", "")]
        public DateTime Birthday { get => _Birthday; set { if (OnPropertyChanging(__.Birthday, value)) { _Birthday = value; OnPropertyChanged(__.Birthday); } } }
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
                    case __.Birthday: return _Birthday;
                    default: return base[name];
                }
            }
            set
            {
                switch (name)
                {
                    case __.Birthday: _Birthday = value.ToDateTime(); break;
                    default: base[name] = value; break;
                }
            }
        }
        #endregion

        #region 字段名
        /// <summary>取得ApplicationUser字段信息的快捷方式</summary>
        public partial class _
        {
            /// <summary>生日</summary>
            public static readonly Field Birthday = FindByName(__.Birthday);

            static Field FindByName(String name) => Meta.Table.FindByName(name);
        }

        /// <summary>取得ApplicationUser字段名称的快捷方式</summary>
        public partial class __
        {
            /// <summary>生日</summary>
            public const String Birthday = "Birthday";
        }
        #endregion
    }

    /// <summary>接口</summary>
    public partial interface IApplicationUser
    {
        #region 属性
        /// <summary>生日</summary>
        DateTime Birthday { get; set; }
        #endregion

        #region 获取/设置 字段值
        /// <summary>获取/设置 字段值</summary>
        /// <param name="name">字段名</param>
        /// <returns></returns>
        Object this[String name] { get; set; }
        #endregion
    }
}