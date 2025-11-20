using System;
using System.Collections.Generic;
using AsmodeeNet.Foundation;
using Code.State;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UILevelUpCard : MonoBehaviour
{
	[SerializeField]
	private RectTransform cardHolder;

	[SerializeField]
	private GameObject newHighlight;

	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private RectTransform animatedRectTranform;

	[SerializeField]
	private float animationTime = 0.3f;

	[SerializeField]
	private Image lockIcon;

	[SerializeField]
	private float fullCardPositionX = -642f;

	[SerializeField]
	private RectTransform fullCardHolder;

	private AbilityCardUI abilityCard;

	private bool highlighted;

	private bool isNew;

	public UnityEvent OnMouseEnter = new UnityEvent();

	public UnityEvent OnMouseExit = new UnityEvent();

	private LTDescr animMove;

	private UiNavigationTransitMarkableGroup _transitGroup;

	private bool _transitSelected;

	private bool _isSelected;

	private SimpleKeyActionHandlerBlocker _blocker;

	private Vector3 _fullCardPrewiewOffset;

	public Button.ButtonClickedEvent OnClick => button.onClick;

	public bool IsInteractable => button.IsInteractable();

	public CAbilityCard AbilityCard => abilityCard?.AbilityCard;

	public FullAbilityCard FullAbilityCard => abilityCard.fullAbilityCard;

	public Selectable Selectable => button;

	private void Awake()
	{
		IState state = Singleton<UINavigation>.Instance.StateMachine.GetState(CampaignMapStateTag.MapStory);
		_blocker = new SimpleKeyActionHandlerBlocker(isBlock: true);
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, OnSubmitPressed).AddBlocker(_blocker).AddBlocker(new StateActionHandlerBlocker(new HashSet<IState> { state })));
		_fullCardPrewiewOffset = (InputManager.GamePadInUse ? new Vector3(-75f, -45f) : new Vector3(-60f, -45f));
	}

	private void OnDestroy()
	{
		if (Singleton<KeyActionHandlerController>.IsInitialized)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, OnSubmitPressed);
		}
		OnMouseEnter.RemoveAllListeners();
		OnMouseExit.RemoveAllListeners();
		animMove = null;
		button.onClick.RemoveAllListeners();
		OnCancelLeanTween();
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			Clear();
		}
	}

	public IUiNavigationSelectable GetMiniCardNavigationSelectable()
	{
		IUiNavigationSelectable result = null;
		if (abilityCard != null)
		{
			result = abilityCard.MiniAbilityCard.GetNavigationSelectable();
		}
		return result;
	}

	public void MouseEnter()
	{
		if (Singleton<UINavigation>.Instance.StateMachine.CurrentState != Singleton<UINavigation>.Instance.StateMachine.GetState(CampaignMapStateTag.ShowingCardLevelUp))
		{
			highlighted = true;
			SetBackground();
			if (!button.interactable || InputManager.GamePadInUse)
			{
				abilityCard.ToggleFullCardPreview(isHighlighted: true, fullCardHolder);
				abilityCard.fullAbilityCard.BlockRaycasts(block: true);
				RectTransform rectTransform = abilityCard.fullAbilityCard.transform as RectTransform;
				abilityCard.fullAbilityCard.transform.position = abilityCard.MiniAbilityCard.transform.position + _fullCardPrewiewOffset;
				abilityCard.fullAbilityCard.transform.position += rectTransform.DeltaWorldPositionToFitTheScreen(UIManager.Instance.UICamera, 200f, 50f);
			}
			OnMouseEnter.Invoke();
		}
	}

	public void MouseExit()
	{
		if (Singleton<UINavigation>.Instance.StateMachine.CurrentState != Singleton<UINavigation>.Instance.StateMachine.GetState(CampaignMapStateTag.ShowingCardLevelUp) && base.gameObject.activeInHierarchy)
		{
			highlighted = false;
			SetBackground();
			if (!_transitSelected)
			{
				abilityCard.ToggleFullCardPreview(isHighlighted: false);
			}
			OnMouseExit.Invoke();
		}
	}

	private void SetBackground()
	{
		abilityCard.ToggleHighlight(isNew, highlighted);
	}

	public void Init(CAbilityCard card, bool isNew, string characterID)
	{
		this.isNew = isNew;
		Clear();
		abilityCard = ObjectPool.SpawnCard(card.ID, ObjectPool.ECardType.Ability, cardHolder, resetLocalScale: true, resetToMiddle: true).GetComponent<AbilityCardUI>();
		abilityCard.ChangeDefaultHighlightScale(button.highlightScaleFactor);
		abilityCard.ChangeDefaultHoverMovement(button.hoverMovement);
		abilityCard.MarkAsInFurtherAbilityPanel(value: true);
		abilityCard.transform.SetAsFirstSibling();
		abilityCard.Init(card, null, characterID, null, null, null, null, disableFullCard: false, (RectTransform rect) => new Vector2(fullCardPositionX, rect.anchoredPosition.y));
		abilityCard.PlayEffect(InitiativeTrackActorAvatar.InitiativeEffects.None);
		abilityCard.CardSelected += OnSelectedCard;
		abilityCard.CardDeselected += OnDeselectedCard;
		if (InputManager.GamePadInUse)
		{
			_transitGroup = abilityCard.transform.GetComponent<UiNavigationTransitMarkableGroup>();
			if (_transitGroup != null)
			{
				_transitGroup.OnTransitMarkedEvent += OnNavigationTransitSelected;
				_transitGroup.OnTransitUnmarkedEvent += OnNavigationTransitDeselected;
			}
		}
		newHighlight.SetActive(isNew);
		base.gameObject.SetActive(value: true);
		if (isNew)
		{
			button.enabled = false;
			animatedRectTranform.anchoredPosition = new Vector2(animatedRectTranform.rect.width, animatedRectTranform.anchoredPosition.y);
			animMove = LeanTween.move(animatedRectTranform, new Vector2(0f, animatedRectTranform.anchoredPosition.y), animationTime).setOnComplete((Action)delegate
			{
				button.enabled = true;
				animMove = null;
			});
		}
		else
		{
			animatedRectTranform.anchoredPosition = new Vector2(0f, animatedRectTranform.anchoredPosition.y);
		}
	}

	private void OnSubmitPressed()
	{
		if (_transitSelected || _isSelected)
		{
			UILevelUpWindow instance = Singleton<UILevelUpWindow>.Instance;
			if ((object)instance != null && !instance.IsShowing)
			{
				OnClick?.Invoke();
			}
		}
	}

	public void SetLocked(bool locked)
	{
		lockIcon.gameObject.SetActive(locked);
		button.interactable = !locked;
	}

	public void Hide()
	{
		OnCancelLeanTween();
		button.enabled = false;
		abilityCard.fullAbilityCard.bottomActionButton.actionButton.gameObject.SetActive(value: false);
		abilityCard.fullAbilityCard.topActionButton.actionButton.gameObject.SetActive(value: false);
		animMove = LeanTween.move(animatedRectTranform, new Vector2(animatedRectTranform.rect.width, animatedRectTranform.anchoredPosition.y), animationTime).setOnComplete((Action)delegate
		{
			abilityCard.fullAbilityCard.bottomActionButton.actionButton.gameObject.SetActive(value: true);
			abilityCard.fullAbilityCard.topActionButton.actionButton.gameObject.SetActive(value: true);
			button.enabled = true;
			animMove = null;
			base.gameObject.SetActive(value: false);
		});
	}

	private void OnCancelLeanTween()
	{
		if (animMove != null)
		{
			LeanTween.cancel(animMove.id);
			animMove = null;
		}
	}

	private void Clear()
	{
		button.enabled = true;
		animatedRectTranform.anchoredPosition = new Vector2(animatedRectTranform.rect.width, animatedRectTranform.anchoredPosition.y);
		OnCancelLeanTween();
		if (abilityCard != null)
		{
			abilityCard.CardSelected -= OnSelectedCard;
			abilityCard.CardDeselected -= OnDeselectedCard;
			abilityCard.MarkAsInFurtherAbilityPanel(value: false);
			abilityCard.ToggleFullCardPreview(isHighlighted: false);
			if (_transitGroup != null)
			{
				_transitGroup.OnTransitMarkedEvent -= OnNavigationTransitSelected;
				_transitGroup.OnTransitUnmarkedEvent -= OnNavigationTransitDeselected;
			}
			ObjectPool.RecycleCard(abilityCard.CardID, ObjectPool.ECardType.Ability, abilityCard.gameObject);
			abilityCard = null;
		}
		SetLocked(locked: false);
	}

	private void OnNavigationTransitSelected(IUiNavigationSelectable selectable)
	{
		if (animMove != null)
		{
			ForceFinishLeanTween();
		}
		_transitSelected = true;
		button.OnPointerEnter(null);
		MouseEnter();
	}

	private void OnNavigationTransitDeselected(IUiNavigationSelectable selectable)
	{
		if (animMove != null)
		{
			ForceFinishLeanTween();
		}
		_transitSelected = false;
		button.OnPointerExit(null);
		MouseExit();
	}

	private void ForceFinishLeanTween()
	{
		animMove.rectTransform.anchoredPosition = animMove.to;
		animMove.callOnCompletes();
		OnCancelLeanTween();
	}

	private void OnSelectedCard()
	{
		_blocker.SetBlock(value: false);
		_isSelected = true;
		button.OnPointerEnter(null);
		MouseEnter();
	}

	private void OnDeselectedCard()
	{
		_blocker.SetBlock(value: true);
		_isSelected = false;
		button.OnPointerExit(null);
		MouseExit();
	}
}
