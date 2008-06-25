using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using NMemcached;

namespace NMemcachedd.PerformanceTests
{
	class Program
	{
		static int readCycles;
		static int writeCycles;
		private static readonly ManualResetEvent read = new ManualResetEvent(false);
		private static readonly ManualResetEvent write = new ManualResetEvent(false);
		static void Main()
		{
			var uriString = "net.tcp://localhost:33433/";
			var server = new ServiceHost(typeof(MemcacheService),
									 new Uri(uriString));
			var binding = new NetTcpBinding(SecurityMode.None, false);
			server.AddServiceEndpoint(typeof(IMemacache), binding,
				"MemcacheService");

			server.Open();

			var clients = new List<MemcachedClient>();
			var count = 20;
			for (int i = 0; i < count; i++)
			{
				var client = new MemcachedClient(new NetTcpBinding(SecurityMode.None), uriString + "MemcacheService");
				clients.Add(client);
			}
			const int interationCount = 10000;
			const int reportCount = 10000;
			Console.WriteLine("created clients, starting to connect");
			var startNew = Stopwatch.StartNew();
			for (int i = 0; i < count; i++)
			{
				var client = clients[i];
				if (i % 2 == 0)
				{
					new Thread(()=>
					{
						while (IncrementRead(reportCount) < interationCount)
							client.Get("foo");
						read.Set();
					})
					{
						IsBackground = true
					}.Start();
				}
				else
				{
					new Thread(() =>
					{
						while (IncrementWrites(reportCount) < interationCount)
							client.Set("foo", "bar");
						write.Set();
					})
					{
						IsBackground = true
					}.Start();
				}
			}
			WaitHandle.WaitAll(new WaitHandle[] { read, write });

			startNew.Stop();
			Console.WriteLine("took " + startNew.ElapsedMilliseconds + " total " + readCycles + " reads and " + writeCycles + " writes using " + count + " connections");
		}

		private static int IncrementWrites(int reportCount)
		{
			var increment = Interlocked.Increment(ref writeCycles);
			if (increment % reportCount == 0)
				Console.WriteLine("wrote " + increment);
			return increment;
		}

		private static int IncrementRead(int reportCount)
		{
			var increment = Interlocked.Increment(ref readCycles);
			if (increment % reportCount == 0)
				Console.WriteLine("read " + increment);
			return increment;
		}
	}
}
