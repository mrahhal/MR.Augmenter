using System.Threading.Tasks;

namespace Benchmark
{
	public class SomeService : ISomeService
	{
		public Task<string> FindSomething()
		{
			return Task.FromResult("something");
		}
	}
}
