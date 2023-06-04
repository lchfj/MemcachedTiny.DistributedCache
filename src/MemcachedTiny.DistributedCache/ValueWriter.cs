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

using MemcachedTiny.Util;
using Microsoft.Extensions.Caching.Distributed;

namespace MemcachedTiny.DistributedCache
{
    /// <summary>
    /// 数据写入器
    /// </summary>
    public class ValueWriter : IValueWriter
    {
        /// <summary>
        /// Memcached 允许的最大大小
        /// </summary>
        protected const int MaxSize = 1020 * 1024;

        /// <inheritdoc/>
        public virtual int Flags { get; set; }
        /// <inheritdoc/>
        public virtual uint Second { get; set; }
        /// <inheritdoc/>
        public virtual byte[] Value { get; set; }

        /// <summary>
        /// 原始数据
        /// </summary>
        protected virtual byte[] OriginalValue { get; }
        /// <summary>
        /// 缓存选项
        /// </summary>
        protected virtual DistributedCacheEntryOptions? Options { get; }
        /// <summary>
        /// 压缩器
        /// </summary>
        protected virtual ICompress? Compress { get; }


        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="options">选项</param>
        /// <param name="compress">压缩器</param>
        public ValueWriter(byte[] value, DistributedCacheEntryOptions? options, ICompress? compress)
        {
            OriginalValue = value;
            Options = options;
            Compress = compress;
            Value = Array.Empty<byte>();
        }

        /// <summary>
        /// 转换缓存选项
        /// </summary>
        /// <returns></returns>
        protected virtual byte[] ChangeOptions()
        {
            var slidingArray = Array.Empty<byte>();

            var now = DateTime.UtcNow.Ticks;
            long expirationTime;
            if (Options is null)
            {
                expirationTime = DateTime.MaxValue.Ticks;
            }
            else if (Options.SlidingExpiration.HasValue)
            {
                var ticks = Options.SlidingExpiration.Value.Ticks;
                if (ticks < TimeSpan.TicksPerSecond)
                    ticks = TimeSpan.TicksPerSecond;

                var extendTime = Math.Min(ticks / 2, ConstValue.MaxExtendTime);
                if (extendTime < TimeSpan.TicksPerSecond)
                    extendTime = TimeSpan.TicksPerSecond;

                expirationTime = now + ticks + extendTime;
                slidingArray = new byte[16];
                MBitConverter.GetByte(ticks).CopyTo(slidingArray, 0);
                MBitConverter.GetByte(expirationTime).CopyTo(slidingArray, 8);
            }
            else if (Options.AbsoluteExpiration.HasValue)
            {
                expirationTime = Options.AbsoluteExpiration.Value.UtcTicks;
            }
            else if (Options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                expirationTime = now + Options.AbsoluteExpirationRelativeToNow.Value.Ticks;
            }
            else
            {
                expirationTime = DateTime.MaxValue.Ticks;
            }

            expirationTime = Math.Min(expirationTime, now + TimeSpan.TicksPerDay * 30);
            Second = Convert.ToUInt32((expirationTime - now) / TimeSpan.TicksPerSecond);

            return slidingArray;
        }

        /// <summary>
        /// 压缩数据
        /// </summary>
        /// <returns></returns>
        protected virtual byte[]? CompressValue()
        {
            return Compress?.Compress(OriginalValue);
        }

        /// <summary>
        /// 获取分片列表
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual List<byte[]> GetSubList(byte[] value)
        {
            var list = new List<byte[]>();

            var position = 0;
            while (position < value.Length)
            {
                var length = Math.Min(value.Length - position, MaxSize);
                var buffer = GC.AllocateUninitializedArray<byte>(length);

                Array.Copy(value, position, buffer, 0, buffer.Length);

                list.Add(buffer);

                position += length;
            }

            return list;
        }


        /// <inheritdoc/>
        public virtual void PrepareSet(IMemcachedClient memcachedClient)
        {
            var valueType = ValueTypeEnum.Original;

            var slidingArray = ChangeOptions();
            if (slidingArray.Length > 0)
                valueType |= ValueTypeEnum.Sliding;


            var value = CompressValue();
            if (value is null)
                value = OriginalValue;
            else
                valueType |= ValueTypeEnum.Compress;



            if (slidingArray.Length + value.Length > MaxSize)
            {
                valueType |= ValueTypeEnum.Split;

                var list = GetSubList(value);

                using var store = new MemoryStream(list.Count * 16);
                foreach (var item in list)
                {
                    var key = Guid.NewGuid();
                    store.Write(key.ToByteArray());

                    var t = memcachedClient.Set(key.ToString(), 0, Second, item);
                }

                value = store.ToArray();
            }


            Flags = (int)valueType;
            if (slidingArray.Length == 0)
            {
                Value = value;
            }
            else
            {
                Value = GC.AllocateUninitializedArray<byte>(slidingArray.Length + value.Length);
                slidingArray.CopyTo(Value, 0);
                value.CopyTo(Value, slidingArray.Length);
            }

        }

        /// <inheritdoc/>
        public virtual Task PrepareSetAsync(IMemcachedClient memcachedClient, CancellationToken token)
        {
            var valueType = ValueTypeEnum.Original;

            var slidingArray = ChangeOptions();
            if (slidingArray.Length > 0)
                valueType |= ValueTypeEnum.Sliding;


            var value = CompressValue();
            if (value is null)
                value = OriginalValue;
            else
                valueType |= ValueTypeEnum.Compress;


            var taskList = new List<Task>();
            if (slidingArray.Length + value.Length > MaxSize)
            {
                valueType |= ValueTypeEnum.Split;

                var list = GetSubList(value);

                using var store = new MemoryStream(list.Count * 16);
                foreach (var item in list)
                {
                    var key = Guid.NewGuid();
                    store.Write(key.ToByteArray());

                    taskList.Add(memcachedClient.SetAsync(key.ToString(), 0, Second, item, token));
                }

                value = store.ToArray();
            }


            Flags = (int)valueType;
            if (slidingArray.Length == 0)
            {
                Value = value;
            }
            else
            {
                Value = GC.AllocateUninitializedArray<byte>(slidingArray.Length + value.Length);
                slidingArray.CopyTo(Value, 0);
                value.CopyTo(Value, slidingArray.Length);
            }


            if (taskList.Count <= 0)
                return Task.CompletedTask;
            return Task.WhenAll(taskList.ToArray());
        }
    }
}
