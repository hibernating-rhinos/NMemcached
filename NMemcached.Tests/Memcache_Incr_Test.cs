using System;
using System.Text;
using MbUnit.Framework;
using NMemcached.Model;
using NMemcached.Util;

namespace NMemcached.Tests
{
	[TestFixture]
	public class Memcache_Incr_Test : CacheMixin
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
		public void When_incrementing_value_no_on_cache_will_return_not_found()
		{
			ArithmeticResult result = memcache.Incr("foo", 5);
			Assert.AreEqual(CacheOperationResult.NotFound, result.Result);
		}

		[Test]
		public void When_incrementing_value_on_cache_which_is_in_invalid_format_assumes_it_is_zero()
		{
			Cache["foo"] = new CachedItem {Buffer = new byte[] {1}};
			ArithmeticResult result = memcache.Incr("foo", 5);
			Assert.AreEqual(CacheOperationResult.Stored, result.Result);
			Assert.AreEqual(5, result.Value);
		}

		[Test]
		public void When_incrementing_value_on_cache_which_is_in_valid_format_use_this_as_base()
		{
			Cache["foo"] = new CachedItem { Buffer = Encoding.ASCII.GetBytes("12") };
			var result = memcache.Incr("foo", 5);
			Assert.AreEqual(CacheOperationResult.Stored, result.Result);
			Assert.AreEqual(17, result.Value);

			result = memcache.Incr("foo", 5);
			Assert.AreEqual(CacheOperationResult.Stored, result.Result);
			Assert.AreEqual(22, result.Value);
		}
	}
}