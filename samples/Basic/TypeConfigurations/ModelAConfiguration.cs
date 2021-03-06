﻿using Basic.Models;
using MR.Augmenter;

namespace Basic.TypeConfigurations
{
	public class ModelAConfiguration : TypeConfiguration<ModelA>
	{
		public ModelAConfiguration()
		{
			// Always remove
			Remove(nameof(ModelA.Ex));

			// Expose if (expose means remove only if the predicate evaluates to false)
			ExposeIf(nameof(ModelA.Secret), "IsAdmin");

			// Always add
			Add("Some", (x, state) => $"/{x.Hash}/some");
			Add("IsFoo", (x, state) => state["IsFoo"]);

			// Add if the "key" in the state evaluates to a truthy
			// value (basically that it exists and it's not false)
			AddIf("Bar", "Bar", (x, state) => state["Bar"]);

			// Do some custom work. Careful, here you're not protected.
			Custom((x, d) =>
			{
				// d is a normal dictionary, do whatever you need to do.

				d["EnableThatThing"] = /* anything really */ Boxed.True;

				//d.Remove(nameof(ModelA.Id));
			});
		}
	}
}
