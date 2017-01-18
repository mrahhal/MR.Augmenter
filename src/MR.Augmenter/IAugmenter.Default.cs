using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using IState = System.Collections.Generic.IReadOnlyDictionary<string, object>;

namespace MR.Augmenter
{
	/// <summary>
	/// Default implementation of <see cref="IAugmenter"/>.
	/// Transforms objects to <see cref="IDictionary{TKey, TValue}"/> when necessary.
	/// </summary>
	public class Augmenter : AugmenterBase
	{
		public Augmenter(
			AugmenterConfiguration configuration,
			IServiceProvider services)
			: base(configuration, services)
		{
		}

		protected override object AugmentCore(AugmentationContext context)
		{
			var obj = context.Object;
			var root = new Dictionary<string, object>();

			foreach (var typeConfiguration in BuildList(context.TypeConfiguration, context.EphemeralTypeConfiguration))
			{
				AugmentObject(obj, typeConfiguration, root, context.State);
			}

			return root;
		}

		private void AugmentObject(object obj, TypeConfiguration typeConfiguration, Dictionary<string, object> root, IState state)
		{
			foreach (var property in typeConfiguration.Properties)
			{
				var nestedObject = property.GetValue(obj);
				if (property.TypeConfiguration == null)
				{
					// This should be copied verbatim.
					// REVIEW: Should we check if it's wrapped and unwrap it?

					root[property.PropertyInfo.Name] = nestedObject;
				}
				else
				{
					if (nestedObject == null)
					{
						root[property.PropertyInfo.Name] = null;
					}
					else if (!property.TypeInfoWrapper.IsArray)
					{
						var done = false;
						AugmenterWrapper wrapper = null;
						if (property.TypeInfoWrapper.IsWrapper)
						{
							wrapper = nestedObject as AugmenterWrapper;
							nestedObject = wrapper.Object;

							if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(nestedObject.GetType().GetTypeInfo()))
							{
								// The nested object is an array. This means we should apply the
								// wrapper's configuration to all items in the array.

								var nestedList = new List<object>();
								AugmentArray(nestedObject, property, nestedList, state,
									BuildList(property.TypeConfiguration, wrapper.TypeConfiguration));
								root[property.PropertyInfo.Name] = nestedList;
								done = true;
							}
						}

						if (!done)
						{
							var nestedDict = new Dictionary<string, object>();
							foreach (var item in BuildList(property.TypeConfiguration, null))
							{
								AugmentObject(nestedObject, item, nestedDict, state);
							}
							root[property.PropertyInfo.Name] = nestedDict;
						}
					}
					else
					{
						var nestedList = new List<object>();
						AugmentArray(nestedObject, property, nestedList, state, BuildList(property.TypeConfiguration, null));
						root[property.PropertyInfo.Name] = nestedList;
					}
				}
			}

			foreach (var augment in typeConfiguration.Augments)
			{
				ApplyAugment(obj, root, augment, state);
			}
		}

		private void AugmentArray(
			object obj,
			APropertyInfo property,
			List<object> nestedList,
			IState state,
			List<TypeConfiguration> typeConfigurations)
		{
			var asEnumerable = obj as IEnumerable;
			foreach (var item in asEnumerable)
			{
				object actual = item;
				if (property.TypeInfoWrapper.IsWrapper)
				{
					var wrapper = item as AugmenterWrapper;
					if (wrapper != null)
					{
						actual = wrapper.Object;
					}
				}

				var dict = new Dictionary<string, object>();
				foreach (var tc in typeConfigurations)
				{
					AugmentObject(actual, tc, dict, state);
				}
				nestedList.Add(dict);
			}
		}

		private void ApplyAugment(object obj, Dictionary<string, object> root, Augment augment, IState state)
		{
			switch (augment.Kind)
			{
				case AugmentKind.Add:
					ApplyAddAugment(obj, root, augment, state);
					break;

				case AugmentKind.Remove:
					ApplyRemoveAugment(obj, root, augment, state);
					break;
			}
		}

		private void ApplyAddAugment(object obj, Dictionary<string, object> root, Augment augment, IState state)
		{
			var value = augment.ValueFunc(obj, state);
			if (ShouldIgnoreAugment(value))
			{
				return;
			}

			root[augment.Name] = augment.ValueFunc(obj, state);
		}

		private void ApplyRemoveAugment(object obj, Dictionary<string, object> root, Augment augment, IState state)
		{
			var value = augment.ValueFunc?.Invoke(obj, state);
			if (ShouldIgnoreAugment(value))
			{
				return;
			}

			root.Remove(augment.Name);
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
	}
}
