using System;
using System.Text;
using MbUnit.Framework;
using NMemcached.Model;
using NMemcached.Util;

namespace NMemcached.Tests
{
	[TestFixture]
	public class Memcache_Decr_Tests : CacheMixin
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
		public void When_Decrementing_value_on_cache_below_zero_will_set_to_zero()
		{
			Cache["foo"] = new CachedItem { Buffer = Encoding.ASCII.GetBytes("12") };
			ArithmeticResult result = memcache.Decr("foo", 15);

			Assert.AreEqual(0, result.Value);
			Assert.AreEqual(CacheOperationResult.Stored, result.Result);
		}

		[Test]
		public void When_Decrementing_value_on_cache_which_is_in_valid_format_use_this_as_base()
		{
			Cache["foo"] = new CachedItem { Buffer = Encoding.ASCII.GetBytes("12") };
			var result = memcache.Decr("foo", 5);

			Assert.AreEqual(7, result.Value);
			Assert.AreEqual(CacheOperationResult.Stored, result.Result);

			result = memcache.Decr("foo", 5);

			Assert.AreEqual(2, result.Value);
			Assert.AreEqual(CacheOperationResult.Stored, result.Result);

		}
	}
}