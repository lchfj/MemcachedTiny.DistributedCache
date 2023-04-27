namespace MemcachedTiny.DistributedCache.Test
{
    [TestClass]
    public class ExpireTest : BasicClass
    {
        [TestMethod]
        public void AbsoluteExpiration()
        {
            var value = Guid.NewGuid();
            var key = value.ToString();

            Memcached.Set(key, value.ToByteArray(), new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds(3)
            });

            {
                var getResult = Memcached.Get(key);
                Assert.IsNotNull(getResult);

                var newG = new Guid(getResult);
                Assert.AreEqual(value, newG);
            }

            Thread.Sleep(5000);

            {
                var getResult = Memcached.Get(key);
                Assert.IsNull(getResult);
            }
        }

        [TestMethod]
        public void AbsoluteExpirationRelativeToNow()
        {
            var value = Guid.NewGuid();
            var key = value.ToString();

            Memcached.Set(key, value.ToByteArray(), new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3)
            });

            {
                var getResult = Memcached.Get(key);
                Assert.IsNotNull(getResult);

                var newG = new Guid(getResult);
                Assert.AreEqual(value, newG);
            }

            Thread.Sleep(5000);

            {
                var getResult = Memcached.Get(key);
                Assert.IsNull(getResult);
            }
        }

        [TestMethod]
        public void Sliding()
        {
            var value = Guid.NewGuid();
            var key = value.ToString();

            Memcached.Set(key, value.ToByteArray(), new DistributedCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromSeconds(5)
            });

            {
                var getResult = Memcached.Get(key);
                Assert.IsNotNull(getResult);

                var newG = new Guid(getResult);
                Assert.AreEqual(value, newG);
            }

            for (var i = 0; i < 5; i++)
            {
                Thread.Sleep(3000);


                var getResult = Memcached.Get(key);
                Assert.IsNotNull(getResult);

                var newG = new Guid(getResult);
                Assert.AreEqual(value, newG);
            }


            Thread.Sleep(7000);

            {
                var getResult = Memcached.Get(key);
                Assert.IsNull(getResult);
            }
        }

        [TestMethod]
        public void Refresh()
        {
            var value = Guid.NewGuid();
            var key = value.ToString();

            Memcached.Set(key, value.ToByteArray(), new DistributedCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromSeconds(5)
            });

            {
                var getResult = Memcached.Get(key);
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
                var getResult = Memcached.Get(key);
                Assert.IsNotNull(getResult);

                var newG = new Guid(getResult);
                Assert.AreEqual(value, newG);
            }

            Thread.Sleep(7000);

            {
                var getResult = Memcached.Get(key);
                Assert.IsNull(getResult);
            }
        }
    }
}