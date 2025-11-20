using Assets.Script.Misc;
using GLOOM;
using MapRuleLibrary.Party;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.PopupStates;
using UnityEngine;
using UnityEngine.UI;

public class UICompletedPersonalQuestWindow : MonoBehaviour
{
	[SerializeField]
	private UIWindow window;

	[SerializeField]
	protected UICharacterPersonalQuest personalQuest;

	[SerializeField]
	protected GUIAnimator completedAnimation;

	[SerializeField]
	protected ExtendedButton closeButton;

	[SerializeField]
	protected HeaderHighlightText header;

	private ControllerInputAreaLocal controllerArea;

	private CallbackPromise promise;

	private CMapCharacter character;

	private void Awake()
	{
		controllerArea = GetComponent<ControllerInputAreaLocal>();
		controllerArea?.OnFocusedArea.AddListener(OnFocused);
		window.onTransitionComplete.AddListener(delegate(UIWindow _, UIWindow.VisualState state)
		{
			if (state == UIWindow.VisualState.Hidden)
			{
				OnHidden();
			}
		});
		completedAnimation.OnAnimationFinished.AddListener(OnFinishedCompleteAnimation);
		closeButton.onClick.AddListener(window.Hide);
	}

	private void OnDestroy()
	{
		closeButton.onClick.RemoveAllListeners();
	}

	private void OnFocused()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(PopupStateTag.PersonalQuestCompleted);
	}

	public ICallbackPromise Show(CMapCharacter character, PersonalQuestDTO personalQuestDTO)
	{
		promise = new CallbackPromise();
		completedAnimation.Stop();
		completedAnimation.GoInitState();
		header.gameObject.SetActive(value: false);
		header.Hide();
		closeButton.gameObject.SetActive(value: false);
		personalQuest.SetPersonalQuest(character, personalQuestDTO.Data, personalQuestDTO.CurrentProgress, personalQuestDTO.TotalProgress, personalQuestDTO.Step, delegate
		{
			if (character.PersonalQuest.CurrentPersonalQuestStep > personalQuestDTO.Step)
			{
				personalQuest.SetPersonalQuest(character);
				character.PersonalQuest.LastProgressShown = character.PersonalQuest.PersonalQuestConditionState.CurrentProgress;
				closeButton.TextLanguageKey = "GUI_CONTINUE_PERSONAL_QUEST_PROGRESSION";
				header.SetHighlgihtText("GUI_PERSONAL_QUEST_PROGRESS_COMPLETED");
				Singleton<HelpBox>.Instance.ShowTranslated(string.Format(LocalizationManager.GetTranslation($"GUI_COMPLETED_PERSONAL_QUEST_PROGRESS_TIP_{UIInfoTools.Instance.GetCharacterGender(character.CharacterYMLData.Model)}"), character.CharacterName.IsNOTNullOrEmpty() ? character.CharacterName : LocalizationManager.GetTranslation(character.CharacterYMLData.LocKey)));
			}
			else
			{
				closeButton.TextLanguageKey = "GUI_ANNOUNCE_RETIREMENT";
				header.SetHighlgihtText("GUI_PERSONAL_QUEST_COMPLETED");
				Singleton<HelpBox>.Instance.ShowTranslated(string.Format(LocalizationManager.GetTranslation($"GUI_COMPLETED_PERSONAL_QUEST_TIP_{UIInfoTools.Instance.GetCharacterGender(character.CharacterYMLData.Model)}"), character.CharacterName.IsNOTNullOrEmpty() ? character.CharacterName : LocalizationManager.GetTranslation(character.CharacterYMLData.LocKey)));
			}
			completedAnimation.Play();
			header.gameObject.SetActive(value: true);
			header.ShowHighlight();
		});
		character.PersonalQuest.LastProgressShown = character.PersonalQuest.PersonalQuestConditionState.CurrentProgress;
		window.Show();
		return promise;
	}

	private void OnFinishedCompleteAnimation()
	{
		closeButton.gameObject.SetActive(value: true);
		controllerArea?.Enable();
	}

	private void OnHidden()
	{
		if (promise != null)
		{
			Singleton<HelpBox>.Instance.Hide();
			completedAnimation.Stop();
			header.Hide();
			promise.Resolve();
			controllerArea?.Destroy();
		}
	}

	private void OnDisable()
	{
		completedAnimation.Stop();
	}
}
