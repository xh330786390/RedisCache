using MySteel.Common.Helper;

namespace RedisCache.Tests
{
    class Program
    {
        /// <summary>
        /// 使用库
        /// </summary>
        private static int db = int.Parse(AppSettingsHelper.GetStringValue("Use_Db"));

        static void Main(string[] args)
        {
            //写入String 值
            MySteelRedisCache.AppInstance.StringSet<string>("Str_key1", "this is first value", db);

            //写入Hash值
            MySteelRedisCache.AppInstance.HashSet("Hash_Key1", "field1", "this is first value", db);
        }
    }
}
