/*
 * Copyright (C) 2023 lchfj.cn
 * 
 * This file is part of MemcachedTiny.DistributedCache.
 * 
 * MemcachedTiny.DistributedCache is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * MemcachedTiny.DistributedCache is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License along with MemcachedTiny.DistributedCache. If not, see <https://www.gnu.org/licenses/>.
 */

using System.Security.Cryptography;
using System.Text;

namespace MemcachedTiny.DistributedCache
{
    /// <summary>
    /// 缓存键转换器
    /// </summary>
    public class KeyTransform : IKeyTransform
    {
        /// <inheritdoc/>
        public virtual string TransformKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (key.Any(c => c is < (char)0x21 or > (char)0x7e))
            {
                var hash = HashString(key);
                return "HASHKEY-" + hash;
            }

            if (key.Length > 250)
            {
                var hash = HashString(key);
                return string.Concat(key.AsSpan(0, 40), hash);
            }

            return key;
        }

        /// <summary>
        /// 计算键的HASH值，以此清除非ASCII不可打印字符
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        protected virtual string HashString(string key)
        {
            var array = Encoding.UTF8.GetBytes(key);
            var hash = SHA1.HashData(array);
            return Convert.ToHexString(hash);
        }
    }
}