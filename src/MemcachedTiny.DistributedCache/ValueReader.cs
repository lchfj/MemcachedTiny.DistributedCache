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

using MemcachedTiny.Result;
using MemcachedTiny.Util;

namespace MemcachedTiny.DistributedCache
{
    /// <summary>
    /// 数据读取器
    /// </summary>
    public class ValueReader : IValueReader
    {
        /// <summary>
        /// 时间信息
        /// </summary>
        /// <param name="Second">新的过期时间（从现在开始的秒数）</param>
        /// <param name="Time">新的过期时间（UTC-Ticks）</param>
        protected record TimeInfo(uint Second, long Time);

        /// <summary>
        /// memcached 单片最大值
        /// </summary>
        protected const int MaxSize = 1024 * 1024;

        /// <inheritdoc/>
        public virtual byte[]? Value { get; internal set; }

        /// <summary>
        /// 缓存键
        /// </summary>
        protected virtual string Key { get; }
        /// <summary>
        /// 数据结果
        /// </summary>
        protected virtual IGetResult Result { get; }
        /// <summary>
        /// 数据压缩器
        /// </summary>
        protected virtual ICompress? Compress { get; }

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="key"></param>
        /// <param name="result"></param>
        /// <param name="compress"></param>
        public ValueReader(string key, IGetResult result, ICompress? compress)
        {
            Key = key;
            Result = result;
            Compress = compress;
        }

        /// <summary>
        /// 获取新的过期时间
        /// </summary>
        /// <returns>null：当前的时间无需更新</returns>
        protected virtual TimeInfo? GetNewTime()
        {
            var now = DateTime.UtcNow.Ticks;
            var slidingTime = MBitConverter.ReadLong(Result.Value, 0);
            var expiration = MBitConverter.ReadLong(Result.Value, 8);

            var extendTime = Math.Min(slidingTime / 2, ConstValue.MaxExtendTime);
            if (expiration < now + extendTime)
                return null;

            {
                var max = TimeSpan.TicksPerDay * 30 - extendTime;
                if (slidingTime >= max)
                    slidingTime = max;
            }

            if (expiration - now - extendTime > slidingTime)
                return new TimeInfo(0, 0);

            var span = slidingTime + extendTime;
            var second = Convert.ToUInt32(span / TimeSpan.TicksPerSecond);
            if (second <= 0)
                second = 1;

            return new TimeInfo(second, now + span);
        }

        /// <summary>
        /// 获取所有的分片键
        /// </summary>
        /// <returns></returns>
        protected virtual List<Guid> GetSubKey()
        {
            var valueType = (ValueTypeEnum)Result.Flags;

            var position = 0;
            if (valueType.HasFlag(ValueTypeEnum.Sliding))
                position += 16;

            var list = new List<Guid>();
            var value = Result.Value;
            var buffer = new byte[16];
            while (position < value.Length)
            {
                Array.Copy(value, position, buffer, 0, 16);
                list.Add(new Guid(buffer));
                position += 16;
            }

            return list;

        }

        /// <summary>
        /// 获取新的主数据内容
        /// </summary>
        /// <param name="newTime">新的过期时间</param>
        /// <returns></returns>
        protected virtual byte[] GetNewMainValue(TimeInfo newTime)
        {
            var buffer = new byte[Result.Value.Length];
            Result.Value.CopyTo(buffer, 0);

            var array = MBitConverter.GetByte(newTime.Time);
            array.CopyTo(buffer, 8);

            return buffer;
        }


        /// <inheritdoc/>
        public virtual void PrepareGet(IMemcachedClient memcachedClient)
        {
            var valueType = ((ValueTypeEnum)Result.Flags);
            if (valueType == ValueTypeEnum.Original)
            {
                Value = Result.Value;
                return;
            }

            TimeInfo? newTime = null;
            if (valueType.HasFlag(ValueTypeEnum.Sliding))
            {
                newTime = GetNewTime();
                if (newTime is null)
                {
                    Value = null;
                    return;
                }
                else if (newTime.Second > 0)
                {
                    var buffer = GetNewMainValue(newTime);
                    memcachedClient.Set(Key, Result.Flags, newTime.Second, buffer);
                }
            }

            var valueStream = new MemoryStream(Result.Value.Length);
            if (valueType.HasFlag(ValueTypeEnum.Split))
            {
                var subList = GetSubKey();
                valueStream.Capacity = subList.Count * MaxSize;
                foreach (var guid in subList)
                {
                    IGetResult sub;
                    if (newTime is null || newTime.Second == 0)
                        sub = memcachedClient.Get(guid.ToString());
                    else
                        sub = memcachedClient.GetAndTouch(guid.ToString(), newTime.Second);

                    if (!sub.Success)
                    {
                        Value = null;
                        return;
                    }
                    valueStream.Write(sub.Value);
                }
            }
            else if (valueType.HasFlag(ValueTypeEnum.Sliding))
            {
                valueStream.Write(Result.Value, 16, Result.Value.Length - 16);
            }
            else
            {
                valueStream.Write(Result.Value);
            }


            valueStream.Position = 0;
            if (valueType.HasFlag(ValueTypeEnum.Compress))
                Value = Compress?.Decompress(valueStream);
            else
                Value = valueStream.ToArray();
        }

        /// <inheritdoc/>
        public virtual async Task PrepareGetAsync(IMemcachedClient memcachedClient, CancellationToken token)
        {
            var valueType = ((ValueTypeEnum)Result.Flags);
            if (valueType == ValueTypeEnum.Original)
            {
                Value = Result.Value;
                return;
            }

            TimeInfo? newTime = null;
            if (valueType.HasFlag(ValueTypeEnum.Sliding))
            {
                newTime = GetNewTime();
                if (newTime is null)
                {
                    Value = null;
                    return;
                }
                else if (newTime.Second > 0)
                {
                    var buffer = GetNewMainValue(newTime);
                    _ = memcachedClient.SetAsync(Key, Result.Flags, newTime.Second, buffer, token);
                }
            }


            var valueStream = new MemoryStream(Result.Value.Length);
            if (valueType.HasFlag(ValueTypeEnum.Split))
            {
                var subList = GetSubKey();
                var taskList = new List<Task<IGetResult>>(subList.Count);
                foreach (var guid in subList)
                {
                    if (newTime is null || newTime.Second == 0)
                        taskList.Add(memcachedClient.GetAsync(guid.ToString(), token));
                    else
                        taskList.Add(memcachedClient.GetAndTouchAsync(guid.ToString(), newTime.Second, token));
                }
                await Task.WhenAll(taskList.ToArray()).ConfigureAwait(false);


                if (taskList.Any(t => !t.IsCompletedSuccessfully || !t.Result.Success))
                {
                    Value = null;
                    return;
                }


                valueStream.Capacity = taskList.Sum(t => t.Result.Value.Length);
                foreach (var task in taskList)
                {
                    valueStream.Write(task.Result.Value);
                }
            }
            else if (valueType.HasFlag(ValueTypeEnum.Sliding))
            {
                valueStream.Write(Result.Value, 16, Result.Value.Length - 16);
            }
            else
            {
                valueStream.Write(Result.Value);
            }


            valueStream.Position = 0;
            if (valueType.HasFlag(ValueTypeEnum.Compress))
                Value = Compress?.Decompress(valueStream);
            else
                Value = valueStream.ToArray();
        }



        /// <inheritdoc/>
        public virtual void Touch(IMemcachedClient memcachedClient)
        {
            var valueType = (ValueTypeEnum)Result.Flags;
            if (!valueType.HasFlag(ValueTypeEnum.Sliding))
                return;

            var newTime = GetNewTime();
            if (newTime is null || newTime.Second == 0)
                return;

            {
                var array = GetNewMainValue(newTime);
                memcachedClient.Set(Key, Result.Flags, newTime.Second, array);
            }

            if (!valueType.HasFlag(ValueTypeEnum.Split))
                return;

            var subList = GetSubKey();
            foreach (var sub in subList)
                memcachedClient.Touch(sub.ToString(), newTime.Second);
        }

        /// <inheritdoc/>
        public virtual Task TouchAsync(IMemcachedClient memcachedClient, CancellationToken token)
        {
            var valueType = (ValueTypeEnum)Result.Flags;
            if (!valueType.HasFlag(ValueTypeEnum.Sliding))
                return Task.CompletedTask;

            var newTime = GetNewTime();
            if (newTime is null || newTime.Second == 0)
                return Task.CompletedTask;

            var taskList = new List<Task>();

            {
                var array = GetNewMainValue(newTime);
                taskList.Add(memcachedClient.SetAsync(Key, Result.Flags, newTime.Second, array, token));
            }

            if (!valueType.HasFlag(ValueTypeEnum.Split))
                return taskList[0];

            var subList = GetSubKey();
            foreach (var sub in subList)
                taskList.Add(memcachedClient.TouchAsync(sub.ToString(), newTime.Second, token));

            return Task.WhenAll(taskList.ToArray());
        }
    }
}