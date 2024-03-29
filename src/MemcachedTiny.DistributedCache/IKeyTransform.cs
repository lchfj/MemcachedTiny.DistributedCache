﻿/*
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
    /// 缓存键转换接口（要求线程安全）
    /// </summary>
    public interface IKeyTransform
    {
        /// <summary>
        /// 转换缓存键以满足Memcached要求
        /// </summary>
        /// <param name="key">原始缓存键</param>
        /// <returns>新的缓存键</returns>
        string TransformKey(string key);
    }
}