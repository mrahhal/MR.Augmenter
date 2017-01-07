using System;

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
		/// <returns>A new augmented object.</returns>
		object Augment<T>(T obj, Action<TypeConfiguration<T>> configure = null);
	}
}
