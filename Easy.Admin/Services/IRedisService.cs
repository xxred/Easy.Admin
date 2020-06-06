using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Easy.Admin.Services
{
    /// <summary>
    /// redis操作Service,对象和数组都以json形式进行存储
    /// </summary>
    public interface IRedisService
    {
        /// <summary>
        /// 存储数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Set(string key, string value);

        /// <summary> 存储数据 </summary>
        string Get(string key);

        /// <summary>
        /// 设置超期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expire">单位秒</param>
        /// <returns></returns>
        bool Expire(string key, int expire);

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="key"></param>
        void Remove(string key);

        /// <summary>
        /// 自增操作
        /// </summary>
        /// <param name="key"></param>
        /// <param name="delta">自增步长</param>
        /// <returns></returns>
        long Increment(string key, long delta);
    }
}
