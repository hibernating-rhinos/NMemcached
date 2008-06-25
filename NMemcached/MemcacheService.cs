using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web.Caching;
using NMemcached.Model;
using NMemcached.Util;

namespace NMemcached
{
	public class MemcacheService : CacheMixin, IMemacache
	{
		#region IMemacache Members

		public void FlushAll(DateTime? expiresAt)
		{
			if (expiresAt == null)
			{
				ClearCache();
				return;
			}
			foreach (DictionaryEntry de in Cache)
			{
				var cachedItem = de.Value as CachedItem;
				if (cachedItem != null)
					cachedItem.ExpiresAt = expiresAt.Value;
			}
		}

		public string Version()
		{
			return typeof(MemcacheService)
				.Assembly
				.GetName()
				.Version
				.ToString();
		}

		public ArithmeticResult Incr(string key, ulong value)
		{
			return ArithmaticOperation(key, value, (x, y) => x + y);
		}

		public ArithmeticResult Decr(string key, ulong value)
		{
			return ArithmaticOperation(key, value, (x, y) =>  y > x ? 0 : x - y );
		}

		public CachedValue[] Get(params string[] keys)
		{
			var values = new List<CachedValue>();
			keys = keys ?? new string[0];
			foreach (var key in keys)
			{
				if (key == null)
					continue;

				var item = Cache.Get(key) as CachedItem;
				if (item == null)
					continue;
				if (item.ExpiresAt < SystemTime.Now())
					continue;

				values.Add(new CachedValue
				{
					Key = item.Key,
					Flags = item.Flags,
					Timestamp = item.Timestamp,
					Value = item.Buffer

				});
			}
			return values.ToArray();
		}

		public CacheOperationResult Set(string key, long flags, DateTime? expiry, byte[] data)
		{
			ValidateKey(key);
			var expiresAt = expiry ?? SystemTime.Now().AddYears(100);
			var cachedItem = new CachedItem
			{
				Key = key,
				Buffer = data,
				Flags = flags,
				ExpiresAt = expiresAt
			};
			Cache.Insert(key, cachedItem, null, expiresAt, NoSlidingExpiration);

			return CacheOperationResult.Stored;

		}

		public CacheOperationResult Add(string key, long flags, DateTime? expiry, byte[] data)
		{
			ValidateKey(key);
			if (Cache.Get(key) != null)
				return CacheOperationResult.NotStored;
			return Set(key, flags, expiry, data);
		}

		public CacheOperationResult Append(string key, byte[] data)
		{
			ValidateKey(key);
			var cachedItem = Cache.Get(key) as CachedItem;
			if (cachedItem == null)
			{
				return CacheOperationResult.NotStored;
			}
			lock (cachedItem)
			{
				var oldBufferLength = cachedItem.Buffer.Length;
				var buffer = cachedItem.Buffer;
				Array.Resize(ref buffer, oldBufferLength + data.Length);
				Array.Copy(data, 0, buffer, oldBufferLength, data.Length);
				cachedItem.Buffer = buffer;
			}
			return CacheOperationResult.Stored;
		}

		public CacheOperationResult Prepend(string key, byte[] data)
		{
			ValidateKey(key);
			var cachedItem = Cache.Get(key) as CachedItem;
			if (cachedItem == null)
			{
				return CacheOperationResult.NotStored;
			}
			lock (cachedItem)
			{
				byte[] oldBuffer = cachedItem.Buffer;
				cachedItem.Buffer = new byte[cachedItem.Buffer.Length + data.Length];

				Array.Copy(data, 0, cachedItem.Buffer, 0, data.Length);
				Array.Copy(oldBuffer, 0, cachedItem.Buffer, data.Length, oldBuffer.Length);
			}
			return CacheOperationResult.Stored;
		}

		public CacheOperationResult Replace(string key, long flags, DateTime? expiry, byte[] data)
		{
			ValidateKey(key);
			object o = Cache.Get(key);
			if (o == null || o is BlockOperationOnItemTag)
			{
				return CacheOperationResult.NotStored;
			}

			return Set(key, flags, expiry, data);
		}

		public CacheOperationResult Delete(string key, DateTime? deleteUtil)
		{
			ValidateKey(key);
			object removedItem;
			if (deleteUtil != null)
			{
				var doNotModifyTag = new BlockOperationOnItemTag(deleteUtil.Value);
				removedItem = Cache.Get(key);
				Cache.Insert(key, doNotModifyTag, null, deleteUtil.Value,
							 NoSlidingExpiration, CacheItemPriority.High, null);
				if (removedItem == null)//it wasn't in the cache in first place
					Cache.Remove(key);
			}
			else
			{
				removedItem = Cache.Remove(key);
			}
			return removedItem != null ? 
				CacheOperationResult.Deleted : 
				CacheOperationResult.NotFound;
		}

		public CacheOperationResult CompareAndSwap(string key, long flags, DateTime? expiry, long timestamp, byte[] data)
		{
			ValidateKey(key);
			var cachedItem = Cache.Get(key);
			if (cachedItem is BlockOperationOnItemTag)
			{
				return CacheOperationResult.NotStored;
			}

			var item = cachedItem as CachedItem;
			if (item == null)
			{
				return CacheOperationResult.NotFound;
			}
			lock (item)
			{
				if (item.Timestamp != timestamp)
				{
					return CacheOperationResult.Exists;
				}
				return Set(key, flags, expiry, data);
			}
		}

		#endregion

		private ArithmeticResult ArithmaticOperation(string key, ulong newValue, Func<ulong, ulong, ulong> operation)
		{
			ValidateKey(key);
			var item = Cache.Get(key) as CachedItem;
			if (item == null)
			{
				return new ArithmeticResult { Result = CacheOperationResult.NotFound, };
			}
			ulong cachedValue = 0;
			lock (item)
			{
				try
				{
					string str = Encoding.ASCII.GetString(item.Buffer);
					cachedValue = ulong.Parse(str);
				}
				catch
				{
				}
				cachedValue = operation(cachedValue, newValue);
				item.Buffer = Encoding.ASCII.GetBytes(cachedValue.ToString());
			}
			return new ArithmeticResult { Result = CacheOperationResult.Stored, Value = cachedValue };
		}


		private static void ValidateKey(string key)
		{
			if (string.IsNullOrEmpty(key))
				throw new ArgumentException("Key cannot be null or empty");
			if (key.Length > 250)
				throw new ArgumentException("Key cannot be over 250 characters");
		}
	}
}