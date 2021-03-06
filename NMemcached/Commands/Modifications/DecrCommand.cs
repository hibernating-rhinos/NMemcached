using System.IO;
using NMemcached.Commands.Modifications;

namespace NMemcached.Commands.Modifications
{
	public class DecrCommand : AbstractArithmeticOperation
	{

		protected override ulong ArithmeticOperation(ulong cachedValue)
		{
			if (Value > cachedValue)
				return 0;
			return cachedValue - Value;
		}
	}
}