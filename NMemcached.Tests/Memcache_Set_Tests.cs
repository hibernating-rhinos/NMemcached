using System;
using System.IO;
using System.Threading;
using MbUnit.Framework;
using NMemcached.Model;
using NMemcached.Util;

namespace NMemcached.Tests
{
	[TestFixture]
	public class Memcache_Set_Tests : CacheMixin
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
		public void When_setting_item_will_put_it_in_cache()
		{
			var buffer = new byte[] { 1, 2, 3, 4 };
			memcache.Set("foo", 5, null, buffer);

			var cachedItem = (CachedItem)Cache.Get("foo");
			Assert.IsNotNull(cachedItem);
			CollectionAssert.AreEqual(buffer, cachedItem.Buffer);
			Assert.AreEqual(5, cachedItem.Flags);
		}

		[Test]
		public void When_setting_item_will_reply_that_it_was_stored()
		{
			var buffer = new byte[] { 1, 2, 3, 4 };
			CacheOperationResult result = memcache.Set("foo", 5, null, buffer);
			Assert.AreEqual(CacheOperationResult.Stored, result);
		}

		[Test]
		public void When_setting_item_that_already_exists_will_reply_with_stored()
		{
			var buffer = new byte[] { 1, 2, 3, 4 };
			CacheOperationResult result = memcache.Set("foo", 5, null, buffer);
			Assert.AreEqual(CacheOperationResult.Stored, result);
			result = memcache.Set("foo", 5, null, buffer);
			Assert.AreEqual(CacheOperationResult.Stored, result);
		}
	}
}