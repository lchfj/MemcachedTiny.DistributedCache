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
    /// 缓存选项
    /// </summary>
    public class MemcachedOption : MemcachedClientSetting
    {
        /// <summary>
        /// 禁用压缩
        /// </summary>
        public bool CompressDisabled { get; set; }
        /// <summary>
        /// 启用压缩的最小大小
        /// </summary>
        /// <remarks>原始数据大小大于本设定值才会启用压缩</remarks>
        public int? CompressMinSize { get; set; }
        /// <summary>
        /// 压缩后数据需要达到的最小比例（%）
        /// </summary>
        /// <remarks>压缩后数据大小 / 压缩前数据大小 小于 本设定值 才会启用压缩支持 </remarks>
        public int? CompressRate { get; set; }
    }
}
