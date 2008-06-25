using System;
using System.Collections.Generic;
using MbUnit.Framework;
using NMemcached.Model;
using NMemcached.Util;

namespace NMemcached.Tests
{
	[TestFixture]
	public class Memcache_FlushAll_Tests : CacheMixin
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
		public void Without_delay_will_clear_the_cache()
		{
			for (int i = 0; i < 50; i++)
			{
				Cache[i.ToString()] = i;
			}
			Assert.AreEqual(50, Cache.Count);

			memcache.FlushAll(null);

			Assert.AreEqual(0, Cache.Count);
		}

		[Test]
		public void With_timeout_will_set_expiry_in_cache()
		{
			var items = new List<CachedItem>();
			for (int i = 0; i < 50; i++)
			{
				var cachedItem = new CachedItem {ExpiresAt = SystemTime.Now()};
				items.Add(cachedItem);
				Cache[i.ToString()] = cachedItem;
			}
			Assert.AreEqual(50, Cache.Count);

			memcache.FlushAll(SystemTime.Now().AddSeconds(60));

			Assert.AreEqual(50, Cache.Count);
			foreach (var item in items)
			{
				Assert.AreEqual(new DateTime(2000,1,1,0,1,0), item.ExpiresAt);
			}
		}
	}
}