using System;
using NewLife.Caching;
using NewLife.Log;

namespace Easy.Admin.Services.Impl
{
    /// <summary> redis操作Service的实现类
    /// 用NewLife.ICache替代
    /// </summary>
    public class RedisService : IRedisService
    {
        private readonly ICache _cache;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cache"></param>
        public RedisService(ICache cache)
        {
            this._cache = cache;
        }

        public void Set(string key, string value)
        {
            XTrace.WriteLine($"设置验证码：{key}------>{value}");
            _cache.Set(key, value);
        }


        public string Get(string key)
        {
            return _cache.Get<string>(key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expire">单位秒</param>
        /// <returns></returns>
        public bool Expire(string key, int expire)
        {
            return _cache.SetExpire(key, TimeSpan.FromSeconds(expire));
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public long Increment(string key, long delta)
        {
            return _cache.Increment(key, delta);
        }
    }
}
