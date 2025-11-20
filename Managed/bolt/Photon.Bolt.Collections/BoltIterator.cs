namespace Photon.Bolt.Collections;

[Documentation(Ignore = true)]
public struct BoltIterator<T> where T : class, IBoltListNode<T>
{
	private T _node;

	private int _count;

	private int _number;

	public T val;

	public BoltIterator(T node, int count)
	{
		_node = node;
		_count = count;
		_number = 0;
		val = null;
	}

	public bool Next()
	{
		return Next(out val);
	}

	public bool Next(out T item)
	{
		if (_number < _count)
		{
			item = _node;
			_node = _node.next;
			_number++;
			return true;
		}
		item = null;
		return false;
	}
}
