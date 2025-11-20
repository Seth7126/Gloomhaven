using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using Code.State;
using GLOOM;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using Script.GUI.SMNavigation.States.PopupStates;
using Script.GUI.SMNavigation.States.ScenarioStates;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterStoryBox : MonoBehaviour
{
	[SerializeField]
	private Image portrait;

	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private TextMeshProUGUI dialogText;

	[SerializeField]
	private Image shieldIcon;

	[SerializeField]
	private string audioItemSkip = "PlaySound_UIDialogueBoxSkip";

	[SerializeField]
	private StoryImageViewer narrativeFlow;

	[SerializeField]
	private GUIAnimator showTextLineAnimator;

	[SerializeField]
	private GUIAnimator showDialogAnimator;

	[SerializeField]
	private Button skipButton;

	[SerializeField]
	private LayoutElementExtended maxSizeContainer;

	[SerializeField]
	private int maxSize = 300;

	[SerializeField]
	private List<LayoutElement> horizontalSpaceLayouts;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	[SerializeField]
	private Hotkey hotkey;

	private List<DialogLineDTO> dialogs;

	private int currentDialogIndex = -1;

	private Action onFinish;

	private Action _storyFinishedCallback;

	private SimpleKeyActionHandlerBlocker skipButtonBlocker;

	private IStateFilter _stateFilter = new StateFilterByType(typeof(DialogueMessageState), typeof(AnimationScenarioState), typeof(ConfirmationBoxState), typeof(GuildmasterRewardsState)).InverseFilter();

	private bool _navigateToPreviousStateWhenHidden;

	public bool IsStoryBoxFocused => controllerArea.IsFocused;

	private void Awake()
	{
		if (!InputManager.GamePadInUse)
		{
			skipButton.onClick.AddListener(Skip);
		}
		showDialogAnimator.OnAnimationFinished.AddListener(delegate
		{
			ShowLine(0);
		});
		IState state = Singleton<UINavigation>.Instance.StateMachine.GetState(PopupStateTag.DialogueMessage);
		IState state2 = Singleton<UINavigation>.Instance.StateMachine.GetState(CampaignMapStateTag.MapStory);
		SelectorActionHandlerBlocker keyActionHandlerBlocker = new SelectorActionHandlerBlocker(new StateActionHandlerBlocker(new HashSet<IState> { state, state2 }, invert: true), new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerArea), () => Singleton<UINavigation>.Instance != null && !Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<CampaignMapState>());
		skipButtonBlocker = new SimpleKeyActionHandlerBlocker(!skipButton.interactable);
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, Skip).AddBlocker(keyActionHandlerBlocker).AddBlocker(skipButtonBlocker));
		SetSkipButtonInteractable(value: false);
	}

	private void OnDestroy()
	{
		if (!CoreApplication.IsQuitting)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, Skip);
			if (!InputManager.GamePadInUse)
			{
				skipButton.onClick.RemoveAllListeners();
			}
			else
			{
				hotkey.Deinitialize();
			}
			_storyFinishedCallback = null;
		}
	}

	private void SetSkipButtonInteractable(bool value)
	{
		skipButton.interactable = value;
		skipButtonBlocker.SetBlock(!value);
	}

	public void Show(List<DialogLineDTO> lines, Action onFinish = null, bool navigateToPreviousStateWhenHidden = true)
	{
		dialogs = lines;
		this.onFinish = onFinish;
		_navigateToPreviousStateWhenHidden = navigateToPreviousStateWhenHidden;
		currentDialogIndex = -1;
		SetSkipButtonInteractable(value: false);
		if (lines.Count == 0)
		{
			Hide();
			return;
		}
		maxSizeContainer.MaxHeight = maxSize;
		foreach (LayoutElement horizontalSpaceLayout in horizontalSpaceLayouts)
		{
			horizontalSpaceLayout.gameObject.SetActive(value: true);
		}
		showTextLineAnimator.GoInitState();
		if (lines[0].narrativeImageID.IsNOTNullOrEmpty())
		{
			showDialogAnimator.GoInitState();
			narrativeFlow.LoadImages((from it in lines
				where it.narrativeImageID.IsNOTNullOrEmpty()
				select it.narrativeImageID).ToList(), delegate
			{
				narrativeFlow.Show(lines[0].narrativeImageID, ShowDialog);
			});
		}
		else
		{
			narrativeFlow.Hide();
			ShowDialog();
		}
		controllerArea.Enable();
		if (InputManager.GamePadInUse)
		{
			hotkey.Initialize(Singleton<UINavigation>.Instance.Input);
		}
	}

	public void AddFinishedCallback(Action finishedCallback)
	{
		_storyFinishedCallback = (Action)Delegate.Combine(_storyFinishedCallback, finishedCallback);
	}

	private void ShowDialog()
	{
		string translation = LocalizationManager.GetTranslation(dialogs[0].text);
		translation = Singleton<InputManager>.Instance.LocalizeControls(translation);
		dialogText.text = translation;
		DecorateBox(dialogs[0]);
		showDialogAnimator.Play();
	}

	private void ShowLine(int dialogIndex)
	{
		if (dialogs == null || dialogIndex >= dialogs.Count)
		{
			Hide();
			return;
		}
		maxSizeContainer.MaxHeight = maxSize;
		foreach (LayoutElement horizontalSpaceLayout in horizontalSpaceLayouts)
		{
			horizontalSpaceLayout.gameObject.SetActive(value: true);
		}
		StopCurrentAudio();
		currentDialogIndex = dialogIndex;
		SetSkipButtonInteractable(value: true);
		DialogLineDTO line = dialogs[currentDialogIndex];
		DecorateBox(line);
		DecorateLine(line);
		showTextLineAnimator.Play();
		StartCoroutine(CheckSize());
	}

	private void StopCurrentAudio()
	{
		if (currentDialogIndex >= 0 && dialogs != null && currentDialogIndex < dialogs.Count && dialogs[currentDialogIndex].narrativeAudioId.IsNOTNullOrEmpty())
		{
			AudioControllerUtils.StopSound(dialogs[currentDialogIndex].narrativeAudioId);
		}
	}

	private void DecorateLine(DialogLineDTO line)
	{
		string translation = LocalizationManager.GetTranslation(line.text);
		translation = Singleton<InputManager>.Instance.LocalizeControls(translation);
		dialogText.text = translation;
		if (line.cameraLocation.HasValue)
		{
			CameraController.s_CameraController.m_TargetFocalPoint = line.cameraLocation.Value;
		}
		if (line.narrativeImageID.IsNOTNullOrEmpty())
		{
			narrativeFlow.Show(line.narrativeImageID);
		}
		if (line.narrativeAudioId.IsNOTNullOrEmpty())
		{
			AudioControllerUtils.PlaySound(line.narrativeAudioId, optional: true);
		}
	}

	private IEnumerator CheckSize()
	{
		yield return null;
		bool flag = (maxSizeContainer.transform as RectTransform).rect.height == (float)maxSize;
		foreach (LayoutElement horizontalSpaceLayout in horizontalSpaceLayouts)
		{
			horizontalSpaceLayout.gameObject.SetActive(!flag);
		}
		maxSizeContainer.MaxHeight = (flag ? (-1) : maxSize);
	}

	private void DecorateBox(DialogLineDTO line)
	{
		title.text = line.title;
		portrait.sprite = UIInfoTools.Instance.GetStoryCharacterExpression(line.character, line.expression);
		shieldIcon.sprite = UIInfoTools.Instance.GetStoryCharacterShield(line.character);
	}

	private void ShowNextLine()
	{
		AudioControllerUtils.PlaySound(audioItemSkip);
		ShowLine(currentDialogIndex + 1);
	}

	public void Skip()
	{
		ShowNextLine();
	}

	public void Hide()
	{
		SetSkipButtonInteractable(value: false);
		if (_navigateToPreviousStateWhenHidden)
		{
			Singleton<UINavigation>.Instance.StateMachine.ToNonMenuPreviousState(_stateFilter);
		}
		OnHidden();
		onFinish?.Invoke();
		_storyFinishedCallback?.Invoke();
		_storyFinishedCallback = null;
		if (InputManager.GamePadInUse)
		{
			hotkey.Deinitialize();
		}
	}

	private void OnHidden()
	{
		StopCurrentAudio();
		showDialogAnimator?.Stop();
		showTextLineAnimator?.Stop();
		narrativeFlow.Hide();
		controllerArea.Destroy();
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			OnHidden();
		}
	}
}
