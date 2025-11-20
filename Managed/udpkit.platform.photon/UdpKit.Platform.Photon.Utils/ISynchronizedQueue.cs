namespace UdpKit.Platform.Photon.Utils;

internal interface ISynchronizedQueue<T>
{
	int Count { get; }

	void Clear();

	void Enqueue(T item);

	bool TryDequeue(out T item);
}
