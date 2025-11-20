#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using FFSNet;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShortRest : MonoBehaviour
{
	[SerializeField]
	private GameObject dialogPrefab;

	[SerializeField]
	private RectTransform dialogHolder;

	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private TextMeshProUGUI buttonText;

	[SerializeField]
	private Sprite regularSprite;

	[SerializeField]
	private Sprite regularHighlightedSprite;

	[SerializeField]
	private Sprite selectSprite;

	[SerializeField]
	private Sprite selectHighlightedSprite;

	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private GUIAnimator warningAnimator;

	[SerializeField]
	private GameObject improvedHighlight;

	[SerializeField]
	private TooltipUI tooltip;

	[SerializeField]
	private GameObject improvedDescriptionTooltip;

	[SerializeField]
	private CanvasGroup basicInfoTooltipCanvasGroup;

	[SerializeField]
	private InteractabilityIsolatedUIControl m_UIControlLimiting;

	[SerializeField]
	private GameObject unfocusedMask;

	private UnityAction<CPlayerActor> shortRestAction;

	private CPlayerActor playerActor;

	private bool isSelected;

	private bool isMouseIn;

	private bool isValid = true;

	private Action onClickInvalid;

	private YesNoDialog yesNoDialog;

	private ControllerInputAreaCustom controllerArea;

	public ExtendedButton Button => button;

	public bool IsSelected => isSelected;

	public bool IsInteractable => button.interactable;

	public CPlayerActor PlayerActor => playerActor;

	public static event Action<ShortRest, bool> OnSelectShortRest;

	public static event Action<ShortRest, bool> OnUnselectShortRest;

	public void Init(CPlayerActor playerActor, UnityAction<CPlayerActor> shortRestAction)
	{
		this.playerActor = playerActor;
		this.shortRestAction = shortRestAction;
		if (yesNoDialog == null)
		{
			yesNoDialog = UnityEngine.Object.Instantiate(dialogPrefab, InputManager.GamePadInUse ? dialogHolder.GetComponentInParent<Canvas>().transform : dialogHolder).GetComponent<YesNoDialog>();
			yesNoDialog.name = playerActor.CharacterClass.DefaultModel + "_" + yesNoDialog.name;
		}
		yesNoDialog.PrepareInteractabilityForPlayer(playerActor);
		yesNoDialog.Init("GUI_SHORT_REST_CONFIRMATION", autoClose: false, delegate
		{
			Select(active: false);
			shortRestAction(playerActor);
			UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.ShortRestConfirmed));
			controllerArea?.Destroy();
			controllerArea = null;
		}, delegate
		{
			Select(active: false);
			MouseExit();
			UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.ShortRestCancelled));
			if (InputManager.GamePadInUse)
			{
				EventSystem.current.SetSelectedGameObject(Button.gameObject);
			}
			ControllerInputAreaManager.Instance.FocusArea(EControllerInputAreaType.CharacterActions);
			controllerArea?.Destroy();
			controllerArea = null;
		});
		yesNoDialog.Hide();
		if (m_UIControlLimiting != null)
		{
			m_UIControlLimiting.ControlSecondIdentifier = playerActor.ActorGuid;
		}
		warningAnimator.Stop(goToEnd: true);
		SetValid(isValid: true);
		SetUnfocused(FFSNetwork.IsOnline && !playerActor.IsUnderMyControl);
	}

	public void Show(CPlayerActor actor, bool isSelected)
	{
		SetSelected(isSelected);
		if (!FFSNetwork.IsOnline || (playerActor.IsUnderMyControl && !Singleton<UIReadyToggle>.Instance.PlayersReady.Contains(PlayerRegistry.MyPlayer) && !Singleton<UIReadyToggle>.Instance.IsProgressingBar))
		{
			SetValid(isValid: true);
		}
		else if (playerActor.IsUnderMyControl)
		{
			SetValid(isValid: false, UIMultiplayerNotifications.ShowCancelReadiedCards);
		}
		else
		{
			SetValid(isValid: false, UIMultiplayerNotifications.ShowSelectedOtherPlayerCard);
		}
		SetUnfocused(FFSNetwork.IsOnline && !playerActor.IsUnderMyControl);
		base.gameObject.SetActive(value: true);
		SetImproved(actor.CharacterClass.ImprovedShortRest);
	}

	private void SetSelected(bool selected)
	{
		isSelected = selected;
		UpdateBackground();
	}

	private void SetUnfocused(bool unfocused)
	{
		unfocusedMask.SetActive(unfocused);
	}

	public void ResetSelection()
	{
		SetSelected(selected: false);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
		if (tooltip.gameObject.activeSelf)
		{
			tooltip.gameObject.SetActive(value: false);
			UIManager.Instance.UnhighlightElement(tooltip.gameObject, unlockUI: false);
		}
	}

	private void SetImproved(bool isImproved)
	{
		basicInfoTooltipCanvasGroup.alpha = (isImproved ? 0.5f : 1f);
		improvedDescriptionTooltip.SetActive(isImproved);
		improvedHighlight.SetActive(isImproved);
	}

	public void SetInteractable(bool interactable)
	{
		button.interactable = interactable;
		button.ToggleFade(!interactable);
		tooltip.Init("GUI_TOOLTIP_SHORT_REST", interactable ? null : "GUI_TOOLTIP_SHORT_REST_WARNING");
	}

	private void UpdateBackground()
	{
		if (isSelected)
		{
			backgroundImage.sprite = (isMouseIn ? selectHighlightedSprite : selectSprite);
		}
		else
		{
			backgroundImage.sprite = (isMouseIn ? regularHighlightedSprite : regularSprite);
		}
	}

	public void MouseClick()
	{
		Debug.Log("Clicked on the short rest button.");
		Select(!isSelected);
	}

	private void Select(bool active)
	{
		if (!button.interactable && active)
		{
			Debug.Log("Short rest button was about to be activated but it is not interactable. Returning.");
			return;
		}
		if (!isValid && active)
		{
			warningAnimator.Play();
			onClickInvalid?.Invoke();
			return;
		}
		SetSelected(active);
		if (isSelected)
		{
			tooltip.ToggleEnable(active: false);
		}
		else
		{
			CardsHandManager.Instance.GetHand(playerActor).ToggleHighlight(new List<CardPileType>
			{
				CardPileType.Hand,
				CardPileType.Round
			}, active: false);
		}
		ShowPopup(isSelected);
		InitiativeTrack.Instance.CheckRoundAbilityCardsOrLongRestSelected();
		UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.ShortRestPressed));
	}

	private void ShowPopup(bool active)
	{
		if (active)
		{
			Debug.Log("Activating Short Rest popup");
			yesNoDialog.Show(dialogHolder);
		}
		else if (!InputManager.GamePadInUse)
		{
			Debug.Log("Deactivating Short Rest popup");
			yesNoDialog.Hide();
		}
	}

	public void MouseEnter()
	{
		isMouseIn = true;
		UpdateBackground();
		CardsHandManager.Instance.GetHand(playerActor).ToggleHighlight(CardPileType.Discarded, active: true, playerActor.CharacterClass.LongRest, keepShortRest: true);
		if (!isSelected && !tooltip.gameObject.activeSelf)
		{
			UIManager.Instance.HighlightElement(tooltip.gameObject, fadeEverythingElse: false, lockUI: false);
			tooltip.gameObject.SetActive(value: true);
		}
		ShortRest.OnSelectShortRest?.Invoke(this, arg2: true);
	}

	public void MouseExit()
	{
		if (tooltip.gameObject.activeSelf)
		{
			tooltip.gameObject.SetActive(value: false);
			UIManager.Instance.UnhighlightElement(tooltip.gameObject, unlockUI: false);
		}
		if (isSelected)
		{
			Debug.Log("Hovering ended but the short rest button is selected. Returning from MouseExit function.");
			return;
		}
		isMouseIn = false;
		CardsHandManager.Instance.GetHand(playerActor).ToggleHighlight(new List<CardPileType>
		{
			CardPileType.Hand,
			CardPileType.Round
		}, active: false, keepLongRest: true);
		UpdateBackground();
		ShortRest.OnUnselectShortRest?.Invoke(this, arg2: false);
	}

	public void SetValid(bool isValid, Action onClickInvalid = null)
	{
		this.isValid = isValid;
		this.onClickInvalid = onClickInvalid;
	}

	private void OnDisable()
	{
		warningAnimator.Stop(goToEnd: true);
	}
}
