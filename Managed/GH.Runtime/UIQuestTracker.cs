using System;
using MapRuleLibrary.MapState;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIQuestTracker : MonoBehaviour
{
	[SerializeField]
	private Image questIcon;

	[SerializeField]
	private RectTransform pointer;

	[SerializeField]
	private float offsetAngle;

	[SerializeField]
	private Image highlightQuestIcon;

	private Action<CQuestState> onClicked;

	private CQuestState quest;

	private UIWindow window;

	private void Awake()
	{
		window = GetComponent<UIWindow>();
	}

	public void SetQuest(CQuestState quest, Action<CQuestState> onClicked)
	{
		this.quest = quest;
		this.onClicked = onClicked;
		questIcon.sprite = UIInfoTools.Instance.GetQuestMarkerSprite(quest);
		highlightQuestIcon.sprite = UIInfoTools.Instance.GetQuestMarkerHighlightSprite(quest);
	}

	public void Select()
	{
		onClicked?.Invoke(quest);
	}

	public void RefreshPointTo(Vector3 position)
	{
		Vector3 vector = position - pointer.transform.position;
		float angle = Mathf.Atan2(vector.y, vector.x) * 57.29578f + offsetAngle;
		pointer.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
	}

	public void Hide()
	{
		window.Hide();
	}

	public void Show()
	{
		window.Show();
	}
}
