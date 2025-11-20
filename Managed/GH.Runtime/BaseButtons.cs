using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BaseButtons : MonoBehaviour
{
	[SerializeField]
	private GameObject[] baseButtonList;

	[SerializeField]
	private GameObject dialogueContent;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	private bool dialogueContentFull;

	private SkipFrameKeyActionHandlerBlocker _skipFrameKeyActionHandlerBlocker;

	private SimpleKeyActionHandlerBlocker _simpleKeyActionBlocker;

	private HashSet<object> _disableRequests = new HashSet<object>();

	private bool IsEnabled => !_disableRequests.Any();

	public event Action<bool> ChangeCanvasVisible;

	private void Awake()
	{
		_skipFrameKeyActionHandlerBlocker = new SkipFrameKeyActionHandlerBlocker(this, defaultIsBlock: false);
		_simpleKeyActionBlocker = new SimpleKeyActionHandlerBlocker();
	}

	private void OnEnable()
	{
		RunBlocker();
		AddBindings();
	}

	private void OnDisable()
	{
		RemoveBindings();
	}

	public void ToggleBurnOneCardBlock(bool isBlocked)
	{
		_simpleKeyActionBlocker.SetBlock(isBlocked);
	}

	private void AddBindings()
	{
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.BURN_ONE_CARD, BurnOneCard).AddBlocker(_simpleKeyActionBlocker).AddBlocker(_skipFrameKeyActionHandlerBlocker));
	}

	private void RemoveBindings()
	{
		if (Singleton<KeyActionHandlerController>.Instance != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.BURN_ONE_CARD, BurnOneCard);
		}
	}

	private void BurnOneCard()
	{
		if ((!Singleton<UIResultsManager>.IsInitialized || !Singleton<UIResultsManager>.Instance.IsShown) && (!Singleton<StoryController>.IsInitialized || !Singleton<StoryController>.Instance.IsVisible) && !Singleton<ESCMenu>.Instance.IsOpen && Choreographer.s_Choreographer.PlayerSelectingToAvoidDamageOrNot && !GetChildWithName(dialogueContent, "Full") && !Singleton<InputManager>.Instance.PlayerControl.ActionIsHandled(Singleton<InputManager>.Instance.PlayerControl.UICancel) && !baseButtonList[0].activeInHierarchy)
		{
			clickButton(baseButtonList[3]);
		}
	}

	private bool GetChildWithName(GameObject obj, string name)
	{
		bool flag = false;
		if (obj.transform.Find(name) != null)
		{
			return true;
		}
		return false;
	}

	private void Update()
	{
		if ((Singleton<UIResultsManager>.IsInitialized && Singleton<UIResultsManager>.Instance.IsShown) || (Singleton<StoryController>.IsInitialized && Singleton<StoryController>.Instance.IsVisible) || Singleton<ESCMenu>.Instance.IsOpen || !IsEnabled)
		{
			return;
		}
		if (InputManager.GetWasPressed(KeyAction.CLEAR_TARGETS_OR_UNDO) && baseButtonList[0].activeInHierarchy)
		{
			clickButton(baseButtonList[0]);
		}
		else if (InputManager.GetWasPressed(KeyAction.CONFIRM_ACTION_BUTTON) && baseButtonList[1].activeInHierarchy)
		{
			clickButton(baseButtonList[1]);
		}
		else if (InputManager.GetWasPressed(KeyAction.SKIP_ATTACK) && baseButtonList[2].activeInHierarchy)
		{
			clickButton(baseButtonList[2]);
		}
		if (Choreographer.s_Choreographer.PlayerSelectingToAvoidDamageOrNot && !GetChildWithName(dialogueContent, "Full"))
		{
			if (InputManager.GetWasPressed(KeyAction.RECEIVE_DAMAGE) && !baseButtonList[1].activeInHierarchy)
			{
				clickButton(baseButtonList[4]);
			}
			else if (InputManager.GetWasPressed(KeyAction.BURN_TWO_CARDS) && !baseButtonList[2].activeInHierarchy)
			{
				clickButton(baseButtonList[5]);
			}
		}
	}

	private void clickButton(GameObject Objbutton)
	{
		if (Objbutton.GetComponent<Selectable>().IsInteractable())
		{
			ExecuteEvents.Execute(Objbutton, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
		}
	}

	public void RunBlocker()
	{
		_skipFrameKeyActionHandlerBlocker.Run();
	}

	public void Toggle(object requester, bool isEnabled)
	{
		if (isEnabled)
		{
			_disableRequests.Remove(requester);
		}
		else
		{
			_disableRequests.Add(requester);
		}
		RefreshEnabled();
	}

	private void RefreshEnabled()
	{
		bool isEnabled = IsEnabled;
		_canvasGroup.alpha = (isEnabled ? 1f : 0f);
		_canvasGroup.interactable = isEnabled;
		this.ChangeCanvasVisible?.Invoke(IsEnabled);
	}
}
