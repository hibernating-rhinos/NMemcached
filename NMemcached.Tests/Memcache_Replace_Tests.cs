using System;
using MbUnit.Framework;
using NMemcached.Model;
using NMemcached.Util;

namespace NMemcached.Tests
{
	[TestFixture]
	public class Memcache_Replace_Tests : CacheMixin
	{
		private MemcacheService memcache;

		[SetUp]
		public void Setup()
		{
			SystemTime.Now = () => DateTime.Now;
			ClearCache();
			memcache = new MemcacheService();
		}

		[Test]
		public void When_replacing_item_not_on_cache_will_reply_that_it_was_not_stored()
		{
			var buffer = new byte[] { 1, 2, 3, 4 };
			CacheOperationResult result = memcache.Replace("foo", 5, null, buffer);
			Assert.AreEqual(CacheOperationResult.NotStored, result);
		}

		[Test]
		public void When_replacing_item_on_cache_will_reply_that_it_was_not_stored()
		{
			memcache.Set("foo", 5, null, new byte[] { 1, 2, 3, 4 });

			CacheOperationResult result =
				memcache.Replace("foo", 5, null, new byte[] { 12, 3, 4 });
			Assert.AreEqual(CacheOperationResult.Stored, result);
		}

		[Test]
		public void When_replacing_item_on_cache_will_change_item_on_cache()
		{
			memcache.Set("foo", 5, null, new byte[] { 1, 2, 3, 4 });
			memcache.Replace("foo", 2, null, new byte[] { 12, 3, 4 });
			var item = (CachedItem)Cache.Get("foo");

			Assert.AreEqual("foo", item.Key);
			Assert.AreEqual(2, item.Flags);
			CollectionAssert.AreEqual(new byte[] { 12, 3, 4 }, item.Buffer);
		}
	}
}