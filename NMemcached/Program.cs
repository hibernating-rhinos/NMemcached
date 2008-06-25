using System;
using System.Net;
using System.Threading;

namespace NMemcached
{
	public class Program
	{
		static void Main()
		{
			Thread.CurrentThread.Join();
		}
	}
}
