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

using Microsoft.Extensions.Logging;
using TL = MemcachedTiny.Logging;

namespace MemcachedTiny.DistributedCache
{
    /// <summary>
    /// 日志工厂适配器
    /// </summary>
    public class MSLoggerFactoryAdapter : TL.ILoggerFactory
    {
        /// <summary>
        /// 真正的工厂
        /// </summary>
        protected virtual ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="loggerFactory"></param>
        public MSLoggerFactoryAdapter(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
        }

        /// <inheritdoc/>
        public virtual TL.ILogger<T> CreateLogger<T>()
        {
            var logger = LoggerFactory.CreateLogger<T>();
            return new MSLoggerAdapter<T>(logger);
        }
    }
}