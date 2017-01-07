namespace MR.Augmenter
{
	public abstract class CommonTestHost : TestHost
	{
		protected AugmenterConfiguration ConfigureCommon()
		{
			var configuration = new AugmenterConfiguration();

			configuration.Configure<TestModel1>(c =>
			{
				c.ConfigureAdd("Bar", b => $"({b.Id})");
				c.ConfigureRemove(nameof(TestModel1.Some));
			});

			configuration.Configure<TestModelA>(c =>
			{
				c.ConfigureAdd("Id2", x => $"{x.Id}2");
			});
			configuration.Configure<TestModelB>(c =>
			{
				c.ConfigureRemove(nameof(TestModelB.Foo));
			});
			configuration.Configure<TestModelC>(c =>
			{
				c.ConfigureAdd("Bar2", x => $"{x.Id}-{x.Foo}");
				c.ConfigureRemove(nameof(TestModel1.Some));
			});

			return configuration;
		}
	}
}
