using Script.GUI.SMNavigation.Tabs;
using UnityEngine;

[RequireComponent(typeof(TabComponentInputListener))]
public class FTUECharactersTabController : MonoBehaviour
{
	private TabComponentInputListener _tabInput;

	private void Awake()
	{
		if (Singleton<MapFTUEManager>.Instance != null)
		{
			_tabInput = GetComponent<TabComponentInputListener>();
			Singleton<MapFTUEManager>.Instance.OnStartedStep.AddListener(OnStartedStep);
			Singleton<MapFTUEManager>.Instance.OnFinished.AddListener(OnFinished);
		}
	}

	private void OnStartedStep(EMapFTUEStep step)
	{
		switch (step)
		{
		case EMapFTUEStep.Initial:
			_tabInput.UnRegister();
			break;
		case EMapFTUEStep.SelectFirstSlot:
			_tabInput.Register();
			_tabInput.OnNext += UnregisterTab;
			_tabInput.OnPrevious += UnregisterTab;
			break;
		case EMapFTUEStep.CreatedFirstCharacter:
			NewPartyDisplayUI.PartyDisplay.ToggleLockTabInput(locked: true);
			_tabInput.UnRegister();
			break;
		case EMapFTUEStep.SelectSecondSlot:
			NewPartyDisplayUI.PartyDisplay.ToggleLockTabInput(locked: false);
			_tabInput.Register();
			break;
		case EMapFTUEStep.CreatedSecondCharacter:
			OnFinished();
			break;
		}
	}

	private void UnregisterTab()
	{
		_tabInput.OnNext -= UnregisterTab;
		_tabInput.OnPrevious -= UnregisterTab;
		_tabInput.UnRegister();
	}

	private void OnFinished()
	{
		_tabInput.OnNext -= UnregisterTab;
		_tabInput.OnPrevious -= UnregisterTab;
		_tabInput.Register();
		OnDestroy();
	}

	private void OnDestroy()
	{
		if (MapFTUEManager.IsPlaying)
		{
			Singleton<MapFTUEManager>.Instance.OnStartedStep.RemoveListener(OnStartedStep);
			Singleton<MapFTUEManager>.Instance.OnFinished.RemoveListener(OnFinished);
			_tabInput.OnNext -= UnregisterTab;
			_tabInput.OnPrevious -= UnregisterTab;
			_tabInput.UnRegister();
		}
	}
}
