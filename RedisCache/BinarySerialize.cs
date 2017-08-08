using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace RedisCache
{
    /// <summary>
    /// 二进制序列化、反序列化
    /// </summary>
    internal class BinarySerialize
    {
        /// <summary>
        /// 序列化：Objec 转 byte数组
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] SerializeToBinary(object obj)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                IFormatter brFormatter = new BinaryFormatter();
                brFormatter.Serialize(memStream, obj);
                return memStream.ToArray();
            }
        }

        /// <summary>
        /// 反序列化： byte数组 转 Objec
        /// </summary>
        /// <param name="binaryData"></param>
        /// <returns></returns>
        public static object DeserializeToObject(byte[] binaryData)
        {
            using (MemoryStream memStream = new MemoryStream(binaryData))
            {
                IFormatter brFormatter = new BinaryFormatter();
                return brFormatter.Deserialize(memStream);
            }
        }
    }
}
