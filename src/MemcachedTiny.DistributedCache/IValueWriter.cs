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

namespace MemcachedTiny.DistributedCache
{
    /// <summary>
    /// 值写入器
    /// </summary>
    public interface IValueWriter
    {
        /// <summary>
        /// 数据类型标识
        /// </summary>
        int Flags { get; }
        /// <summary>
        /// 缓存时间
        /// </summary>
        uint Second { get; }
        /// <summary>
        /// 要写入缓存的值
        /// </summary>
        byte[] Value { get; }

        /// <summary>
        /// 准备写入的数据
        /// </summary>
        /// <param name="memcachedClient">当前使用的客户端</param>
        void PrepareSet(IMemcachedClient memcachedClient);
        /// <summary>
        /// 准备写入的数据
        /// </summary>
        /// <param name="memcachedClient">当前使用的客户端</param>
        /// <param name="token">任务取消令牌</param>
        /// <returns></returns>
        Task PrepareSetAsync(IMemcachedClient memcachedClient, CancellationToken token);
    }
}