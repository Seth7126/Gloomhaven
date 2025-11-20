using System;
using System.Collections;
using System.Collections.Generic;
using Chronos;
using SM.Gamepad;
using ScenarioRuleLibrary.YML;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using UnityEngine;
using UnityEngine.UI;

public class UICampaignRewardWindow : MonoBehaviour
{
	[SerializeField]
	private HeaderHighlightText header;

	[SerializeField]
	private ExtendedButton continueButton;

	[SerializeField]
	private GridLayoutGroup rewardsSection;

	[SerializeField]
	private UICampaignRewardWindowSlot rewardPrefab;

	[SerializeField]
	private int initialPool = 5;

	[SerializeField]
	private float delayBetweenRewards = 0.2f;

	[SerializeField]
	private float delayRevealRewards = 0.2f;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	[SerializeField]
	private Hotkey _continueHotkey;

	[SerializeField]
	private Image _continueBackground;

	private List<UICampaignRewardWindowSlot> rewardsUI = new List<UICampaignRewardWindowSlot>();

	private UIWindow window;

	private Action onFinishedRevealAnimation;

	private Action continueAction;

	private Func<Reward, bool> highlightChecker;

	private bool isRevealing;

	private List<Reward> rewards;

	private SimpleKeyActionHandlerBlocker animationBlocker;

	protected void Awake()
	{
		window = GetComponent<UIWindow>();
		if (!InputManager.GamePadInUse)
		{
			continueButton.onClick.AddListener(OnContinueButtonClick);
		}
		window.onTransitionComplete.AddListener(delegate(UIWindow w, UIWindow.VisualState stat)
		{
			switch (stat)
			{
			case UIWindow.VisualState.Hidden:
				OnHidden();
				break;
			case UIWindow.VisualState.Shown:
				OnShown();
				break;
			}
		});
		HelperTools.NormalizePool(ref rewardsUI, rewardPrefab.gameObject, rewardsSection.transform, initialPool, delegate(UICampaignRewardWindowSlot obj)
		{
			obj.gameObject.SetActive(value: false);
		});
		controllerArea.OnFocusedArea.AddListener(OnFocused);
		if (InputManager.GamePadInUse)
		{
			animationBlocker = new SimpleKeyActionHandlerBlocker(isBlock: true);
			Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, OnContinueButtonClick, ShowHotkey, HideHotkey).AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerArea)).AddBlocker(new ExtendedButtonActiveKeyActionHandlerBlocker(continueButton)).AddBlocker(new ExtendedButtonInteractableKeyActionHandlerBlocker(continueButton))
				.AddBlocker(animationBlocker));
		}
	}

	private void ShowHotkey()
	{
		_continueHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
		_continueBackground.enabled = true;
	}

	private void HideHotkey()
	{
		if (InputManager.GamePadInUse)
		{
			_continueHotkey.DisplayHotkey(active: false);
			_continueHotkey.Deinitialize();
			_continueBackground.enabled = false;
		}
	}

	private void OnDestroy()
	{
		if (!InputManager.GamePadInUse)
		{
			continueButton.onClick.RemoveAllListeners();
		}
		Singleton<KeyActionHandlerController>.Instance?.RemoveHandler(KeyAction.UI_SUBMIT, OnContinueButtonClick);
	}

	private void OnFocused()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.CampaignReward);
	}

	public void Show(string titleKey, List<Reward> rewards)
	{
		this.rewards = rewards;
		header.SetHighlgihtText(titleKey);
		HelperTools.NormalizePool(ref rewardsUI, rewardPrefab.gameObject, rewardsSection.transform, rewards.Count);
		for (int i = 0; i < rewards.Count; i++)
		{
			rewardsUI[i].Hide();
		}
		isRevealing = true;
		if (window.IsOpen)
		{
			OnShown();
		}
		else
		{
			window.Show();
		}
		controllerArea.Enable();
		if ((bool)InitiativeTrack.Instance)
		{
			InitiativeTrack.Instance.ToggleSortingOrder(value: false);
		}
	}

	public void SetRewardsStartAxis(GridLayoutGroup.Axis axis)
	{
		rewardsSection.startAxis = axis;
	}

	public void SetOnFinishedRevealAnimation(Action onFinishedRevealAnimation)
	{
		this.onFinishedRevealAnimation = onFinishedRevealAnimation;
	}

	public void SetRewardHighlightChecker(Func<Reward, bool> highlightChecker)
	{
		this.highlightChecker = highlightChecker;
	}

	public void EnableContinueButton(string closeKey, Action continueAction, bool isInteractable = true)
	{
		this.continueAction = continueAction;
		continueButton.gameObject.SetActive(value: false);
		continueButton.TextLanguageKey = closeKey;
		SetContinueButtonInteractable(isInteractable);
	}

	public void SetContinueButtonInteractable(bool isInteractable)
	{
		continueButton.interactable = isInteractable;
	}

	public void DisableContinueButton()
	{
		continueAction = null;
		continueButton.gameObject.SetActive(value: false);
	}

	private void OnShown()
	{
		header.Show(delegate
		{
			StartCoroutine(ShowRewards(rewards));
		});
	}

	public void Hide()
	{
		if ((bool)InitiativeTrack.Instance)
		{
			InitiativeTrack.Instance.ToggleSortingOrder(value: true);
		}
		window.Hide();
	}

	private void OnDisable()
	{
		controllerArea.Destroy();
		StopAnimations();
	}

	private void StopAnimations()
	{
		for (int i = 0; i < rewardsUI.Count && rewardsUI[i].gameObject.activeSelf; i++)
		{
			rewardsUI[i].StopAnimations();
		}
		header.Hide();
	}

	private void OnHidden()
	{
		controllerArea.Destroy();
		StopAnimations();
	}

	private IEnumerator ShowRewards(List<Reward> rewards)
	{
		animationBlocker?.SetBlock(value: true);
		isRevealing = true;
		if (rewardsSection.startAxis == GridLayoutGroup.Axis.Vertical)
		{
			for (int i = 0; i < rewards.Count; i++)
			{
				Reward reward = rewards[i];
				rewardsUI[i].ShowReward(reward, highlightChecker != null && highlightChecker(reward));
				yield return Timekeeper.instance.WaitForSeconds(delayBetweenRewards);
			}
		}
		else
		{
			Vector2 cellsPerAxis = rewardsSection.GetCellCountPerAxis(rewards.Count);
			for (int i = 0; (float)i < cellsPerAxis.x; i++)
			{
				for (int j = 0; (float)j < cellsPerAxis.y; j++)
				{
					int num = i + j * (int)cellsPerAxis.x;
					if (num >= rewards.Count)
					{
						break;
					}
					rewardsUI[num].ShowReward(rewards[num], highlightChecker != null && highlightChecker(rewards[num]));
					yield return Timekeeper.instance.WaitForSeconds(delayBetweenRewards);
				}
			}
		}
		for (int k = 0; k < rewards.Count; k++)
		{
			rewardsUI[k].Reveal();
		}
		yield return Timekeeper.instance.WaitForSeconds(delayRevealRewards);
		isRevealing = false;
		if (continueAction != null)
		{
			continueButton.gameObject.SetActive(value: true);
			Singleton<UINavigation>.Instance.NavigationManager.TrySelectFirstIn(Singleton<UINavigation>.Instance.NavigationManager.CurrentNavigationRoot);
		}
		animationBlocker?.SetBlock(value: false);
		onFinishedRevealAnimation?.Invoke();
	}

	private void OnContinueButtonClick()
	{
		continueAction?.Invoke();
	}
}
