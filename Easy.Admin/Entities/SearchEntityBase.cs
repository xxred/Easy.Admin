using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using XCode;

namespace Easy.Admin.Entities
{
    /// <summary>
    /// 搜索关键词列表
    /// </summary>

    public class SearchList
    {
        /// <summary>
        /// 名称
        /// </summary>
        [JsonProperty]
        public String Name { get; set; }
        /// <summary>
        /// 等于值
        /// </summary>
        [JsonProperty]
        public Object Equal { get; set; }
        /// <summary>
        /// 大于值
        /// </summary>
        [JsonProperty]
        public Object More { get; set; }
        /// <summary>
        /// 小于值
        /// </summary>
        [JsonProperty]
        public Object Less { get; set; }
        /// <summary>
        /// 相似值
        /// </summary>
        [JsonProperty]
        public Object Like { get; set; }
        /// <summary>
        /// 生成表达式
        /// </summary>
        /// <returns></returns>
        public Expression Expression<T>() where T : Entity<T>, new()
        {
            var entity = new T();
            var exp = new Expression();
            if (Name.IsNullOrWhiteSpace()) return null;

            // 判断字段是否存在，包括拓展字段，不仅仅是数据库字段
            var item = Entity<T>.Meta.AllFields.Find(e => e.Name == Name);

            if (item == null) return null;


            // 是否有原始字段
            bool hasOriField = false;
            // item.Field不为空，说明是原始字段
            // item.Field和item.OriField为空，说明拓展字段也找不到（没有用Map特性，无映射属性，单纯的拓展字段）
            if (item.Field == null && !(hasOriField = item.OriField != null)) return null;

            // 映射了原始字段
            if (hasOriField)
            {

                // 只处理等于和模糊匹配
                if (More != null) return null;
                if (Less != null) return null;

                var fact = EntityFactory.CreateOperate(item.Map.Provider.EntityType);
                // 关联表的字段，相当于外键所在表的字段
                var field = fact.AllFields.Find(e => e.Name == item.Map.Provider.Key);
                // 如果该字段不是数据库字段，跳过
                if (field?.Field == null) return null;

                //处理一下查询的字段，默认是查询主键
                var select = fact.Unique;

                // 不是id结尾的字段，可能是两个表名称一样的字段
                // 如果找到对应的字段，重新赋值select，找不到则返回null
                // 比如两个表都有AreaKey

                // 如果以id结尾，在另一个表中并且找到了对应字段名，重新赋值select
                // 比如两个表都有Did

                var mapNameField = fact.AllFields.Find(e => e.Name == item.Map.Name);
                if (mapNameField != null)
                {
                    select = mapNameField;
                }
                else if (!item.Map.Name.ToLower().EndsWith("id"))
                {
                    return null;
                }

                //var sb = new SelectBuilder
                //{
                //    Column = fact.Unique,// 主键，ID
                //    Table = fact.FormatedTableName
                //};
                // sb = fact.Session.Dal.PageSplit(sb, 0, 20);
                // mysql有些版本不支持在子查询中使用limit,所以直接查找记录

                // 增加查询条数限制，避免数据量大时查询所有时搞崩数据库

                var where = "";

                if (Equal != null && Equal.ToString() != "")
                {
                    where = field.Equal(Equal);
                }

                if (Like != null && Like.ToString() != "")
                {
                    where = field.Contains(Like + "");
                }

                var list = fact.FindAll(where, null, select, 0, 20);
                var ids = list.Select(s => s[select]).ToList();

                // 不能用join，否则拼接非数字类型时拼接成 "55sas5,aas777" 不正确的格式
                //var ids = list.Join(",", j => "'" + j[select] + "'");

                // 如果查找结果为空，设置ids默认值，否则会影响查询结果
                if (ids.Count < 1)
                {
                    ids.Add("''");
                }
                exp &= item.OriField.In(ids);

            }
            else
            {
                if (Equal != null && Equal.ToString() != "") exp &= item.Equal(Equal);
                if (More != null && More.ToString() != "") exp &= item >= More;
                if (Less != null && Less.ToString() != "") exp &= item <= Less;
                if (Like != null && Like.ToString() != "") exp &= item.Contains(Like + "");
            }

            return exp;
        }
    }

    /// <summary>
    /// 实体搜索基类，生成表达式
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class Search
    {
        /// <summary>
        /// 搜索列表And
        /// </summary>
        public List<SearchList> And { get; set; }
        /// <summary>
        /// 搜索列表Or
        /// </summary>
        public List<SearchList> Or { get; set; }
        /// <summary>
        ///表达式
        /// </summary>
        public virtual Expression Expressions<T>() where T : Entity<T>, new()
        {

            var and = new Expression();
            var or = new Expression();
            if (And != null)
                foreach (var item in And)
                {
                    var exp = item.Expression<T>();
                    if (exp == null || (exp + "").IsNullOrWhiteSpace()) continue;
                    and &= exp;
                }
            if (Or != null)
                foreach (var item in Or)
                {
                    var exp = item.Expression<T>();
                    if (exp == null || (exp + "").IsNullOrWhiteSpace()) continue;
                    or |= exp;
                }
            var expTmp = and & or;
            return (expTmp + "").IsNullOrWhiteSpace() ? null : expTmp;
        }
    }
}

