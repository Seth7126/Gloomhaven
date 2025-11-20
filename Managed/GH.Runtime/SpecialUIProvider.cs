using Script.GUI.IngameMenu.EscMenuVoiceChat;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using VoiceChat;

public class SpecialUIProvider : Singleton<SpecialUIProvider>
{
	[SerializeField]
	private AssetReference _referenceCompendiumUI;

	[SerializeField]
	private AssetReference _referenceUIMultiplayerVoiceChat;

	[SerializeField]
	private GameObject _voiceChatCanvasPrefab;

	[SerializeField]
	private InteractabilityIsolatedUIControl _interactabilityIsolatedUIControl;

	private AsyncOperationHandle<GameObject> _handlerCompendiumUI;

	private AsyncOperationHandle<GameObject> _handlerVoiceChat;

	private GameObject _compendiumUI;

	private VoceChatOptions _voiceChat;

	private EscMenuVoiceChatController _escMenuVoiceChatController;

	public GameObject CompendiumUIObject => GetCompendiumUI();

	public VoceChatOptions UIMultiplayerVoiceChat => GetVoiceChat();

	public EscMenuVoiceChatController EscMenuVoiceChatController => GetEscMenuVoiceChatController();

	public void Unload()
	{
		if (_compendiumUI != null)
		{
			UnloadPrefab(_compendiumUI, _handlerCompendiumUI);
			_handlerCompendiumUI = default(AsyncOperationHandle<GameObject>);
			_compendiumUI = null;
		}
		if (_voiceChat != null)
		{
			UnloadPrefab(_voiceChat.gameObject, _handlerVoiceChat);
			_handlerVoiceChat = default(AsyncOperationHandle<GameObject>);
			_voiceChat = null;
		}
		OtherUpdate();
	}

	private GameObject GetCompendiumUI()
	{
		if (_compendiumUI == null)
		{
			AssetBundleManager.ReleaseHandle(_handlerCompendiumUI, releaseInstance: true);
			_handlerCompendiumUI = Addressables.InstantiateAsync(_referenceCompendiumUI.RuntimeKey, base.transform, instantiateInWorldSpace: false, trackHandle: false);
			_handlerCompendiumUI.WaitForCompletion();
			_compendiumUI = _handlerCompendiumUI.Result;
			_compendiumUI.GetComponent<UIWindow>().OtherInit();
			OtherUpdate();
		}
		return _compendiumUI;
	}

	private VoceChatOptions GetVoiceChat()
	{
		if (_voiceChat == null)
		{
			_handlerVoiceChat = Addressables.InstantiateAsync(_referenceUIMultiplayerVoiceChat.RuntimeKey, base.transform, instantiateInWorldSpace: false, trackHandle: false);
			_handlerVoiceChat.WaitForCompletion();
			if (_handlerVoiceChat.Result != null)
			{
				_voiceChat = _handlerVoiceChat.Result.GetComponent<VoceChatOptions>();
			}
			OtherUpdate();
		}
		return _voiceChat;
	}

	private EscMenuVoiceChatController GetEscMenuVoiceChatController()
	{
		if (_escMenuVoiceChatController == null)
		{
			GameObject gameObject = Object.Instantiate(_voiceChatCanvasPrefab, null);
			_escMenuVoiceChatController = gameObject.GetComponentInChildren<EscMenuVoiceChatController>();
		}
		return _escMenuVoiceChatController;
	}

	private void UnloadPrefab(GameObject prefab, AsyncOperationHandle<GameObject> handle)
	{
		if (prefab != null)
		{
			prefab.SetActive(value: false);
			prefab.transform.SetParent(null);
			if (handle.IsValid())
			{
				AssetBundleManager.ReleaseHandle(handle, releaseInstance: true);
			}
		}
	}

	private void OtherUpdate()
	{
		if (_interactabilityIsolatedUIControl != null)
		{
			_interactabilityIsolatedUIControl.FillAllowedButtonsListWithChildrenOnly();
		}
	}
}
