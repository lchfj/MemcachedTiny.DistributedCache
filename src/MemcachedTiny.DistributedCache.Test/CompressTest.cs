namespace MemcachedTiny.DistributedCache.Test
{
    [TestClass]
    public class CompressTest
    {
        public Memcached Memcached { get; }

        public CompressTest()
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

        public byte[] CreatLongValue()
        {
            var value = new byte[1024 * 1024 * 9];
            var stream = new MemoryStream(value);
            for (var i = 0; i < 1024 * 9; i++)
            {
                var data = (byte)Random.Shared.Next(byte.MaxValue);
                for (var j = 0; j < 1024; j++)
                {
                    stream.WriteByte(data);
                }
            }
            return value;
        }

        [TestMethod]
        public virtual void SyncOne()
        {
            var key = Guid.NewGuid().ToString();
            var value = CreatLongValue();

            Memcached.Set(key, value, null);

            var getResult = Memcached.Get(key);
            Assert.IsNotNull(getResult);

            Assert.IsTrue(getResult.SequenceEqual(value));
        }

        [TestMethod]
        public virtual async Task AsyncOne()
        {
            var key = Guid.NewGuid().ToString();
            var value = CreatLongValue();

            await Memcached.SetAsync(key, value, null).ConfigureAwait(false);

            var getResult = await Memcached.GetAsync(key).ConfigureAwait(false);

            Assert.IsNotNull(getResult);
            Assert.IsTrue(getResult.SequenceEqual(value));
        }
    }
}
