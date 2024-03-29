﻿namespace MemcachedTiny.DistributedCache.Test
{
    [TestClass]
    public class LongData
    {
        public Memcached Memcached { get; }

        public LongData()
        {
            Memcached = CreatClient();
        }

        public virtual Memcached CreatClient()
        {
            return new Memcached(new MemcachedOption()
            {
                Connect = new List<string>() { "127.0.0.1:11211" }
            });
        }

        [TestMethod]
        public virtual void SyncOne()
        {
            var key = Guid.NewGuid().ToString();
            var value = new byte[1024 * 1024 * 9 / 2];
            Random.Shared.NextBytes(value);

            Memcached.Set(key, value, null);

            var getResult = Memcached.Get(key);
            Assert.IsNotNull(getResult);

            Assert.IsTrue(getResult.SequenceEqual(value));
        }

        [TestMethod]
        public virtual async Task AsyncOne()
        {
            var key = Guid.NewGuid().ToString();
            var value = new byte[1024 * 1024 * 9 / 2];
            Random.Shared.NextBytes(value);

            await Memcached.SetAsync(key, value, null).ConfigureAwait(false);

            var getResult = await Memcached.GetAsync(key).ConfigureAwait(false);

            Assert.IsNotNull(getResult);
            Assert.IsTrue(getResult.SequenceEqual(value));
        }
    }
}
