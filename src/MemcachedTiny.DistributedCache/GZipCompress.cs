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
using System.IO.Compression;

namespace MemcachedTiny.DistributedCache
{
    /// <summary>
    /// GZip压缩实现
    /// </summary>
    public class GZipCompress : ICompress
    {
        /// <summary>
        /// 启用压缩的最小大小
        /// </summary>
        protected virtual int CompressMinSize { get; }

        /// <summary>
        /// 压缩后数据需要达到的最小比例（%）
        /// </summary>
        protected virtual int CompressRate { get; }

        /// <summary>
        /// 创建实例
        /// </summary>
        public GZipCompress(int? compressMinSize, int? compressRate)
        {
            CompressMinSize = compressMinSize ?? 4 * 1024 - 16;
            CompressRate = compressRate ?? 80;
        }

        /// <inheritdoc/>
        public virtual byte[]? Compress(byte[] value)
        {
            if (value.Length < CompressMinSize)
                return null;

            using var compressedStream = new MemoryStream(value.Length + 4);
            compressedStream.Write(MBitConverter.GetByte(value.Length));

            using var gzip = new GZipStream(compressedStream, CompressionLevel.Fastest);
            gzip.Write(value);
            gzip.Flush();

            var data = compressedStream.ToArray();
            if (data.Length * 100 / CompressRate > value.Length)
                return null;

            return data;
        }

        /// <inheritdoc/>
        public virtual byte[] Decompress(Stream compressedStream)
        {
            var lengthBuffer = new byte[4];
            compressedStream.Read(lengthBuffer);
            var length = MBitConverter.ReadInt(lengthBuffer, 0);
            var buffer = new byte[length];

            using var memoryStream = new MemoryStream(buffer);
            using var gzip = new GZipStream(compressedStream, CompressionMode.Decompress);

            gzip.CopyTo(memoryStream);
            if (memoryStream.Length != length)
                throw new Exception();

            return buffer;
        }
    }
}