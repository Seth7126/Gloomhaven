namespace XUnity.Common.Utilities;

internal class WeakNullReference<T> : WeakReference<T> where T : class
{
	public static readonly WeakNullReference<T> Singleton = new WeakNullReference<T>();

	public override bool IsAlive => true;

	private WeakNullReference()
		: base((T)null)
	{
	}
}
