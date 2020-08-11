using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using NewLife;
using NewLife.Data;
using NewLife.Log;
using NewLife.Model;
using NewLife.Reflection;
using NewLife.Threading;
using NewLife.Web;
using XCode;
using XCode.Cache;
using XCode.Configuration;
using XCode.DataAccessLayer;
using XCode.Membership;

namespace Easy.Admin.Localization.Models
{
    /// <summary>本地化记录</summary>
    [Serializable]
    [ModelCheckMode(ModelCheckModes.CheckTableWhenFirstUse)]
    public class LocalizationRecords : LocalizationRecords<LocalizationRecords> { }

    /// <summary>本地化记录</summary>
    public partial class LocalizationRecords<TEntity> : Entity<TEntity> where TEntity : LocalizationRecords<TEntity>, new()
    {
        #region 对象操作
        static LocalizationRecords()
        {
            // 用于引发基类的静态构造函数，所有层次的泛型实体类都应该有一个
            var entity = new TEntity();


            // 过滤器 UserModule、TimeModule、IPModule
            Meta.Modules.Add<TimeModule>();
        }

        /// <summary>验证数据，通过抛出异常的方式提示验证失败。</summary>
        /// <param name="isNew">是否插入</param>
        public override void Valid(Boolean isNew)
        {
            // 如果没有脏数据，则不需要进行任何处理
            if (!HasDirty) return;

            // 这里验证参数范围，建议抛出参数异常，指定参数名，前端用户界面可以捕获参数异常并聚焦到对应的参数输入框
            if (Key.IsNullOrEmpty()) throw new ArgumentNullException(nameof(Key), "关键字不能为空！");

            // 在新插入数据或者修改了指定字段时进行修正
            //if (!Dirtys[nameof(UpdateTime)]) UpdateTime = DateTime.Now;

            // 检查唯一索引
            // CheckExist(isNew, __.Key, __.ResourceKey, __.LocalizationCulture);
        }

        /// <summary>首次连接数据库时初始化数据，仅用于实体类重载，用户不应该调用该方法</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override void InitData()
        {
            // InitData一般用于当数据表没有数据时添加一些默认数据，该实体类的任何第一次数据库操作都会触发该方法，默认异步调用
            if (Meta.Session.Count > 0) return;

            if (XTrace.Debug) XTrace.WriteLine("开始初始化TEntity[本地化记录]数据……");

            var data = new[]
            {
                new[] { "User type error","Request","zh-CN","用户类型错误" },
                new[] { "Incorrect ID", "Request","zh-CN","ID不正确" },
                new[] { "No permission", "Request","zh-CN","没有权限" },
                new[] { "Wrong account or password", "Request","zh-CN","账号或密码错误" },
                new[] { "The verification code is incorrect or expired", "Request","zh-CN","验证码不正确或已过期" },
                new[] { "Invalid return URL", "Request","zh-CN","返回URL无效" },
                new[] { "Data not found", "Request","zh-CN","找不到数据" },
                new[] { "There is no form content", "Request","zh-CN","没有表单内容" },
                new[] { "The uploaded file was not found", "Request","zh-CN","没有找到上传的文件" },
                new[] { "The login type is not supported", "Request","zh-CN","不支持该登录类型" },
                new[] { "No login or login timeout", "Request","zh-CN","没有登陆或登录超时" },
                new[] { "Could not find the id claim in context.User", "Request","zh-CN","context.User中找不到存放id的声明" },
                new[] { "Password is incorrect", "Request","zh-CN","密码不正确" },
                new[] { "The user does not exist, please contact the administrator", "Request","zh-CN","用户不存在，请联系管理员" },
                new[] { "User already exists", "Request","zh-CN","用户已存在" },
                new[] { "Failed to create user", "Request","zh-CN","创建用户失败" },
                new[] { "The user was not found", "Request","zh-CN","找不到该用户" },
                new[] { "The user has been disabled", "Request","zh-CN","用户已被禁用" },
            };

            foreach (var item in data)
            {
                var entity = new TEntity();
                entity.Key = item[0];
                entity.ResourceKey = item[1];
                entity.LocalizationCulture = item[2];
                entity.Text = item[3];
                entity.Insert();
            }

            if (XTrace.Debug) XTrace.WriteLine("完成初始化TEntity[本地化记录]数据！");
        }

        ///// <summary>已重载。基类先调用Valid(true)验证数据，然后在事务保护内调用OnInsert</summary>
        ///// <returns></returns>
        //public override Int32 Insert()
        //{
        //    return base.Insert();
        //}

        ///// <summary>已重载。在事务保护范围内处理业务，位于Valid之后</summary>
        ///// <returns></returns>
        //protected override Int32 OnDelete()
        //{
        //    return base.OnDelete();
        //}
        #endregion

        #region 扩展属性
        #endregion

        #region 扩展查询
        /// <summary>根据编号查找</summary>
        /// <param name="id">编号</param>
        /// <returns>实体对象</returns>
        public static TEntity FindByID(Int32 id)
        {
            if (id <= 0) return null;

            // 实体缓存
            if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.ID == id);

            // 单对象缓存
            return Meta.SingleCache[id];

            //return Find(_.ID == id);
        }

        /// <summary>根据关键字、资源关键字、语言文化名称查找</summary>
        /// <param name="key">关键字</param>
        /// <param name="resourceKey">资源关键字</param>
        /// <param name="localizationCulture">语言文化名称</param>
        /// <returns>实体对象</returns>
        public static TEntity FindByKeyAndResourceKeyAndLocalizationCulture(String key, String resourceKey, String localizationCulture)
        {
            // 实体缓存
            if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.Key == key && e.ResourceKey == resourceKey && e.LocalizationCulture == localizationCulture);

            return Find(_.Key == key & _.ResourceKey == resourceKey & _.LocalizationCulture == localizationCulture);
        }

        /// <summary>根据资源关键字查找</summary>
        /// <param name="resourceKey">资源关键字</param>
        /// <returns>实体列表</returns>
        public static IList<TEntity> FindAllByResourceKey(String resourceKey)
        {
            // 实体缓存
            if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.ResourceKey == resourceKey);

            return FindAll(_.ResourceKey == resourceKey);
        }
        #endregion

        #region 高级查询
        ///// <summary>高级查询</summary>
        ///// <param name="key">关键字(根据此值换取相应语言文字)</param>
        ///// <param name="resourceKey">资源关键字(将资源分类，方便管理)</param>
        ///// <param name="localizationCulture">语言文化名称</param>
        ///// <param name="key">关键字</param>
        ///// <param name="page">分页参数信息。可携带统计和数据权限扩展查询等信息</param>
        ///// <returns>实体列表</returns>
        //public static IList<TEntity> Search(String key, String resourceKey, String localizationCulture, String key, PageParameter page)
        //{
        //    var exp = new WhereExpression();

        //    if (!key.IsNullOrEmpty()) exp &= _.Key == key;
        //    if (!resourceKey.IsNullOrEmpty()) exp &= _.ResourceKey == resourceKey;
        //    if (!localizationCulture.IsNullOrEmpty()) exp &= _.LocalizationCulture == localizationCulture;
        //    if (!key.IsNullOrEmpty()) exp &= _.Text.Contains(key);

        //    return FindAll(exp, page);
        //}

        // Select Count(ID) as ID,ResourceKey From LocalizationRecords Where CreateTime>'2020-01-24 00:00:00' Group By ResourceKey Order By ID Desc limit 20
        static readonly FieldCache<TEntity> _ResourceKeyCache = new FieldCache<TEntity>(__.ResourceKey)
        {
            //Where = _.CreateTime > DateTime.Today.AddDays(-30) & Expression.Empty
        };

        /// <summary>获取资源关键字列表，字段缓存10分钟，分组统计数据最多的前20种，用于魔方前台下拉选择</summary>
        /// <returns></returns>
        public static IDictionary<String, String> GetResourceKeyList() => _ResourceKeyCache.FindAllName();
        #endregion

        #region 业务操作
        #endregion
    }
}