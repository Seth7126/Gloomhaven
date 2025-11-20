using TMPro;
using UnityEngine;

public class UIDistributeGoldActorInfo : UIDistributePointsActorInfo
{
	[SerializeField]
	private TextMeshProUGUI goldText;

	[SerializeField]
	private Color increaseGoldColor;

	[SerializeField]
	private Color decreaseGoldColor;

	[SerializeField]
	private Color defaultGoldColor;

	[SerializeField]
	[TextArea]
	private string format = "<sprite name=\"Gold_Icon_White\" color=#FCC951FF>{0}";

	private DistributeRewardService.DistributeMapActor m_Actor;

	public override void Display(IDistributePointsActor actor)
	{
		m_Actor = actor as DistributeRewardService.DistributeMapActor;
		base.gameObject.SetActive(m_Actor.ShowGold);
	}

	public override void RefreshAssignedPoints(int currentPoints, int assignedPoints)
	{
		if (m_Actor.ShowGold)
		{
			int num = m_Actor.Character.CharacterGold + (m_Actor.IsNegative ? (-assignedPoints) : assignedPoints);
			goldText.text = (format.IsNullOrEmpty() ? num.ToString() : string.Format(format, num));
			goldText.color = ((assignedPoints == 0) ? defaultGoldColor : (m_Actor.IsNegative ? decreaseGoldColor : increaseGoldColor));
		}
	}
}
