using UnityEngine;
using UnityEngine.UI;

public class CarouselCard : MonoBehaviour
{
	public int ID;

	public Text ID_Text;

	private float spaceBetweenCards = 300f;

	private float retractedScale = 1f;

	private float enlargedScale = 1.5f;

	private float lastDistanceToCenter;

	public void Start()
	{
	}

	public void UpdateText()
	{
		ID_Text.text = ID.ToString();
	}

	public float getXRelative()
	{
		return (float)ID * spaceBetweenCards;
	}

	public float getAnchoredX()
	{
		return base.gameObject.GetComponent<RectTransform>().anchoredPosition.x;
	}

	public float getPositionX()
	{
		return base.gameObject.GetComponent<RectTransform>().position.x;
	}

	public float GetDistanceTo(float x, bool absolute = true)
	{
		if (absolute)
		{
			return Mathf.Abs(getPositionX() - x);
		}
		return getPositionX() - x;
	}

	public void UpdateBasedOnDistnace(float center)
	{
		float distanceTo = GetDistanceTo(center);
		float num = Mathf.Lerp(enlargedScale, retractedScale, Mathf.Clamp01(distanceTo / 300f));
		base.transform.localScale = new Vector3(num, num, num);
	}

	public float GetLastDistance()
	{
		return lastDistanceToCenter;
	}

	public void SetLastDistance(float value)
	{
		lastDistanceToCenter = value;
	}

	public void SetSpaceBetweenCards(float value)
	{
		spaceBetweenCards = value;
	}
}
