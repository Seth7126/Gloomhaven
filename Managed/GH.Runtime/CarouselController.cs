using System.Collections.Generic;
using UnityEngine;

public class CarouselController : MonoBehaviour
{
	public RectTransform center;

	public RectTransform scrollPanel;

	public CarouselCard carouselCardPrefab;

	private List<CarouselCard> cards = new List<CarouselCard>();

	private float[] distnaces;

	private const float c_SnapSpeed = 0.1f;

	private bool dragging;

	private int cardDistance;

	private const float c_SpaceBetweenCards = 300f;

	private void Start()
	{
		for (int i = 0; i < 15; i++)
		{
			CarouselCard carouselCard = Object.Instantiate(carouselCardPrefab, scrollPanel.transform, worldPositionStays: false);
			carouselCard.ID = i;
			carouselCard.GetComponent<RectTransform>().localPosition = new Vector3(300f * (float)i, 0f, 0f);
			carouselCard.SetSpaceBetweenCards(300f);
			carouselCard.UpdateText();
			cards.Add(carouselCard);
		}
		BlinkToViewCardAtID(4);
	}

	private void Awake()
	{
	}

	private void Update()
	{
		float num = float.MaxValue;
		CarouselCard carouselCard = null;
		float num2 = 0f;
		for (int i = 0; i < cards.Count; i++)
		{
			num2 = cards[i].GetDistanceTo(center.GetComponent<RectTransform>().position.x);
			cards[i].SetLastDistance(num2);
			if (num2 < num)
			{
				num = num2;
				carouselCard = cards[i];
			}
			if (cards[i].GetDistanceTo(center.GetComponent<RectTransform>().position.x, absolute: false) < (float)(-(300 * cards.Count) / 2))
			{
				float x = cards[i].gameObject.GetComponent<RectTransform>().anchoredPosition.x;
				float y = cards[i].gameObject.GetComponent<RectTransform>().anchoredPosition.y;
				Vector2 anchoredPosition = new Vector2(x + (float)(300 * cards.Count), y);
				cards[i].gameObject.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
			}
			else if (cards[i].GetDistanceTo(center.GetComponent<RectTransform>().position.x) > (float)(300 * cards.Count / 2))
			{
				float x2 = cards[i].gameObject.GetComponent<RectTransform>().anchoredPosition.x;
				float y2 = cards[i].gameObject.GetComponent<RectTransform>().anchoredPosition.y;
				Vector2 anchoredPosition2 = new Vector2(x2 - (float)(300 * cards.Count), y2);
				cards[i].gameObject.GetComponent<RectTransform>().anchoredPosition = anchoredPosition2;
			}
		}
		SortCardsByDistance();
		for (int j = 0; j < cards.Count; j++)
		{
			cards[j].UpdateBasedOnDistnace(center.GetComponent<RectTransform>().position.x);
		}
		if (!dragging && carouselCard != null)
		{
			LerpCardToMiddle(carouselCard);
		}
	}

	private void LerpCardToMiddle(CarouselCard card)
	{
		float x = Mathf.Lerp(scrollPanel.anchoredPosition.x, 0f - card.getAnchoredX(), 0.1f);
		scrollPanel.anchoredPosition = new Vector2(x, scrollPanel.anchoredPosition.y);
	}

	private void BlinkToViewCardAtID(int id)
	{
		scrollPanel.anchoredPosition = new Vector2(0f - cards[id].getXRelative(), scrollPanel.anchoredPosition.y);
	}

	public void BeginDrag()
	{
		dragging = true;
	}

	public void EndDrag()
	{
		dragging = false;
	}

	public void SortCardsByDistance()
	{
		List<CarouselCard> list = new List<CarouselCard>(cards);
		list.Sort((CarouselCard a, CarouselCard b) => a.GetLastDistance().CompareTo(b.GetLastDistance()));
		for (int num = 0; num < list.Count; num++)
		{
			list[num].GetComponent<RectTransform>().SetAsFirstSibling();
		}
	}
}
