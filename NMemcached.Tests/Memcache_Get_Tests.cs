using System;
using MbUnit.Framework;
using NMemcached.Model;
using NMemcached.Util;

namespace NMemcached.Tests
{
	[TestFixture]
	public class Memcache_Get_Tests : CacheMixin
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
		public void Calling_without_any_keys_will_return_empty_result()
		{
			CachedValue[] values = memcache.Get();
			Assert.AreEqual(0, values.Length);
		}

		[Test]
		public void Calling_with_null_keys_will_return_empty_result()
		{
			CachedValue[] values = memcache.Get(null);
			Assert.AreEqual(0, values.Length);
		}

		[Test]
		public void Calling_with_null_key_will_ignore_nulls()
		{
			Cache["foo"] = new CachedItem
			{
				ExpiresAt = SystemTime.Now().AddSeconds(60)
			};
			CachedValue[] values = memcache.Get(null,"foo");
			Assert.AreEqual(1, values.Length);
		}

		[Test]
		public void When_getting_item_that_is_not_in_cache_will_return_empty_result_set()
		{
			CachedValue[] values = memcache.Get("foo");
			Assert.AreEqual(0, values.Length);
		}

		[Test]
		public void When_getting_item_that_has_been_expired_will_return_empty_result()
		{
			Cache["foo"] = new CachedItem
			{
				Buffer = new byte[] { 1, 2, 3 },
				Flags = 2,
				Key = "foo",
				ExpiresAt = SystemTime.Now().AddDays(-1)
			};

			CachedValue[] values = memcache.Get("foo");
			Assert.AreEqual(0, values.Length);
		}

		[Test]
		public void When_getting_item_that_is_in_cache_will_return_item()
		{
			Cache["foo"] = new CachedItem
			{
				Buffer = new byte[] { 1, 2, 3 },
				Flags = 2,
				Key = "foo",
				ExpiresAt = SystemTime.Now().AddDays(1)
			};
			CachedValue[] values = memcache.Get("foo");
			Assert.AreEqual(1, values.Length);

			Assert.AreEqual("foo", values[0].Key);
			Assert.AreEqual(2, values[0].Flags);
			Assert.AreEqual(630822816000000000L, values[0].Timestamp);

			CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, values[0].Value);
		}

		[Test]
		public void When_getting_several_items_that_are_in_cache_will_return_items()
		{
			Cache["foo0"] = new CachedItem
			{
				Buffer = new byte[] { 1, 2, 3 },
				Flags = 2,
				Key = "foo0",
				ExpiresAt = SystemTime.Now().AddDays(1)
			};
			Cache["foo1"] = new CachedItem
			{
				Buffer = new byte[] { 1, 2, 3 },
				Flags = 2,
				Key = "foo1",
				ExpiresAt = SystemTime.Now().AddDays(1)
			};
			Cache["foo2"] = new CachedItem
			{
				Buffer = new byte[] { 1, 2, 3 },
				Flags = 2,
				Key = "foo2",
				ExpiresAt = SystemTime.Now().AddDays(1)
			};

			CachedValue[] values = memcache.Get("foo0", "foo1", "foo2");
			for (var i = 0; i < 3; i++)
			{
				Assert.AreEqual("foo"+i, values[i].Key);
				Assert.AreEqual(2, values[i].Flags);
				Assert.AreEqual(630822816000000000L, values[i].Timestamp);

				CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, values[i].Value);
			}
		}

		[Test]
		public void When_getting_several_items_where_some_in_cache_and_some_not_will_return_items_that_are_in_cache()
		{
			Cache["foo0"] = new CachedItem
			{
				Buffer = new byte[] {1, 2, 3},
				Flags = 2,
				Key = "foo0",
				ExpiresAt = SystemTime.Now().AddDays(1)
			};
			Cache["foo2"] = new CachedItem
			{
				Buffer = new byte[] {1, 2, 3},
				Flags = 2,
				Key = "foo2",
				ExpiresAt = SystemTime.Now().AddDays(1)
			};

			CachedValue[] values = memcache.Get("foo0", "foo1", "foo2");
			Assert.AreEqual(2, values.Length);

			Assert.AreEqual("foo0", values[0].Key);
			Assert.AreEqual(2, values[0].Flags);
			Assert.AreEqual(630822816000000000L, values[0].Timestamp);

			CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, values[0].Value);

			Assert.AreEqual("foo2", values[1].Key);
			Assert.AreEqual(2, values[1].Flags);
			Assert.AreEqual(630822816000000000L, values[1].Timestamp);

			CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, values[1].Value);
		}
	}
}