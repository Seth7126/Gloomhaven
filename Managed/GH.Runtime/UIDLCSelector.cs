using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM.MainMenu;
using GLOOM.MainMenu.DLC;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

public class UIDLCSelector : MonoBehaviour
{
	[Serializable]
	private class DLCStateText
	{
		[SerializeField]
		private TextMeshProUGUI text;

		[SerializeField]
		private Color availableColor;

		[SerializeField]
		private Color unavailableColor;

		public void ShowAvailable()
		{
			text.color = availableColor;
		}

		public void ShowUnavailable()
		{
			text.color = unavailableColor;
		}

		public void SetDefaultColor()
		{
			availableColor = text.color;
		}
	}

	[SerializeField]
	private ExtendedScrollRect container;

	[SerializeField]
	private List<UIDLCSelectorOption> optionsPool;

	[SerializeField]
	private DLCStateText[] texts;

	[SerializeField]
	private CanvasGroup hotkeyCanvasGroup;

	[SerializeField]
	private UICreateGameDLCStep _createGameDlcStep;

	[SerializeField]
	private DLCsContentInfo _dlCsContentInfo;

	private DLCRegistry.EDLCKey selectedDLC;

	private int _selectedCount;

	public bool IsActivateInLoadState { get; set; }

	public bool IsAnyDlcSelected => _selectedCount > 0;

	private void Awake()
	{
		for (int i = 0; i < optionsPool.Count; i++)
		{
			optionsPool[i].OnToggledDLC.AddListener(OnToggledDLC);
		}
		UpdateHotkeyInteractable();
		for (int j = 0; j < texts.Length; j++)
		{
			texts[j].ShowAvailable();
		}
		UpdateOptions();
	}

	public void UpdateOptions()
	{
		List<DLCRegistry.EDLCKey> list = DLCRegistry.DLCKeys.Where((DLCRegistry.EDLCKey it) => it != DLCRegistry.EDLCKey.None && PlatformLayer.DLC.UserInstalledDLC(it) && _dlCsContentInfo.HasDlcModel(it)).ToList();
		List<DLCRegistry.EDLCKey> list2 = DLCRegistry.DLCKeys.Where((DLCRegistry.EDLCKey it) => it != DLCRegistry.EDLCKey.None && !PlatformLayer.DLC.UserInstalledDLC(it) && UIInfoTools.Instance.GetDLCState(it) == DLCConfig.EDLCState.Available && _dlCsContentInfo.HasDlcModel(it)).ToList();
		List<DLCRegistry.EDLCKey> list3 = DLCRegistry.DLCKeys.Where((DLCRegistry.EDLCKey it) => it != DLCRegistry.EDLCKey.None && !PlatformLayer.DLC.UserInstalledDLC(it) && UIInfoTools.Instance.GetDLCState(it) == DLCConfig.EDLCState.ComingSoon && _dlCsContentInfo.HasDlcModel(it)).ToList();
		int num = list.Count + list2.Count + list3.Count;
		for (int num2 = 0; num2 < optionsPool.Count; num2++)
		{
			if (num2 < num)
			{
				if (num2 < list.Count)
				{
					optionsPool[num2].SetAvailableDLC(list[num2]);
				}
				else if (num2 < list.Count + list2.Count)
				{
					optionsPool[num2].SetPurchaseableDLC(list2[num2 - list.Count]);
				}
				else
				{
					optionsPool[num2].SetComingSoonDLC(list3[num2 - (list.Count + list2.Count)]);
				}
			}
			else
			{
				optionsPool[num2].gameObject.SetActive(value: false);
			}
		}
	}

	private void OnToggledDLC(DLCRegistry.EDLCKey dlc, bool toggled)
	{
		if (toggled)
		{
			selectedDLC |= dlc;
			_selectedCount++;
		}
		else
		{
			selectedDLC &= ~dlc;
			_selectedCount--;
		}
		UpdateHotkeyInteractable();
	}

	private void UpdateHotkeyInteractable()
	{
		if (!InputManager.GamePadInUse)
		{
			return;
		}
		if (!IsActivateInLoadState)
		{
			_createGameDlcStep?.EnableLongConfirmInput();
			hotkeyCanvasGroup.alpha = 1f;
			return;
		}
		hotkeyCanvasGroup.alpha = (IsAnyDlcSelected ? 1f : 0f);
		if (IsAnyDlcSelected)
		{
			_createGameDlcStep.EnableLongConfirmInput();
		}
		else
		{
			_createGameDlcStep.DisableLongConfirmInput();
		}
	}

	public void SetValue(DLCRegistry.EDLCKey selected)
	{
		selectedDLC = selected;
		for (int i = 0; i < optionsPool.Count && optionsPool[i].gameObject.activeSelf; i++)
		{
			optionsPool[i].SetSelected(selected.HasFlag(optionsPool[i].DLC));
		}
	}

	public void SetInteractable(DLCRegistry.EDLCKey selected)
	{
		for (int i = 0; i < optionsPool.Count && optionsPool[i].gameObject.activeSelf; i++)
		{
			optionsPool[i].SetInteractable(!selected.HasFlag(optionsPool[i].DLC));
		}
		UpdateHotkeyInteractable();
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (hasFocus)
		{
			UpdateOptions();
		}
	}

	public DLCRegistry.EDLCKey GetValue()
	{
		return selectedDLC;
	}
}
