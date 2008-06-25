using System;
using System.IO;
using MbUnit.Framework;
using NMemcached.Model;
using NMemcached.Util;

namespace NMemcached.Tests
{
	[TestFixture]
	public class Memcache_CompareAndSwap_Tests : CacheMixin
	{
		private MemcacheService memcache;

		[SetUp]
		public void Setup()
		{
			SystemTime.Now = () => new DateTime(2000, 1, 1);
			ClearCache();
			memcache = new MemcacheService();
		}

		[Test]
		public void Adding_item_that_does_not_exists_will_return_not_found()
		{
			CacheOperationResult result = 
				memcache.CompareAndSwap("foo", 1, null, 523423, new byte[] {1, 2, 13, 10});
			Assert.AreEqual(CacheOperationResult.NotFound, result);
		}

		[Test]
		public void Adding_item_that_does_exists_on_cache_but_has_different_timestamp_value_will_return_exists()
		{
			Cache["foo"] = new CachedItem {Timestamp = 4};

			CacheOperationResult result = 
				memcache.CompareAndSwap("foo", 1, null, 523423, new byte[] { 1, 2, 13, 10 });
			Assert.AreEqual(CacheOperationResult.Exists, result);
	
		}

		[Test]
		public void Adding_item_that_does_exists_on_cache_and_has_matching_timestamp_value_will_return_stored()
		{
			Cache["foo"] = new CachedItem { Timestamp = 4 };
			CacheOperationResult result = 
				memcache.CompareAndSwap("foo", 1, null, 4, new byte[] { 1, 2, 13, 10 });
			Assert.AreEqual(CacheOperationResult.Stored, result);
		}

		[Test]
		public void Adding_item_that_does_exists_on_cache_and_has_matching_timestamp_value_will_replace_value()
		{
			Cache["foo"] = new CachedItem { Buffer = new byte[] { 3, 4 }, Timestamp = 4 };

			CacheOperationResult result =
			memcache.CompareAndSwap("foo", 1, null, 4, new byte[] { 1, 2, });
			Assert.AreEqual(CacheOperationResult.Stored, result);
	
			var c = (CachedItem)Cache["foo"];
			CollectionAssert.AreEqual(new byte[]{1,2}, c.Buffer);
			Assert.IsTrue(4L != c.Timestamp);
		}
	}
}