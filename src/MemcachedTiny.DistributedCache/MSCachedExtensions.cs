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

using MemcachedTiny.DistributedCache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 扩展注入方法
    /// </summary>
    public static class MSCachedExtensions
    {
        /// <summary>
        /// 添加实例到服务中
        /// </summary>
        /// <param name="services">服务列表</param>
        /// <param name="option">选项</param>
        public static void AddMemcacheTiny(this IServiceCollection services, MemcachedOption option)
        {
            if (option is null)
                throw new ArgumentNullException(nameof(option));
            if (option.Connect is not { Count: > 0 })
                throw new ArgumentNullException(nameof(option.Connect));

            var instance = new Memcached(option);
            services.AddSingleton<IDistributedCache>(instance);
        }

        /// <summary>
        /// 添加实例到服务中
        /// </summary>
        /// <param name="services">服务列表</param>
        public static void AddMemcacheTiny(this IServiceCollection services)
        {
            services.AddSingleton<IDistributedCache>(provider =>
            {
                var config = provider.GetService<IConfiguration>();
                var option = config.GetSection("MemcacheTiny").Get<MemcachedOption>();

                option ??= new MemcachedOption();

                if (option.Connect is not { Count: > 0 })
                    option.Connect = new List<string>() { "127.0.0.1:11211" };

                var loggerFactory = provider.GetService<ILoggerFactory>();
                if (loggerFactory is not null)
                    option.LoggerFactory = new MSLoggerFactoryAdapter(loggerFactory);

                return new Memcached(option);
            });
        }
    }
}
