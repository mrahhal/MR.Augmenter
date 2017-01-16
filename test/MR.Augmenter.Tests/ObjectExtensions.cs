namespace MR.Augmenter
{
	public static class ObjectExtensions
	{
		public static T Cast<T>(this object obj) => (T)obj;

		public static T As<T>(this object obj) where T : class => obj as T;
	}
}
