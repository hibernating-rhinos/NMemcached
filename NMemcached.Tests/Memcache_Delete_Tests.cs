using System;
using MbUnit.Framework;
using NMemcached.Model;
using NMemcached.Util;

namespace NMemcached.Tests
{
	[TestFixture]
	public class Memcache_Delete_Tests : CacheMixin
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
		public void When_deleting_item_in_cache_will_remove_from_cache()
		{
			Cache["foo"] = new CachedItem();

			memcache.Delete("foo", null);

			var cachedItem = (CachedItem)Cache.Get("foo");
			Assert.IsNull(cachedItem);
		}

		[Test]
		public void When_deleting_item_in_cache_will_return_deleted()
		{
			Cache["foo"] = new CachedItem();

			CacheOperationResult result = memcache.Delete("foo", null);

			Assert.AreEqual(CacheOperationResult.Deleted, result);
		}

		[Test]
		public void When_deleting_item_not_in_cache_will_return_not_found()
		{
			Cache["foo"] = new CachedItem();

			CacheOperationResult result = memcache.Delete("foo2", null);

			Assert.AreEqual(CacheOperationResult.NotFound, result);
		
		}

		[Test]
		public void When_deleting_item_in_cache_with_time_will_block_add_operations()
		{
			Cache["foo2"] = new CachedItem();
			CacheOperationResult result = memcache.Delete("foo2", SystemTime.Now().AddDays(1));
			Assert.AreEqual(CacheOperationResult.Deleted, result);
		

			var buffer = new byte[] { 1, 2, 3, 4 };
			result = memcache.Add("foo2", 1, null, buffer);
			Assert.AreEqual(CacheOperationResult.NotStored, result);
		}

		[Test]
		public void When_deleting_item_in_cache_with_time_when_item_do_not_exists_should_not_block_add_operations()
		{
			CacheOperationResult result = memcache.Delete("foo2", SystemTime.Now().AddDays(1));
			Assert.AreEqual(CacheOperationResult.NotFound, result);


			var buffer = new byte[] { 1, 2, 3, 4 };
			result = memcache.Add("foo2", 1, null, buffer);
			Assert.AreEqual(CacheOperationResult.Stored, result);
	
		}

		[Test]
		public void When_deleting_item_in_cache_with_time_will_block_replace_operations()
		{
			Cache["foo2"] = new CachedItem();
			CacheOperationResult result = memcache.Delete("foo2", SystemTime.Now().AddDays(1));
			Assert.AreEqual(CacheOperationResult.Deleted, result);


			var buffer = new byte[] { 1, 2, 3, 4 };
			result = memcache.Replace("foo2", 1, null, buffer);
			Assert.AreEqual(CacheOperationResult.NotStored, result);
		}

		[Test]
		public void When_deleting_item_in_cache_with_time_will_block_cas_operations()
		{
			Cache["foo2"] = new CachedItem();
			CacheOperationResult result = memcache.Delete("foo2", SystemTime.Now().AddDays(1));
			Assert.AreEqual(CacheOperationResult.Deleted, result);


			var buffer = new byte[] { 1, 2, 3, 4 };
			result = memcache.CompareAndSwap("foo2", 1, null, 9,buffer);
			Assert.AreEqual(CacheOperationResult.NotStored, result);
		}
	}
}