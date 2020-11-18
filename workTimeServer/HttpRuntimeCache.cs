using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace workTimeServer
{
    public class HttpRuntimeCache : ICacheService
    {
        public void Add<V>(string key, V value)
        {
            HttpRuntimeCacheHelper<V>.GetInstance().Add(key, value);
        }

        public void Add<V>(string key, V value, int cacheDurationInSeconds)
        {
            HttpRuntimeCacheHelper<V>.GetInstance().Add(key, value, cacheDurationInSeconds);
        }

        public bool ContainsKey<V>(string key)
        {
            return HttpRuntimeCacheHelper<V>.GetInstance().ContainsKey(key);
        }

        public V Get<V>(string key)
        {
            return HttpRuntimeCacheHelper<V>.GetInstance().Get(key);
        }

        public IEnumerable<string> GetAllKey<V>()
        {
            return HttpRuntimeCacheHelper<V>.GetInstance().GetAllKey();
        }

        public V GetOrCreate<V>(string cacheKey, Func<V> create, int cacheDurationInSeconds = int.MaxValue)
        {
            var cacheManager = HttpRuntimeCacheHelper<V>.GetInstance();
            if (cacheManager.ContainsKey(cacheKey))
            {
                return cacheManager[cacheKey];
            }
            else
            {
                var result = create();
                cacheManager.Add(cacheKey, result, cacheDurationInSeconds);
                return result;
            }
        }

        public void Remove<V>(string key)
        {
            HttpRuntimeCacheHelper<V>.GetInstance().Remove(key);
        }
    }

    class HttpRuntimeCacheHelper<V>
    {

        #region 全局变量
        private static HttpRuntimeCacheHelper<V> _instance = null;
        private static readonly object _instanceLock = new object();
        private static HashSet<string> CacheList = new HashSet<string>();
        private IMemoryCache _cache;

        public object Current => throw new NotImplementedException();
        #endregion

        #region 构造函数

        private HttpRuntimeCacheHelper()
        {
            _cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        }
        #endregion

        #region  属性
        /// <summary>         
        ///根据key获取value     
        /// </summary>         
        /// <value></value>      
        public V this[string key] => (V)_cache.Get(CreateKey(key));
        #endregion

        #region 公共函数

        /// <summary>         
        /// key是否存在       
        /// </summary>         
        /// <param name="key">key</param>         
        /// <returns> ///  存在<c>true</c> 不存在<c>false</c>.        /// /// </returns>         
        public bool ContainsKey(string key)
        {
            return _cache.TryGetValue(CreateKey(key), out _);
        }

        /// <summary>         
        /// 获取缓存值         
        /// </summary>         
        /// <param name="key">key</param>         
        /// <returns></returns>         
        public V Get(string key)
        {
            return (V)_cache.Get(CreateKey(key));
        }

        /// <summary>         
        /// 获取实例 （单例模式）       
        /// </summary>         
        /// <returns></returns>         
        public static HttpRuntimeCacheHelper<V> GetInstance()
        {
            if (_instance == null)
                lock (_instanceLock)
                    if (_instance == null)
                        _instance = new HttpRuntimeCacheHelper<V>();
            return _instance;
        }

        /// <summary>         
        /// 插入缓存(默认20分钟)        
        /// </summary>         
        /// <param name="key"> key</param>         
        /// <param name="value">value</param>          
        public void Add(string key, V value)
        {
            Add(key, value, 60 * 20);
        }

        /// <summary>         
        /// 插入缓存        
        /// </summary>         
        /// <param name="key"> key</param>         
        /// <param name="value">value</param>         
        /// <param name="cacheDurationInSeconds">过期时间单位秒</param>         
        public void Add(string key, V value, int cacheDurationInSeconds)
        {
            MemoryCacheEntryOptions options = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds(cacheDurationInSeconds),
                SlidingExpiration = TimeSpan.FromSeconds(cacheDurationInSeconds)
            };
            _cache.Set(key, value, options);
            try
            {
                CacheList.Add(key);
            }
            catch (Exception)
            {
            }

        }

        /// <summary>         
        /// 插入缓存.         
        /// </summary>         
        /// <param name="key">key</param>         
        /// <param name="value">value</param>         
        /// <param name="cacheDurationInSeconds">过期时间单位秒</param>         
        /// <param name="priority">缓存项属性</param>         
        public void Add(string key, V value, MemoryCacheEntryOptions options)
        {
            string keyString = CreateKey(key);
            _cache.Set(keyString, value, options);
            try
            {
                CacheList.Add(key);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>         
        /// 删除缓存         
        /// </summary>         
        /// <param name="key">key</param>         
        public void Remove(string key)
        {
            _cache.Remove(CreateKey(key));
            try
            {
                CacheList.Remove(key);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public void RemoveAll()
        {
            try
            {
                foreach (string key in CacheList)
                {
                    _cache.Remove(key);
                }
            }
            catch (Exception)
            {
            }

        }

        /// <summary>
        /// 清除所有包含关键字的缓存
        /// </summary>
        /// <param name="removeKey">关键字</param>
        public void RemoveAll(Func<string, bool> removeExpression)
        {
            var allKeyList = GetAllKey();
            var delKeyList = allKeyList.Where(removeExpression).ToList();
            foreach (var key in delKeyList)
            {
                _cache.Remove(key); ;
            }
        }

        /// <summary>
        /// 获取所有缓存key
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllKey()
        {
            var CacheEnum = CacheList.GetEnumerator();
            while (CacheEnum.MoveNext())
            {
                yield return CacheEnum.Current;
            }
        }
        #endregion

        #region 私有函数

        /// <summary>         
        ///创建KEY   
        /// </summary>         
        /// <param name="key">Key</param>         
        /// <returns></returns>         
        private string CreateKey(string key)
        {
            return key;
        }

        public bool MoveNext()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
