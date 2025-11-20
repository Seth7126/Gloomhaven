namespace UnityEngine.UIElements.UIR;

internal class BasicNode<T> : LinkedPoolItem<BasicNode<T>>
{
	public BasicNode<T> next;

	public T data;

	public void AppendTo(ref BasicNode<T> first)
	{
		if (first == null)
		{
			first = this;
			return;
		}
		BasicNode<T> basicNode = first;
		while (basicNode.next != null)
		{
			basicNode = basicNode.next;
		}
		basicNode.next = this;
	}
}
