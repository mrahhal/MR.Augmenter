using System;
using System.Collections;
using System.Collections.Generic;
using MR.Augmenter.Internal;
using Newtonsoft.Json.Linq;
using IState = System.Collections.Generic.IReadOnlyDictionary<string, object>;

namespace MR.Augmenter
{
	/// <summary>
	/// Represents an <see cref="IAugmenter"/> that uses json.
	/// </summary>
	public class JsonAugmenter : AugmenterBase
	{
		public JsonAugmenter(
			AugmenterConfiguration configuration,
			IServiceProvider services)
			: base(configuration, services)
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

			foreach (var typeConfiguration in BuildList(context.TypeConfiguration, context.EphemeralTypeConfiguration))
			{
				AugmentObject(obj, jobj, typeConfiguration, context.State);
			}

			return jobj;
		}

		private void AugmentObject(object obj, JObject jobj, TypeConfiguration typeConfiguration, IState state)
		{
			foreach (var augment in typeConfiguration.Augments)
			{
				ApplyAugment(obj, jobj, augment, state);
			}

			foreach (var nested in typeConfiguration.NestedTypeConfigurations)
			{
				var nestedObject = nested.Key.GetValue(obj);
				foreach (var item in BuildList(nested.Value.TypeConfiguration, null))
				{
					if (!nested.Value.TypeInfoWrapper.IsArray)
					{
						AugmentObject(nestedObject, (JObject)jobj[nested.Key.Name], item, state);
					}
					else
					{
						AugmentArray(nestedObject, (JArray)jobj[nested.Key.Name], item, nested.Value.TypeInfoWrapper, state);
					}
				}
			}
		}

		private void AugmentArray(object obj, JArray jArray, TypeConfiguration typeConfiguration, TypeInfoWrapper tiw, IState state)
		{
			var asEnumerable = obj as IEnumerable;
			var i = 0;
			foreach (var item in asEnumerable)
			{
				var jObj = (JObject)jArray[i];
				AugmentObject(item, jObj, typeConfiguration, state);
				i++;
			}
		}

		private List<TypeConfiguration> BuildList(TypeConfiguration typeConfiguration, TypeConfiguration ephemeral)
		{
			var list = new List<TypeConfiguration>();

			if (typeConfiguration != null)
				foreach (var tc in typeConfiguration.BaseTypeConfigurations)
				{
					list.Add(tc);
				}

			if (typeConfiguration != null)
			{
				list.Add(typeConfiguration);
			}

			if (ephemeral != null)
			{
				list.Add(ephemeral);
			}

			return list;
		}

		private void ApplyAugment(object obj, JObject jobj, Augment augment, IState state)
		{
			switch (augment.Kind)
			{
				case AugmentKind.Add:
					ApplyAddAugment(obj, jobj, augment, state);
					break;

				case AugmentKind.Remove:
					ApplyRemoveAugment(obj, jobj, augment, state);
					break;
			}
		}

		private void ApplyAddAugment(object obj, JObject jobj, Augment augment, IState state)
		{
			var value = augment.ValueFunc(obj, state);
			if (ShouldIgnoreAugment(value))
			{
				return;
			}

			jobj.Add(augment.Name, JToken.FromObject(augment.ValueFunc(obj, state)));
		}

		private void ApplyRemoveAugment(object obj, JObject jobj, Augment augment, IState state)
		{
			var value = augment.ValueFunc?.Invoke(obj, state);
			if (ShouldIgnoreAugment(value))
			{
				return;
			}

			jobj.Remove(augment.Name);
		}
	}
}
