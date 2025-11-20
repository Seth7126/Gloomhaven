#define ENABLE_LOGS
using System;
using GLOO.Introduction;
using JetBrains.Annotations;
using SM.Gamepad;
using ScenarioRuleLibrary.YML;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIPerksWindow : MonoBehaviour
{
	[SerializeField]
	private UIPerksInventory inventory;

	[SerializeField]
	private UIPerksActiveModifiers activePerkssDisplay;

	[SerializeField]
	private UIIntroduce introduction;

	[Header("Hotkeys")]
	[SerializeField]
	private Hotkey[] _hotkeys;

	private UIWindow window;

	private Action<AttackModifierYMLData> onHoveredPerk;

	private ICharacterService characterData;

	private RectTransform position;

	private Action onHidden;

	public bool IsVisible => window.IsOpen;

	public UIPerksInventory PerkDisplay => inventory;

	public UIPerksActiveModifiers ModifierDisplay => activePerkssDisplay;

	[UsedImplicitly]
	private void Awake()
	{
		window = GetComponent<UIWindow>();
		window.onHidden.AddListener(delegate
		{
			onHidden?.Invoke();
			inventory.HidePerkTooltip();
		});
		inventory.OnHoverSlot.AddListener(delegate(UIPerkInventorySlot slot)
		{
			activePerkssDisplay.PreviewAdd(slot.Perk);
		});
		inventory.OnUnhoverSlot.AddListener(delegate
		{
			activePerkssDisplay.ClearPreview();
		});
		inventory.OnEnabledPerk.AddListener(delegate
		{
			PerkDisplay.Refresh(forceReturnToPool: true);
			activePerkssDisplay.ClearPreview();
		});
		activePerkssDisplay.OnHovered.AddListener(delegate(AttackModifierYMLData perk)
		{
			onHoveredPerk?.Invoke(perk);
		});
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		SetHotkeysActive(value: false);
	}

	public void Show(ICharacterService characterData, RectTransform position, bool showTooltip, Action<AttackModifierYMLData> onHoveredPerk = null, bool editAllowed = true, Action onHidden = null)
	{
		SetHotkeysActive(value: true);
		window.Show();
		Debug.LogGUI("Open perks window " + characterData.CharacterModel);
		this.characterData = characterData;
		this.position = position;
		this.onHidden = onHidden;
		AnalyticsWrapper.LogScreenDisplay(AWScreenName.perk_selection);
		this.onHoveredPerk = onHoveredPerk;
		activePerkssDisplay.Display(characterData);
		inventory.Display(characterData, position, editAllowed);
		Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.Perks);
		ShowTooltip(showTooltip);
	}

	public void Refresh(bool editAllowed = true)
	{
		if (IsVisible && characterData != null)
		{
			Debug.LogGUI("Refresh perks window " + characterData.CharacterModel);
			PerkDisplay.Display(characterData, position, editAllowed);
			ModifierDisplay.Display(characterData);
		}
	}

	public void Hide(bool playHideAudio = true)
	{
		UIWindow uIWindow = window;
		if ((object)uIWindow != null && uIWindow.IsOpen)
		{
			SetHotkeysActive(value: false);
			introduction.Hide();
			string audioItemHide = window.AudioItemHide;
			if (!playHideAudio)
			{
				window.AudioItemHide = null;
			}
			Debug.LogGUI("Hide Perks Window");
			window.Hide();
			window.AudioItemHide = audioItemHide;
		}
	}

	private void ShowTooltip(bool showTooltip)
	{
		if (!showTooltip)
		{
			introduction.Hide();
		}
		else
		{
			introduction.Show();
		}
	}

	public void SetHotkeysActive(bool value)
	{
		if (value)
		{
			Hotkey[] hotkeys = _hotkeys;
			foreach (Hotkey obj in hotkeys)
			{
				obj.Initialize(Singleton<UINavigation>.Instance.Input);
				obj.DisplayHotkey(active: true);
			}
		}
		else
		{
			Hotkey[] hotkeys = _hotkeys;
			foreach (Hotkey obj2 in hotkeys)
			{
				obj2.Deinitialize();
				obj2.DisplayHotkey(active: false);
			}
		}
	}
}
