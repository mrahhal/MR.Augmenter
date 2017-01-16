using System;

namespace MR.Augmenter
{
	public abstract class TestHost : IDisposable
	{
		public virtual void Dispose()
		{
		}

		protected AugmenterConfiguration CreateCommonConfiguration(bool build = true)
		{
			var configuration = new AugmenterConfiguration();

			configuration.Configure<TestModel1>(c =>
			{
				c.ConfigureAdd("Bar", (x, _) => $"({x.Id})");
				c.ConfigureRemove(nameof(TestModel1.Some));
			});
			configuration.Configure<TestModelA>(c =>
			{
				c.ConfigureAdd("Id2", (x, _) => $"{x.Id}2");
			});
			configuration.Configure<TestModelB>(c =>
			{
				c.ConfigureRemove(nameof(TestModelB.Foo));
			});
			configuration.Configure<TestModelC>(c =>
			{
				c.ConfigureAdd("Bar2", (x, _) => $"{x.Id}-{x.Foo}");
				c.ConfigureRemove(nameof(TestModel1.Some));
			});

			if (build)
			{
				configuration.Build();
			}

			return configuration;
		}

		protected AugmenterConfiguration CreateBuiltConfiguration()
		{
			var configuration = new AugmenterConfiguration();
			configuration.Build();
			return configuration;
		}
	}
}
