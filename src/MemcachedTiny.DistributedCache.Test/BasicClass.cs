namespace MemcachedTiny.DistributedCache.Test
{
    public abstract class BasicClass
    {
        public Memcached Memcached { get; }

        protected BasicClass()
        {
            Memcached = CreatClient();
        }

        protected virtual Memcached CreatClient()
        {
            return new Memcached(new MemcachedOption()
            {
                Connect = new List<string>() { "127.0.0.1:11211" }
            });
        }

        protected virtual void SyncOne()
        {
            var value = Guid.NewGuid();
            var key = value.ToString();

            Memcached.Set(key, value.ToByteArray(), null);

            var getResult = Memcached.Get(key);
            Assert.IsNotNull(getResult);

            var newG = new Guid(getResult);
            Assert.AreEqual(value, newG);
        }

        protected virtual async Task AsyncOne()
        {
            var value = Guid.NewGuid();
            var key = value.ToString();

            await Memcached.SetAsync(key, value.ToByteArray(), null).ConfigureAwait(false);

            var getResult = await Memcached.GetAsync(key).ConfigureAwait(false);
            Assert.IsNotNull(getResult);

            var newG = new Guid(getResult);
            Assert.AreEqual(value, newG);
        }
    }
}
