using Assets.Script.GUI.Quest;
using MapRuleLibrary.MapState;
using UnityEngine;
using UnityEngine.UI;

public class UIQuestMapMarker : UIMapMarker
{
	[SerializeField]
	private Image questType;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private GameObject incompleteMask;

	[SerializeField]
	private float lockedOpacity;

	[SerializeField]
	private float incompletedOpacity;

	[SerializeField]
	private GUIAnimator showAnimator;

	private IQuest quest;

	public bool Focused { get; private set; }

	public void SetQuest(CQuestState quest)
	{
		SetQuest(new Quest(quest));
	}

	public void SetQuest(IQuest quest)
	{
		this.quest = quest;
		RefreshState();
	}

	public void RefreshState()
	{
		IRequirementCheckResult requirementCheckResult = quest.CheckRequirements();
		bool flag = requirementCheckResult.IsUnlocked() || Singleton<MapChoreographer>.Instance.ShowAllScenariosMode;
		if (flag)
		{
			incompleteMask.SetActive(value: false);
			canvasGroup.alpha = 1f;
		}
		else if (requirementCheckResult.IsOnlyMissingCharacters())
		{
			canvasGroup.alpha = incompletedOpacity;
			incompleteMask.SetActive(value: false);
		}
		else
		{
			incompleteMask.SetActive(value: true);
			canvasGroup.alpha = lockedOpacity;
		}
		questType.material = (flag ? null : UIInfoTools.Instance.greyedOutMaterial);
		questType.sprite = quest.Icon;
	}

	public void Focus(bool focus)
	{
		Focused = focus;
		canvasGroup.ignoreParentGroups = focus;
	}

	public override void Show()
	{
		Show(instant: true);
	}

	public void Show(bool instant)
	{
		Focus(focus: false);
		base.Show();
		showAnimator.Stop();
		if (instant)
		{
			showAnimator.GoToFinishState();
		}
		else
		{
			showAnimator.Play();
		}
	}

	public override void Hide()
	{
		showAnimator.Stop();
		base.Hide();
		Focus(focus: false);
	}

	private void OnDisable()
	{
		showAnimator.Stop();
	}
}
