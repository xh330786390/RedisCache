using MySteel.Common.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;

namespace RedisCache
{
    public class RedisService
    {
        /// <summary>
        /// Redis操作对象
        /// </summary>
        private ConnectionMultiplexer redis = null;
        /// <summary>
        /// 过期时间
        /// </summary>
        private TimeSpan? expireTimeSpan = null;

        /// <summary>
        /// 长过期时间(目前用海关品种关系)
        /// </summary>
        private TimeSpan? expiryLongTime = null;

        /// <summary>
        /// 是否启动缓存
        /// </summary>
        private bool enableCache = true;

        /// <summary>
        ///Redis 连接状态
        /// </summary>
        private bool redisConnected
        {
            get
            {
                bool _redisConnected = false;
                try
                {
                    _redisConnected = redis.IsConnected;
                }
                catch { }
                return _redisConnected;
            }
        }

        /// <summary>
        /// 构造函数，创建Redis连接对象
        /// </summary>
        /// <param name="servers"></param>
        public RedisService(string servers, bool enableCache = true, TimeSpan? expiry = null, TimeSpan? expiryLongTime = null)
        {
            this.enableCache = enableCache;
            this.expireTimeSpan = expiry;
            this.expiryLongTime = expiryLongTime;

            if (!string.IsNullOrEmpty(servers))
            {
                ConfigurationOptions config = ConfigurationOptions.Parse(servers);
                config.AllowAdmin = true;
                try
                {
                    redis = ConnectionMultiplexer.Connect(config);
                }
                catch { }
            }
        }

        /// <summary>
        /// 存储String类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="blLongTime"></param>
        public void Add<T>(string key, T value, bool blLongTime = false)
        {
            if (this.enableCache && value != null)
            {
                try
                {
                    if (blLongTime)
                    {
                        redis.GetDatabase().StringSet(key, BinarySerialize.SerializeToBinary(value), expiryLongTime);
                    }
                    else
                    {
                        redis.GetDatabase().StringSet(key, BinarySerialize.SerializeToBinary(value), expireTimeSpan);
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 获取String Key对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            if (this.enableCache)
            {
                try
                {
                    var result = redis.GetDatabase().StringGet(key);
                    if (result.HasValue)
                    {
                        return (T)BinarySerialize.DeserializeToObject(result);
                    }
                }
                catch { }
            }
            return default(T);
        }

        /// <summary>
        /// 判断键是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Exist(string key)
        {
            if (this.enableCache)
            {
                try
                {
                    return redis.GetDatabase().KeyExists(key);
                }
                catch { }
            }
            return false;
        }

        /// <summary>
        /// 获取String值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(string key)
        {
            object result = null;
            if (this.enableCache)
            {
                try
                {
                    result = redis.GetDatabase().StringGet(key);
                }
                catch { }
            }
            return result;
        }

        public void Put<T>(string key, T value)
        {
            if (this.enableCache && value != null)
            {
                try
                {
                    redis.GetDatabase().StringSet(key, BinarySerialize.SerializeToBinary(value));
                }
                catch { }
            }
        }

        public void Put<T>(string key, T value, TimeSpan duration)
        {
            if (this.enableCache && value != null)
            {
                try
                {
                    redis.GetDatabase().StringSet(key, BinarySerialize.SerializeToBinary(value), duration);
                }
                catch { }
            }
        }

        public void Remove(string key)
        {
            if (this.enableCache)
            {
                try
                {
                    redis.GetDatabase().KeyDelete(key);
                }
                catch { }
            }
        }

        /// <summary>
        /// 获取KEY
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public string GetKeys(string name, Dictionary<string, object> parameter)
        {
            string key = name;
            if (parameter != null)
            {
                foreach (var ids in parameter.Keys)
                {
                    if (parameter[ids] != null)
                    {
                        key += "_" + parameter[ids].ToString();
                    }
                }
            }
            return key;
        }

        #region [String]
        /// <summary>
        /// 存储字符信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public bool StringSet<T>(string key, T value, int db = 1, TimeSpan? expiry = null)
        {
            if (this.enableCache && value != null)
            {
                try
                {
                    return redis.GetDatabase(db).StringSet(key, BinarySerialize.SerializeToBinary(value), expiry);
                }
                catch { }
            }
            return false;
        }

        /// <summary>
        /// 获取字符信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public T StringGet<T>(string key, int db = 1)
        {
            if (this.enableCache)
            {
                try
                {
                    var result = redis.GetDatabase(db).StringGet(key);
                    if (result.HasValue)
                    {
                        return (T)BinarySerialize.DeserializeToObject(result);
                    }
                }
                catch { }
            }
            return default(T);
        }
        #endregion

        /// <summary>
        /// 移除键
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="db"></param>
        public void RemoveKey(string redisKey, int db = 1)
        {
            if (this.enableCache)
            {
                try
                {
                    redis.GetDatabase(db).KeyDelete(redisKey);
                }
                catch { }
            }
        }

        #region [HastSet]
        /// <summary>
        /// 设置单个 Key-Value 值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="fieldKey"></param>
        /// <param name="value"></param>
        /// <param name="db"></param>
        public void HashSet(string redisKey, string fieldKey, string value, int db = 1)
        {
            if (this.enableCache)
            {
                try
                {
                    redis.GetDatabase(db).HashSet(redisKey, fieldKey, value);
                }
                catch { }
            }
        }

        /// <summary>
        /// 设置多个个 Key-Value 值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="fieldKeys"></param>
        /// <param name="db"></param>
        public void HashSet(string redisKey, Dictionary<string, string> fieldKeys, int db = 1)
        {
            //var entries = fieldKeys.Select(x => new HashEntry(x.Key, x.Value));
            if (this.enableCache)
            {
                try
                {
                    foreach (var item in fieldKeys)
                    {
                        HashSet(redisKey, item.Key, item.Value, db);
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 获取单个hash key值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="fieldKey"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public string HashGet(string redisKey, string fieldKey, int db = 1)
        {
            if (this.enableCache)
            {
                try
                {
                    return redis.GetDatabase(db).HashGet(redisKey, fieldKey);
                }
                catch { }
            }
            return null;
        }

        /// <summary>
        /// 获取多个 hash key 值
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="fieldKeys"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public IEnumerable<string> HashGet(string redisKey, List<string> fieldKeys, int db = 1)
        {
            if (this.enableCache)
            {
                try
                {
                    var hashFields = fieldKeys.Select(x => (RedisValue)x);

                    return ConvertStrings(redis.GetDatabase(db).HashGet(redisKey, hashFields.ToArray()));
                }
                catch { }
            }
            return null;
        }

        /// <summary>
        /// 删除Hash 单个字段
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="fieldKey"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public bool HashDeleteField(string redisKey, string fieldKey, int db = 1)
        {
            if (this.enableCache)
            {
                try
                {
                    return redis.GetDatabase(db).HashDelete(redisKey, fieldKey);
                }
                catch { }
            }
            return false;
        }

        /// <summary>
        /// 删除Hash 单个字段
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="fieldKey"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public long HashDeleteField(string redisKey, List<string> fieldKeys, int db = 1)
        {
            if (this.enableCache)
            {
                try
                {
                    var lt_value = fieldKeys.Select(l => (RedisValue)l);
                    return redis.GetDatabase(db).HashDelete(redisKey, lt_value.ToArray());
                }
                catch { }
            }
            return 0;
        }

        public bool HashExist(string redisKey, string hashField, int db = 1)
        {
            if (this.enableCache)
            {
                try
                {
                    return redis.GetDatabase(db).HashExists(redisKey, hashField);
                }
                catch { }
            }
            return false;
        }


        #endregion

        #region [Set]
        /// <summary>
        /// 添加Set集合
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="setValues"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public long AddSet(string redisKey, List<string> setValues, int db = 1)
        {
            if (this.enableCache)
            {
                try
                {
                    var lt_value = setValues.Select(l => (RedisValue)l);
                    return redis.GetDatabase(db).SetAdd(redisKey, lt_value.ToArray());
                }
                catch { }
            }
            return 0;
        }

        /// <summary>
        /// 添加单个set
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="value"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public bool AddSet(string redisKey, string value, int db = 1)
        {
            if (this.enableCache)
            {
                try
                {
                    return redis.GetDatabase(db).SetAdd(redisKey, value);
                }
                catch { }
            }
            return false;
        }

        /// <summary>
        /// 获取集合元素
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public IEnumerable<string> GetSet(string redisKey, int db = 1)
        {
            if (this.enableCache)
            {
                try
                {
                    return ConvertStrings(redis.GetDatabase(db).SetMembers(redisKey));
                }
                catch { }
            }
            return null;
        }

        /// <summary>
        /// 移除集合单个元素
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="item"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public bool SetRemove(string redisKey, string item, int db = 1)
        {
            if (this.enableCache)
            {
                try
                {
                    return redis.GetDatabase(db).SetRemove(redisKey, item);
                }
                catch { }
            }
            return false;
        }

        /// <summary>
        /// 判断集合中是否包含元素
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="item"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public bool SetContains(string redisKey, string item, int db = 1)
        {
            if (this.enableCache)
            {
                try
                {
                    return redis.GetDatabase(db).SetContains(redisKey, item);
                }
                catch { }
            }
            return false;
        }

        /// <summary>
        /// 移除集合多个元素
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="item"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public long SetRemove(string redisKey, List<string> items, int db = 1)
        {
            if (this.enableCache)
            {
                try
                {
                    var lt_value = items.Select(l => (RedisValue)l);
                    return redis.GetDatabase(db).SetRemove(redisKey, lt_value.ToArray());
                }
                catch { }
            }
            return 0;
        }
        #endregion

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        private IEnumerable<string> ConvertStrings<T>(IEnumerable<T> list) where T : struct
        {
            if (list == null) return null;
            return list.Select(x => x.ToString());
        }

        #region [Server]
        /// <summary>
        /// 清空指定库表所有Key
        /// </summary>
        /// <param name="db"></param>
        public void FlushDb(int db = 0)
        {
            var points = redis.GetEndPoints();
            IServer server = redis.GetServer(points.First());
            server.FlushDatabase(db);

        }

        /// <summary>
        /// 清空所有库表所有Key
        /// </summary>
        public void FlushAllDb()
        {
            var points = redis.GetEndPoints();
            IServer server = redis.GetServer(points.First());
            server.FlushAllDatabases();
        }

        /// <summary>
        /// 获取模糊Key
        /// </summary>
        public List<string> GetKeys(string pattern, int db = 0)
        {
            var points = redis.GetEndPoints();
            IServer server = redis.GetServer(points.First());

            return ConvertStrings(server.Keys(db, pattern)).ToList();
        }
        #endregion
    }
}
