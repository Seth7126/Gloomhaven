using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DebugMenuProvider : Singleton<DebugMenuProvider>
{
	[SerializeField]
	private AssetReference _referenceDebugMenu;

	private AsyncOperationHandle<GameObject> _handler;

	private DebugMenu _debugMenu;

	public DebugMenu DebugMenuInstance => GetDebugMenu();

	[UsedImplicitly]
	protected override void Awake()
	{
		Object.Destroy(this);
	}

	public void ClearMemory()
	{
		if (_debugMenu != null)
		{
			_debugMenu.gameObject.SetActive(value: false);
			_debugMenu.gameObject.transform.SetParent(null);
			AssetBundleManager.ReleaseHandle(_handler, releaseInstance: true);
			_handler = default(AsyncOperationHandle<GameObject>);
		}
	}

	public DebugMenu GetDebugMenu()
	{
		if (_debugMenu == null)
		{
			AssetBundleManager.ReleaseHandle(_handler, releaseInstance: true);
			_handler = Addressables.InstantiateAsync(_referenceDebugMenu.RuntimeKey, base.transform, instantiateInWorldSpace: false, trackHandle: false);
			_handler.WaitForCompletion();
			_debugMenu = _handler.Result.GetComponent<DebugMenu>();
			_debugMenu.gameObject.SetActive(value: false);
		}
		return _debugMenu;
	}
}
