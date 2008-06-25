using MbUnit.Framework;
using NMemcached.Util;

namespace NMemcached.Tests
{
	[TestFixture]
	public class Memcache_Version_Tests : CacheMixin
	{
		[Test]
		public void Will_return_assembly_version_as_memcached_version()
		{
			var memcache = new MemcacheService();
			string version = typeof(MemcacheService).Assembly.GetName().Version.ToString();
			Assert.AreEqual(version,memcache.Version());
		}
	}
}