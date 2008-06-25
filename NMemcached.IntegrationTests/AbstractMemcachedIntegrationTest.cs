using System;
using System.Net;
using System.ServiceModel;
using MbUnit.Framework;
using NMemcached.Util;

namespace NMemcached.IntegrationTests
{
	public class AbstractMemcachedIntegrationTest : CacheMixin
	{
		protected MemcachedClient client;
		private ServiceHost server;

		[SetUp]
		public void Setup()
		{
			ClearCache();
			var uriString = "net.tcp://localhost:33433/";
			server = new ServiceHost(typeof (MemcacheService),
									 new Uri(uriString));
			server.AddServiceEndpoint(typeof (IMemacache),new NetTcpBinding(),
				"MemcacheService");

			server.Open();
			client = new MemcachedClient(uriString + "MemcacheService");
		}

		[TearDown]
		public void TearDown()
		{
			server.Close();
		}
	}
}