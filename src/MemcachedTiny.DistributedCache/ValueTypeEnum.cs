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
    /// 数据类型
    /// </summary>
    public enum ValueTypeEnum
    {
        /// <summary>
        /// 没有特殊处理的数据
        /// </summary>
        Original = 0,
        /// <summary>
        /// 这是一个滑动过期时间缓存
        /// </summary>
        Sliding = 1,
        /// <summary>
        /// 数据经过压缩
        /// </summary>
        Compress = 2,
        /// <summary>
        /// 分片保存的数据
        /// </summary>
        Split = 4
    }
}
