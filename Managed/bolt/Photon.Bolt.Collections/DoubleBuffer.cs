namespace Photon.Bolt.Collections;

internal struct DoubleBuffer<T> where T : struct
{
	public T Previous;

	public T Current;

	public DoubleBuffer<T> Shift(T value)
	{
		DoubleBuffer<T> result = this;
		result.Previous = Current;
		result.Current = value;
		return result;
	}

	public static DoubleBuffer<T> InitBuffer(T value)
	{
		DoubleBuffer<T> result = default(DoubleBuffer<T>);
		result.Previous = value;
		result.Current = value;
		return result;
	}
}
