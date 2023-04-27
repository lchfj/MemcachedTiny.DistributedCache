using System.Collections.Concurrent;

namespace MemcachedTiny.DistributedCache.Test
{
    [TestClass]
    public class MuiltNodeTest : BasicClass
    {
        protected override Memcached CreatClient()
        {
            return new Memcached(new MemcachedOption()
            {
                Connect = new List<string>() { "127.0.0.1:11211", "127.0.0.1:11212", "127.0.0.1:11213", "127.0.0.1:11214" }
            });
        }


        [TestMethod]
        public void TestSync()
        {
            Parallel.For(1, 16, i =>
            {
                for (var j = 0; j < 1000; j++)
                {
                    SyncOne();
                }
            });
        }


        [TestMethod]
        public void TestAsync()
        {
            var taskList = new ConcurrentBag<Task>();
            Parallel.For(1, 16, i =>
            {
                for (var j = 0; j < 1000; j++)
                {
                    taskList.Add(AsyncOne());
                }
            });

            Task.WaitAll(taskList.ToArray());
        }
    }
}