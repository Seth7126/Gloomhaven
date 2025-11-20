using System;
using System.Collections;
using System.Collections.Generic;
using Chronos;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.YML.Message;
using MapRuleLibrary.YML.Quest;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;
using UnityEngine;
using UnityEngine.UI;

public class UILoadoutQuestWindow : MonoBehaviour
{
	[SerializeField]
	private StoryImageViewer imagePaper;

	[SerializeField]
	private AspectRatioFitterTransition paperFitter;

	[SerializeField]
	private float expandPaperDuration = 0.5f;

	[SerializeField]
	private LeanTweenType expandPaperEase;

	[SerializeField]
	private Image expandMask;

	[SerializeField]
	private float expandMaskFrom = 1.1f;

	[SerializeField]
	private float delayToShowText = 0.4f;

	[SerializeField]
	private CanvasGroup _hotkeysCanvas;

	private Action onFinishIntroduction;

	private LTDescr finishAnim;

	private void OnDisable()
	{
		imagePaper.Hide();
		StopAnimations();
	}

	public void Show(CQuest quest, Action onFinishIntroduction)
	{
		this.onFinishIntroduction = onFinishIntroduction;
		paperFitter.transitionPercent = 0f;
		expandMask.SetAlpha(0f);
		HideHotkeys();
		imagePaper.LoadImages(new List<string> { quest.LoadoutImageId }, delegate
		{
			ShowLoadoutBackground(quest);
		});
	}

	private void ShowLoadoutBackground(CQuest quest)
	{
		imagePaper.Show(quest.LoadoutImageId, ShowHotkeys);
		StartCoroutine(ShowIntroductionText(quest));
	}

	private void ShowHotkeys()
	{
		if (_hotkeysCanvas != null)
		{
			_hotkeysCanvas.alpha = 1f;
		}
	}

	private void HideHotkeys()
	{
		if (_hotkeysCanvas == null)
		{
			_hotkeysCanvas = Hotkeys.Instance.GetComponent<CanvasGroup>();
		}
		if (_hotkeysCanvas != null)
		{
			_hotkeysCanvas.alpha = 0f;
		}
	}

	private IEnumerator ShowIntroductionText(CQuest quest)
	{
		if (delayToShowText > 0f)
		{
			yield return Timekeeper.instance.WaitForSeconds(delayToShowText);
		}
		Singleton<MapStoryController>.Instance.Show(EMapMessageTrigger.Loadout, new MapStoryController.MapDialogInfo(new List<DialogLineDTO>
		{
			new DialogLineDTO(quest.LocalisedIntroKey, "Narrator", EExpression.Default, null, AdventureState.MapState.IsCampaign ? LocalizationManager.GetTranslation(quest.LocalisedNameKey) : null, (quest.LoadoutAudioId == null && AdventureState.MapState.IsCampaign) ? quest.LocalisedIntroKey : quest.LoadoutAudioId)
		}, FinishIntroduction, hideOtherUI: false));
	}

	private void FinishIntroduction()
	{
		finishAnim = LeanTween.value(base.gameObject, delegate(float val)
		{
			paperFitter.transitionPercent = val;
			expandMask.SetAlpha(val);
			float num = expandMaskFrom + (1f - expandMaskFrom) * val;
			expandMask.transform.localScale = new Vector3(num, num, 1f);
		}, 0f, 1f, expandPaperDuration).setEase(expandPaperEase).setOnComplete((Action)delegate
		{
			finishAnim = null;
			onFinishIntroduction?.Invoke();
		});
	}

	private void StopAnimations()
	{
		if (finishAnim != null)
		{
			LeanTween.cancel(finishAnim.id);
		}
		finishAnim = null;
	}
}
