
# MemcachedTiny.DistributedCache

基于 `MemcachedTiny` 的 `IDistributedCache` 接口实现。

Implementation of `IDistributedCache` Interface Based on `MemcachedTiny`

---

Copyright (C) 2023 lchfj.cn

`MemcachedTiny.DistributedCache` is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

`MemcachedTiny.DistributedCache` is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License along with `MemcachedTiny.DistributedCache`. If not, see <https://www.gnu.org/licenses/>.

---



## 配置项
### appsettings.json （可不配置，与下方配置等效）

更多配置项参见：`MemcachedTiny.DistributedCache.MemcachedOption`

```json
{
  "MemcacheTiny": {
    "Connect": [ "127.0.0.1:11211" ]
  }
}
```

### Program.cs
```cs
public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);

        builder.Services.AddMemcacheTiny();

        var app = builder.Build();

        app.Run();
    }
}
```