﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Options;
using AArray = System.Collections.Generic.List<object>;
using AObject = System.Collections.Generic.Dictionary<string, object>;

namespace MR.Augmenter
{
	/// <summary>
	/// Default implementation of <see cref="IAugmenter"/>.
	/// Transforms objects to <see cref="IDictionary{TKey, TValue}"/> when necessary.
	/// </summary>
	public class Augmenter : AugmenterBase
	{
		public Augmenter(
			IOptions<AugmenterConfiguration> configuration,
			IServiceProvider services)
			: base(configuration, services)
		{
		}

		protected override object AugmentCore(AugmentationContext context)
		{
			var obj = context.Object;
			var root = new AObject();

			var typeConfigurations = BuildList(context.TypeConfiguration, context.EphemeralTypeConfiguration);
			CopyAndAugmentObject(obj, typeConfigurations, root, context.State, null, null);

			return root;
		}

		private void CopyAndAugmentObject(object obj, List<TypeConfiguration> typeConfigurations, AObject root, IReadOnlyState state, NestedTypeConfiguration ntc, object parentForNested)
		{
			foreach (var typeConfiguration in typeConfigurations)
			{
				if (ntc != null)
				{
					state = CreateNestedState(parentForNested, ntc, state);
				}
				CopyObject(obj, typeConfiguration, root, state);
			}
			foreach (var typeConfiguration in typeConfigurations)
			{
				if (ntc != null)
				{
					state = CreateNestedState(parentForNested, ntc, state);
				}
				AugmentObject(obj, typeConfiguration, root, state);
			}
		}

		private void CopyObject(object obj, TypeConfiguration typeConfiguration, AObject root, IReadOnlyState state)
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
						var ntc = GetNestedTypeConfiguration(typeConfiguration, property);
						var done = false;
						if (property.TypeInfoWrapper.IsWrapper)
						{
							var wrapper = nestedObject as AugmenterWrapper;
							Debug.Assert(wrapper != null, "wrapper != null");
							nestedObject = wrapper.Object;

							if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(nestedObject.GetType().GetTypeInfo()))
							{
								// The nested object is an array. This means we should apply the
								// wrapper's configuration to all items in the array.

								var nestedList = (nestedObject as IList) != null ?
									new AArray((nestedObject as IList).Count) :
									new AArray();
								AugmentArray(obj, ntc, nestedObject, property, nestedList, state,
									BuildList(property.TypeConfiguration, wrapper.TypeConfiguration));
								root[property.PropertyInfo.Name] = nestedList;
								done = true;
							}
						}

						if (done) continue;

						var nestedDict = new AObject();
						var tcs = BuildList(property.TypeConfiguration, ntc?.TypeConfiguration);
						CopyAndAugmentObject(nestedObject, tcs, nestedDict, state, ntc, obj);
						root[property.PropertyInfo.Name] = nestedDict;
					}
					else
					{
						var ntc = GetNestedTypeConfiguration(typeConfiguration, property);
						var nestedList = new AArray();
						AugmentArray(obj, ntc, nestedObject, property, nestedList, state, BuildList(property.TypeConfiguration, ntc?.TypeConfiguration));
						root[property.PropertyInfo.Name] = nestedList;
					}
				}
			}
		}

		private void AugmentObject(object obj, TypeConfiguration typeConfiguration, AObject root, IReadOnlyState state)
		{
			foreach (var augment in typeConfiguration.Augments)
			{
				ApplyAugment(obj, root, augment, state);
			}
			foreach (var v in typeConfiguration.CustomThunks)
			{
				ApplyCustomThunk(obj, root, state, v);
			}
		}

		private void AugmentArray(
			object obj,
			NestedTypeConfiguration ntc,
			object arr,
			APropertyInfo property,
			AArray nestedList,
			IReadOnlyState state,
			List<TypeConfiguration> tcs)
		{
			var asEnumerable = arr as IEnumerable;
			Debug.Assert(asEnumerable != null, "asEnumerable != null");
			foreach (var item in asEnumerable)
			{
				var actual = item;
				if (property.TypeInfoWrapper.IsWrapper)
				{
					if (item is AugmenterWrapper wrapper)
					{
						actual = wrapper.Object;
					}
				}

				var dict = new AObject();
				CopyAndAugmentObject(actual, tcs, dict, state, ntc, obj);
				nestedList.Add(dict);
			}
		}

		private static NestedTypeConfiguration GetNestedTypeConfiguration(
			TypeConfiguration typeConfiguration, APropertyInfo property)
		{
			if (!typeConfiguration.NestedConfigurations.IsValueCreated)
			{
				return null;
			}

			if (!typeConfiguration.NestedConfigurations.Value.TryGetValue(
				property.PropertyInfo, out var ntc))
			{
				return null;
			}

			return ntc;
		}

		private static IReadOnlyState CreateNestedState(object obj, NestedTypeConfiguration ntc, IReadOnlyState state)
		{
			if (ntc?.AddState == null)
			{
				return state;
			}

			var addedState = new State();
			ntc.AddState(obj, state, addedState);
			var newState = new State(state, addedState);
			return new ReadOnlyState(newState);
		}

		private void ApplyAugment(object obj, AObject root, Augment augment, IReadOnlyState state)
		{
			switch (augment.Kind)
			{
				case AugmentKind.Add:
					ApplyAddAugment(obj, root, augment, state);
					break;

				case AugmentKind.Remove:
					ApplyRemoveAugment(obj, root, augment, state);
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void ApplyAddAugment(object obj, AObject root, Augment augment, IReadOnlyState state)
		{
			var value = augment.ValueFunc(obj, state);
			if (ShouldIgnoreAugment(value))
			{
				return;
			}

			root[augment.Name] = augment.ValueFunc(obj, state);
		}

		private void ApplyRemoveAugment(object obj, AObject root, Augment augment, IReadOnlyState state)
		{
			var value = augment.ValueFunc?.Invoke(obj, state);
			if (ShouldIgnoreAugment(value))
			{
				return;
			}

			root.Remove(augment.Name);
		}

		private void ApplyCustomThunk(object obj, AObject root, IReadOnlyState state, Action<object, AObject, IReadOnlyState> customThunk)
		{
			customThunk(obj, root, state);
		}

		private static List<TypeConfiguration> BuildList(TypeConfiguration typeConfiguration, TypeConfiguration ephemeral)
		{
			var list = new List<TypeConfiguration>();

			if (typeConfiguration != null)
			{
				list.AddRange(typeConfiguration.BaseTypeConfigurations);
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
