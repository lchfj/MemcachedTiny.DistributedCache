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
    /// 日志适配器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MSLoggerAdapter<T> : TL.ILogger<T>
    {
        /// <summary>
        /// 真正的日志实例
        /// </summary>
        protected virtual ILogger<T> Logger { get; }

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="logger"></param>
        public MSLoggerAdapter(ILogger<T> logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// 日志级别对应
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        protected virtual LogLevel ChangeLeve(TL.LogLevel level)
        {
            return level switch
            {
                TL.LogLevel.Trace => LogLevel.Trace,
                TL.LogLevel.Debug => LogLevel.Debug,
                TL.LogLevel.Information => LogLevel.Information,
                TL.LogLevel.Error => LogLevel.Error,
                _ => LogLevel.Trace,
            };
        }

        /// <inheritdoc/>
        public virtual bool IsEnabled(TL.LogLevel level)
        {
            return Logger.IsEnabled(ChangeLeve(level));
        }

        /// <inheritdoc/>
        public virtual void Log(TL.LogLevel level, Exception exception, string message, params object[] args)
        {
            Logger.Log(ChangeLeve(level), exception, message, args);
        }
    }
}