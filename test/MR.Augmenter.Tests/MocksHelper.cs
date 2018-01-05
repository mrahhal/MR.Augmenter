using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace MR.Augmenter
{
	public static class MocksHelper
	{
		public static T For<T>(IServiceProvider services)
			where T : class
		{
			var type = typeof(T);
			if (type.GetTypeInfo().IsAbstract)
			{
				return new Mock<T>() { CallBase = true }.Object;
			}

			var ctors = type.GetConstructors();
			var ctor = ctors
				.Where(c => c.IsPublic)
				.OrderBy(c => c.GetParameters().Length)
				.Last();
			var args = ctor.GetParameters().Select(p => services.GetRequiredService(p.ParameterType)).ToArray();
			return new Mock<T>(args) { CallBase = true }.Object;
		}

		public static FakeAugmenterBase AugmenterBase(AugmenterConfiguration configuration)
		{
			var services = new ServiceCollection();
			services.AddOptions();
			services.AddSingleton(Options.Create(configuration));
			services.AddSingleton<FakeAugmenterBase>();
			var provider = services.BuildServiceProvider();
			return For<FakeAugmenterBase>(provider);
		}

		public static Augmenter Augmenter(AugmenterConfiguration configuration)
		{
			var services = new ServiceCollection();
			services.AddOptions();
			services.AddSingleton(Options.Create(configuration));
			services.AddSingleton<Augmenter>();
			var provider = services.BuildServiceProvider();
			return For<Augmenter>(provider);
		}
	}
}
