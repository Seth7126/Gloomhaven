using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AddressableMisc;

public class AddressableContext
{
	public List<AsyncOperationHandle> AsyncOperationHandles { get; set; }

	public AddressableContext(IEnumerable<AsyncOperationHandle> asyncOperationHandles)
		: this()
	{
		AsyncOperationHandles.AddRange(asyncOperationHandles);
	}

	public AddressableContext()
	{
		AsyncOperationHandles = new List<AsyncOperationHandle>();
	}

	public void ReleaseAll()
	{
		foreach (AsyncOperationHandle asyncOperationHandle in AsyncOperationHandles)
		{
			Addressables.Release(asyncOperationHandle);
		}
		AsyncOperationHandles.Clear();
	}
}
