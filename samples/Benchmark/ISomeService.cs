using System.Threading.Tasks;

namespace Benchmark
{
	public interface ISomeService
	{
		Task<string> FindSomething();
	}
}
