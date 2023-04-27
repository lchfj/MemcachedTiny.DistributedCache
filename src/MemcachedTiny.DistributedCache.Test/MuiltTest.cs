using System.Collections.Concurrent;

namespace MemcachedTiny.DistributedCache.Test
{
    [TestClass]
    public class MuiltTest : BasicClass
    {
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