using System.Runtime.Serialization;

namespace NMemcached
{
	[DataContract]
	public class ArithmeticResult
	{
		[DataMember]
		public CacheOperationResult Result { get; set; }
		
		[DataMember]
		public ulong Value { get; set; }
	}
}