namespace MemcachedTiny.DistributedCache.Test
{
    [TestClass]
    public class ExpireTestAsync : BasicClass
    {
        [TestMethod]
        public void AbsoluteExpiration()
        {
            var value = Guid.NewGuid();
            var key = value.ToString();

            Memcached.SetAsync(key, value.ToByteArray(), new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds(3)
            }).Wait();

            {
                var getResult = Memcached.GetAsync(key).Result;
                Assert.IsNotNull(getResult);

                var newG = new Guid(getResult);
                Assert.AreEqual(value, newG);
            }

            Thread.Sleep(5000);

            {
                var getResult = Memcached.GetAsync(key).Result;
                Assert.IsNull(getResult);
            }
        }

        [TestMethod]
        public void AbsoluteExpirationRelativeToNow()
        {
            var value = Guid.NewGuid();
            var key = value.ToString();

            Memcached.SetAsync(key, value.ToByteArray(), new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3)
            }).Wait();

            {
                var getResult = Memcached.GetAsync(key).Result;
                Assert.IsNotNull(getResult);

                var newG = new Guid(getResult);
                Assert.AreEqual(value, newG);
            }

            Thread.Sleep(5000);

            {
                var getResult = Memcached.GetAsync(key).Result;
                Assert.IsNull(getResult);
            }
        }

        [TestMethod]
        public void Sliding()
        {
            var value = Guid.NewGuid();
            var key = value.ToString();

            Memcached.SetAsync(key, value.ToByteArray(), new DistributedCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromSeconds(5)
            }).Wait();

            {
                var getResult = Memcached.GetAsync(key).Result;
                Assert.IsNotNull(getResult);

                var newG = new Guid(getResult);
                Assert.AreEqual(value, newG);
            }

            for (var i = 0; i < 5; i++)
            {
                Thread.Sleep(3000);


                var getResult = Memcached.GetAsync(key).Result;
                Assert.IsNotNull(getResult);

                var newG = new Guid(getResult);
                Assert.AreEqual(value, newG);
            }


            Thread.Sleep(7000);

            {
                var getResult = Memcached.GetAsync(key).Result;
                Assert.IsNull(getResult);
            }
        }

        [TestMethod]
        public void Refresh()
        {
            var value = Guid.NewGuid();
            var key = value.ToString();

            Memcached.SetAsync(key, value.ToByteArray(), new DistributedCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromSeconds(5)
            }).Wait();

            {
                var getResult = Memcached.GetAsync(key).Result;
                Assert.IsNotNull(getResult);

                var newG = new Guid(getResult);
                Assert.AreEqual(value, newG);
            }

            for (var i = 0; i < 3; i++)
            {
                Thread.Sleep(3000);
                Memcached.Refresh(key);
            }


            {
                var getResult = Memcached.GetAsync(key).Result;
                Assert.IsNotNull(getResult);

                var newG = new Guid(getResult);
                Assert.AreEqual(value, newG);
            }

            Thread.Sleep(7000);

            {
                var getResult = Memcached.GetAsync(key).Result;
                Assert.IsNull(getResult);
            }
        }
    }
}