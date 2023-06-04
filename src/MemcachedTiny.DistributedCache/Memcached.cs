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

using Microsoft.Extensions.Caching.Distributed;

namespace MemcachedTiny.DistributedCache
{
    /// <summary>
    /// Memcache缓存
    /// </summary>
    public class Memcached : IDistributedCache
    {
        /// <summary>
        /// Memcached 客户端
        /// </summary>
        protected virtual IMemcachedClient MemcachedClient { get; }
        /// <summary>
        /// 缓存键转换器
        /// </summary>
        protected virtual IKeyTransform KeyTransform { get; }
        /// <summary>
        /// 数据压缩接口
        /// </summary>
        protected virtual ICompress? Compress { get; }

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="option">选项</param>
        public Memcached(MemcachedOption option)
        {
            MemcachedClient = CreatClient(option);
            KeyTransform = CreatKeyTransform();
            Compress = CreatCompress(option);
        }

        /// <summary>
        /// 创建客户端
        /// </summary>
        protected virtual IMemcachedClient CreatClient(IMemcachedClientSetting option)
        {
            return new MemcachedClient(option);
        }

        /// <summary>
        /// 创建缓存键转换器
        /// </summary>
        protected virtual IKeyTransform CreatKeyTransform()
        {
            return new KeyTransform();
        }

        /// <summary>
        /// 创建数据压缩方法
        /// </summary>
        protected virtual ICompress? CreatCompress(MemcachedOption option)
        {
            if (option.CompressDisabled)
                return null;

            return new GZipCompress(option.CompressMinSize, option.CompressRate);
        }

        /// <summary>
        /// 创建值读取器
        /// </summary>
        protected virtual IValueReader CreatValueReader(string key, Result.IGetResult result)
        {
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            return new ValueReader(key, result, Compress);
        }

        /// <summary>
        /// 创建写入器
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="option">值的缓存参数</param>
        /// <returns></returns>
        protected virtual IValueWriter CreatValueWriter(byte[] value, DistributedCacheEntryOptions? option)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            return new ValueWriter(value, option, Compress);
        }

        /// <inheritdoc/>
        public virtual byte[]? Get(string key)
        {
            key = KeyTransform.TransformKey(key);

            var result = MemcachedClient.Get(key);
            if (!result.Success)
                return null;

            var reader = CreatValueReader(key, result);
            reader.PrepareGet(MemcachedClient);

            return reader.Value;
        }

        /// <inheritdoc/>
        public virtual async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            key = KeyTransform.TransformKey(key);

            var result = await MemcachedClient.GetAsync(key, token).ConfigureAwait(false);
            if (!result.Success)
                return null;

            var reader = CreatValueReader(key, result);
            await reader.PrepareGetAsync(MemcachedClient, token).ConfigureAwait(false);

            return reader.Value;
        }

        /// <inheritdoc/>
        public virtual void Refresh(string key)
        {
            key = KeyTransform.TransformKey(key);

            var result = MemcachedClient.Get(key);
            if (!result.Success)
                return;

            var reader = CreatValueReader(key, result);
            reader.Touch(MemcachedClient);
        }

        /// <inheritdoc/>
        public virtual async Task RefreshAsync(string key, CancellationToken token = default)
        {
            key = KeyTransform.TransformKey(key);

            var result = await MemcachedClient.GetAsync(key, token).ConfigureAwait(false);
            if (!result.Success)
                return;

            var reader = CreatValueReader(key, result);
            await reader.TouchAsync(MemcachedClient, token).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual void Remove(string key)
        {
            key = KeyTransform.TransformKey(key);

            MemcachedClient.Delete(key);
        }

        /// <inheritdoc/>
        public virtual Task RemoveAsync(string key, CancellationToken token = default)
        {
            key = KeyTransform.TransformKey(key);

            return MemcachedClient.DeleteAsync(key, token);
        }

        /// <inheritdoc/>
        public virtual void Set(string key, byte[] value, DistributedCacheEntryOptions? options)
        {
            key = KeyTransform.TransformKey(key);

            var writer = CreatValueWriter(value, options);
            writer.PrepareSet(MemcachedClient);
            MemcachedClient.Set(key, writer.Flags, writer.Second, writer.Value);
        }

        /// <inheritdoc/>
        public virtual async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions? options, CancellationToken token = default)
        {
            key = KeyTransform.TransformKey(key);

            var writer = CreatValueWriter(value, options);
            await writer.PrepareSetAsync(MemcachedClient, token).ConfigureAwait(false);
            await MemcachedClient.SetAsync(key, writer.Flags, writer.Second, writer.Value, token).ConfigureAwait(false);
        }
    }
}
