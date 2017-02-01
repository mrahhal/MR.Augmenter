using Microsoft.Extensions.DependencyInjection;

namespace MR.Augmenter
{
	public interface IAugmenterBuilder
	{
		IServiceCollection Services { get; }
	}
}
