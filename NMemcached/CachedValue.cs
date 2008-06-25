using System.Runtime.Serialization;

namespace NMemcached
{
	[DataContract]
	public class CachedValue
	{
		[DataMember]
		public string Key { get; set; }

		[DataMember]
		public long Flags { get; set; }

		[DataMember]
		public long Timestamp { get; set; }

		[DataMember]
		public byte[] Value { get; set; }
	}
}