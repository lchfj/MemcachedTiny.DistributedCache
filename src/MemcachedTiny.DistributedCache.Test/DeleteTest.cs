namespace MemcachedTiny.DistributedCache.Test
{
    [TestClass]
    public class DeleteTest : BasicClass
    {
        [TestMethod]
        public void DeleteSync()
        {
            var value = Guid.NewGuid();
            var key = value.ToString();


            Memcached.Set(key, value.ToByteArray(), null);

            {
                var getResult = Memcached.Get(key);

                Assert.IsNotNull(getResult);

                var newG = new Guid(getResult);
                Assert.AreEqual(value, newG);
            }


            Memcached.Remove(key);

            {
                var getResult = Memcached.Get(key);
                Assert.IsNull(getResult);
            }
        }
        [TestMethod]
        public void DeleteAsync()
        {
            var value = Guid.NewGuid();
            var key = value.ToString();


            Memcached.SetAsync(key, value.ToByteArray(), null).Wait();

            {
                var getResult = Memcached.GetAsync(key).Result;

                Assert.IsNotNull(getResult);

                var newG = new Guid(getResult);
                Assert.AreEqual(value, newG);
            }

            Thread.Sleep(3000);

            Memcached.RemoveAsync(key).Wait();

            {
                var getResult = Memcached.GetAsync(key).Result;
                Assert.IsNull(getResult);
            }
        }
    }
}