using Newtonsoft.Json.Linq;

namespace MR.Augmenter
{
	/// <summary>
	/// Represents an <see cref="IAugmenter"/> that uses json.
	/// </summary>
	public class JsonAugmenter : AugmenterBase
	{
		public JsonAugmenter(AugmenterConfiguration configuration)
			: base(configuration)
		{
		}

		protected override object AugmentCore(AugmentationContext context)
		{
			var obj = context.Object;
			var jobj = context.Object as JObject;
			if (jobj == null)
			{
				jobj = JObject.FromObject(context.Object);
			}

			foreach (var typeConfiguration in context.TypeConfigurations)
			{
				AugmentObject(obj, jobj, typeConfiguration);
			}

			return jobj;
		}

		private void AugmentObject(object obj, JObject jobj, TypeConfiguration typeConfiguration)
		{
			var augments = typeConfiguration.Augments;
			foreach (var augment in augments)
			{
				ApplyAugment(obj, jobj, augment);
			}

			foreach (var nested in typeConfiguration.NestedTypeConfigurations)
			{
				var nestedObject = nested.Key.GetValue(obj);
				AugmentObject(nestedObject, (JObject)jobj[nested.Key.Name], nested.Value);
			}
		}

		private void ApplyAugment(object obj, JObject jobj, Augment augment)
		{
			switch (augment.Kind)
			{
				case AugmentKind.Add:
					ApplyAddAugment(obj, jobj, augment);
					break;

				case AugmentKind.Remove:
					ApplyRemoveAugment(obj, jobj, augment);
					break;
			}
		}

		private void ApplyAddAugment(object obj, JObject jobj, Augment augment)
		{
			var value = augment.ValueFunc(obj);
			if (ShouldIgnoreAugment(value))
			{
				return;
			}

			jobj.Add(augment.Name, JToken.FromObject(augment.ValueFunc(obj)));
		}

		private void ApplyRemoveAugment(object obj, JObject jobj, Augment augment)
		{
			var value = augment.ValueFunc?.Invoke(obj);
			if (ShouldIgnoreAugment(value))
			{
				return;
			}

			jobj.Remove(augment.Name);
		}
	}
}
