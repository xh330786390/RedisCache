using MySteel.Common.Configuration;
using MySteel.Common.Helper;
namespace RedisCache
{
    public class MySteelRedisCache
    {
        private static readonly object SingleLock = new object();

        /// <summary>
        /// 核心服务
        /// </summary>
        private static RedisService instance;
        public static RedisService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (SingleLock)
                    {
                        if (instance == null)
                        {
                            instance = new RedisService(RuntimeConfiguration.Global.Server.RedisCacheAddr,
                                RuntimeConfiguration.Global.Server.EnableCache,
                                new System.TimeSpan(RuntimeConfiguration.Global.Server.CacheHour, RuntimeConfiguration.Global.Server.CacheMinute, 0),
                                  new System.TimeSpan(RuntimeConfiguration.Global.Server.CacheLongHour, 0, 0));
                        }
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// 用户在线
        /// </summary>
        private static RedisService userOnLine_Instance;
        public static RedisService UserOnLine_Instance
        {
            get
            {
                if (userOnLine_Instance == null)
                {
                    lock (SingleLock)
                    {
                        if (userOnLine_Instance == null)
                        {
                            userOnLine_Instance = new RedisService(RuntimeConfiguration.Global.Server.UserOnlineRedisCacheAddr);
                        }
                    }
                }

                return userOnLine_Instance;
            }
        }

        /// <summary>
        /// 应用项目
        /// </summary>
        private static RedisService _appInstance;
        public static RedisService AppInstance
        {
            get
            {
                if (_appInstance == null)
                {
                    lock (SingleLock)
                    {
                        if (_appInstance == null)
                        {
                            _appInstance = new RedisService(MySteel.Common.Helper.AppSettingsHelper.GetStringValue("RedisConnect"));
                        }
                    }
                }

                return _appInstance;
            }
        }
    }
}
