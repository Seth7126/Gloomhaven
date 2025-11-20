using UnityEngine.ResourceManagement.AsyncOperations;

public readonly struct LoadedHandleModel
{
	public AsyncOperationHandle AsyncOperationHandle { get; }

	public bool AlwaysLoaded { get; }

	public LoadedHandleModel(AsyncOperationHandle asyncOperationHandle, bool alwaysLoaded = false)
	{
		AsyncOperationHandle = asyncOperationHandle;
		AlwaysLoaded = alwaysLoaded;
	}
}
