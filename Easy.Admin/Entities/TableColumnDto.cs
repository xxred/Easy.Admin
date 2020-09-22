using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        // [JsonIgnore]
        // [IgnoreDataMember]
        public Type Type
        {
            get => null;
            set => _type = value;
        }

        private Type _type;

        /// <summary>
        /// 数据类型
        /// </summary>
        public string TypeStr => _type.Name;

        /// <summary>
        /// 是否是枚举
        /// </summary>
        /// <value></value>
        public bool IsEnum => _type != null && _type.IsEnum;

        /// <summary>
        /// 枚举值
        /// </summary>
        /// <value></value>
        public Dictionary<object, string> EnumValues
        {
            get
            {
                if (!IsEnum) return null;
                var type = _type;
                var dic = new Dictionary<object, string>();
                var values = Enum.GetValues(_type);
                foreach (var item in values)
                {
                    dic.Add(item.ToInt(), Enum.GetName(type, item));
                }

                return dic;
            }
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