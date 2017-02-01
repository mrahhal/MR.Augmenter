using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MR.Augmenter
{
	/// <summary>
	/// Represents an augmenter that augments object properties.
	/// </summary>
	public interface IAugmenter
	{
		/// <summary>
		/// Augments objects according to configuration.
		/// </summary>
		/// <typeparam name="T">The augmented object's type.</typeparam>
		/// <param name="obj">The object to augment.</param>
		/// <param name="configure">An action that adds more configuration to this augmentation.</param>
		/// <param name="addState">An action that adds state that will be used by augmentations.</param>
		/// <returns>A new augmented object.</returns>
		Task<object> AugmentAsync<T>(
			T obj,
			Action<TypeConfiguration<T>> configure = null,
			Action<IState> addState = null);

		/// <summary>
		/// Augments a list of objects according to configuration.
		/// </summary>
		/// <typeparam name="T">The augmented object's type.</typeparam>
		/// <param name="list">The list of objects to augment.</param>
		/// <param name="configure">An action that adds more configuration to this augmentation.</param>
		/// <param name="addState">An action that adds state that will be used by augmentations.</param>
		/// <returns>A new list of augmented objects.</returns>
		Task<object> AugmentAsync<T>(
			IEnumerable<T> list,
			Action<TypeConfiguration<T>> configure = null,
			Action<IState> addState = null);

		/// <summary>
		/// Augments an array of objects according to configuration.
		/// </summary>
		/// <typeparam name="T">The augmented object's type.</typeparam>
		/// <param name="list">The list of objects to augment.</param>
		/// <param name="configure">An action that adds more configuration to this augmentation.</param>
		/// <param name="addState">An action that adds state that will be used by augmentations.</param>
		/// <returns>A new list of augmented objects.</returns>
		Task<object> AugmentAsync<T>(
			T[] list,
			Action<TypeConfiguration<T>> configure = null,
			Action<IState> addState = null);
	}
}
