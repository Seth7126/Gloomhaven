using TMPro;
using UnityEngine;

public class UIDistributePointsText : UIDistributePointsCounter
{
	[SerializeField]
	private TextMeshProUGUI currentPointsText;

	[SerializeField]
	[TextArea]
	private string format;

	private int currentPoints;

	private int extendedPoints;

	public override void SetCurrentPoints(int currentPoints)
	{
		this.currentPoints = currentPoints;
		Refresh();
	}

	public override void SetExtendedPoints(int extendedPoints)
	{
		this.extendedPoints = extendedPoints;
		Refresh();
	}

	private void Refresh()
	{
		currentPointsText.text = (format.IsNOTNullOrEmpty() ? string.Format(format, currentPoints + extendedPoints) : (currentPoints + extendedPoints).ToString());
	}
}
