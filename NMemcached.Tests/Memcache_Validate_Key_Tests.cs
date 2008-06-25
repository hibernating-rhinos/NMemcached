using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using MbUnit.Framework;
using NMemcached.Util;

namespace NMemcached.Tests
{
	[TestFixture]
	public class Memcache_Validate_Key_Tests : CacheMixin
	{
		private IList<Expression<Action<string>>> actions;
		private MemcacheService memcache;

		[SetUp]
		public void Setup()
		{
			SystemTime.Now = () => new DateTime(2000, 1, 1);
			memcache = new MemcacheService();
			actions = new List<Expression<Action<string>>>
			{
				key => memcache.Add(key, 1, null, new byte[0]),
				key => memcache.Append(key, new byte[0]),
				key => memcache.CompareAndSwap(key, 1, null, 4, new byte[0]),
				key => memcache.Decr(key, 1),
				key => memcache.Delete(key, null),
				key => memcache.Incr(key, 1),
				key => memcache.Prepend(key, new byte[0]),
				key => memcache.Replace(key, 1, null, new byte[0]),
				key => memcache.Set(key, 1, null, new byte[0]),
			};
		}

		[Test]
		public void Will_error_if_key_is_empty()
		{
			foreach (var action in actions)
			{
				try
				{
					Action<string> compile = action.Compile();
					compile("");
					Assert.Fail("Expected " + action + "to get an argument exception");
				}
				catch (ArgumentException e)
				{
					Assert.AreEqual("Key cannot be null or empty", e.Message);
				}
			}
		}

		[Test]
		public void Will_error_if_key_is_null()
		{
			foreach (var action in actions)
			{
				try
				{
					Action<string> compile = action.Compile();
					compile(null);
					Assert.Fail("Expected " + action + "to get an argument exception");
				}
				catch (ArgumentException e)
				{
					Assert.AreEqual("Key cannot be null or empty", e.Message);
				}
			}
		}

		[Test]
		public void Will_error_if_key_is_longer_than_250_charaters()
		{
			foreach (var action in actions)
			{
				try
				{
					Action<string> compile = action.Compile();
					compile(new string('1', 255));
					Assert.Fail("Expected " + action + "to get an argument exception");
				}
				catch (ArgumentException e)
				{
					Assert.AreEqual("Key cannot be over 250 characters", e.Message);
				}
			}
		}
	}
}