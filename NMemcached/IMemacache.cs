using System;
using System.ServiceModel;

namespace NMemcached
{
	[ServiceContract]
	public interface IMemacache
	{
		[OperationContract]
		void FlushAll(DateTime? expiresAt);

		[OperationContract]
		string Version();

		[OperationContract]
		ArithmeticResult Incr(string key, ulong value);

		[OperationContract]
		ArithmeticResult Decr(string key, ulong value);

		[OperationContract]
		CachedValue[] Get(params string[] key);

		[OperationContract]
		CacheOperationResult Set(string key, long flags, DateTime? expiry, byte[] data);

		[OperationContract]
		CacheOperationResult Add(string key, long flags, DateTime? expiry, byte[] data);

		[OperationContract]
		CacheOperationResult Append(string key, byte[] data);

		[OperationContract]
		CacheOperationResult Prepend(string key, byte[] data);

		[OperationContract]
		CacheOperationResult Replace(string key, long flags, DateTime? expiry, byte[] data);

		[OperationContract]
		CacheOperationResult Delete(string key, DateTime? deleteUtil);

		[OperationContract]
		CacheOperationResult CompareAndSwap(string key, long flags, DateTime? expiry, long timestamp, byte[] data);
	}
}