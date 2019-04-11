using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Easy.Admin.Entities
{
    /*
     * 来自第三方的实体、模型，视重要程度而定，将为其创建DTO，降低复杂性，只提取需要部分
     * 方便后续操作，比如传参，序列化
     */

    /// <summary>
    /// 列信息
    /// </summary>
    public class TableColumnDto
    {
        /// <summary>备注</summary>
        public string Description
        {
            get;
            set;
        }

        /// <summary>说明</summary>
        public string DisplayName
        {
            get;
            set;
        }

        /// <summary>属性名</summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>是否允许空</summary>
        public bool IsNullable
        {
            get;
            set;
        }

        /// <summary>长度</summary>
        public int Length
        {
            get;
            set;
        }

        /// <summary>属性类型</summary>
        //[JsonIgnore]
        [IgnoreDataMember]
        public Type Type
        {
            get;
            set;
        }

        /// <summary>
        /// 数据类型
        /// </summary>
        public string TypeStr
        {
            get=>Type?.Name;
        }

        /// <summary>是否只读</summary>
        /// <remarks>放出只读属性的设置，比如在编辑页面的时候，有的字段不能修改 如修改用户时 不能修改用户名</remarks>
        public bool ReadOnly
        {
            get;
            set;
        }
    }
}