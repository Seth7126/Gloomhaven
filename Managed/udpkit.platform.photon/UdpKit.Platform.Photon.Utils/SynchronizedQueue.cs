using System.Collections.Generic;

namespace UdpKit.Platform.Photon.Utils;

internal class SynchronizedQueue<T> : ISynchronizedQueue<T>
{
	private readonly Queue<T> queue = new Queue<T>();

	public int Count
	{
		get
		{
			lock (queue)
			{
				return queue.Count;
			}
		}
	}

	public void Clear()
	{
		lock (queue)
		{
			queue.Clear();
		}
	}

	public void Enqueue(T item)
	{
		lock (queue)
		{
			queue.Enqueue(item);
		}
	}

	public bool TryDequeue(out T item)
	{
		lock (queue)
		{
			if (queue.Count > 0)
			{
				item = queue.Dequeue();
				return true;
			}
			item = default(T);
			return false;
		}
	}
}
