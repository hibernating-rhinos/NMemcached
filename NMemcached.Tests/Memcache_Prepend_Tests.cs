using System;
using System.IO;
using System.Threading;
using MbUnit.Framework;
using NMemcached.Model;
using NMemcached.Util;

namespace NMemcached.Tests
{
	[TestFixture]
	public class Memcache_Prepend_Tests : CacheMixin
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
		public void When_prepending_item_not_on_cache_will_reply_that_it_was_not_stored()
		{
			var buffer = new byte[] { 1, 2, 3, 4 };
			CacheOperationResult result = memcache.Prepend("foo", buffer);
			Assert.AreEqual(CacheOperationResult.NotStored, result);
		}


		[Test]
		public void When_prepending_item_on_cache_will_reply_with_stored()
		{
			var buffer = new byte[] { 1, 2, 3, 4 };
			memcache.Set("foo", 4, null, buffer);
			CacheOperationResult result = memcache.Prepend("foo", buffer);
			Assert.AreEqual(CacheOperationResult.Stored, result);
		}

		[Test]
		public void When_prepending_item_on_cache_will_prepend_to_data_already_on_cache()
		{
			var buffer = new byte[] { 1, 2, 3, 4 };
			memcache.Set("foo", 4, null, buffer);
			
			buffer = new byte[] { 5, 6, 7, 8 };
			memcache.Prepend("foo", buffer);

			var item = (CachedItem)Cache.Get("foo");

			CollectionAssert.AreEqual(new byte[] { 5, 6, 7, 8, 1, 2, 3, 4, }, item.Buffer);
		}

		[Test]
		public void When_prepending_item_on_cache_will_not_modify_flags()
		{
			var buffer = new byte[] { 1, 2, 3, 4 };
			memcache.Set("foo", 4, null, buffer);

			buffer = new byte[] { 5, 6, 7, 8 };
			memcache.Prepend("foo", buffer);

			var item = (CachedItem)Cache.Get("foo");

			Assert.AreEqual(4, item.Flags);
		}
	}
}