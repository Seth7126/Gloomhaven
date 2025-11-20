using AsmodeeNet.Foundation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

internal class FullCardHandNavigation : MonoBehaviour, IMoveHandler, IEventSystemHandler
{
	[SerializeField]
	private ExtendedScrollRect m_ScrollRect;

	[SerializeField]
	private GridLayoutGroup m_GridLayoutGroup;

	[SerializeField]
	private bool navigationHorizontal = true;

	[SerializeField]
	private bool navigationVertical = true;

	private int selected = -1;

	private GameObject currentHovered;

	private bool isNavigationEnabled;

	public void EnableNavigation()
	{
		isNavigationEnabled = true;
		if (base.gameObject.activeInHierarchy)
		{
			SetCurrent(0);
			EventSystem.current.SetSelectedGameObject(base.gameObject);
		}
	}

	public void DisableNavigation()
	{
		if (isNavigationEnabled)
		{
			isNavigationEnabled = false;
			SetCurrent(-1);
			if (EventSystem.current.currentSelectedGameObject == base.gameObject)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
		}
	}

	protected void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			DisableNavigation();
		}
	}

	protected void OnEnable()
	{
		if (isNavigationEnabled)
		{
			EnableNavigation();
		}
	}

	public void OnMove(AxisEventData eventData)
	{
		Vector2 cellCountPerAxis = m_GridLayoutGroup.GetCellCountPerAxis(base.transform.childCount);
		int num = (int)cellCountPerAxis.x;
		int num2 = (int)cellCountPerAxis.y;
		int num3 = selected % num;
		int num4 = selected / num;
		switch (eventData.moveDir)
		{
		case MoveDirection.Right:
			if (navigationHorizontal)
			{
				SetCurrent((num3 + 1) % num + num4 * num);
			}
			break;
		case MoveDirection.Up:
			if (navigationVertical)
			{
				if (selected < num)
				{
					SetCurrent(num * (num2 - 1) + num3);
				}
				else
				{
					SetCurrent(selected - num);
				}
			}
			break;
		case MoveDirection.Left:
			if (navigationHorizontal)
			{
				SetCurrent(((num3 - 1 < 0) ? (num - 1) : (num3 - 1)) + num4 * num);
			}
			break;
		case MoveDirection.Down:
			if (navigationVertical)
			{
				SetCurrent((selected + num > base.transform.childCount) ? num3 : (selected + num));
			}
			break;
		}
	}

	private void SetCurrent(int newSelected)
	{
		if (selected == newSelected)
		{
			return;
		}
		if (currentHovered != null)
		{
			ExecuteEvents.Execute(currentHovered, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);
			currentHovered = null;
		}
		selected = newSelected;
		if (newSelected >= 0 && base.transform.childCount > newSelected)
		{
			RectTransform rectTransform = (RectTransform)base.transform.GetChild(newSelected);
			currentHovered = rectTransform.GetComponent<AbilityCardUI>().fullAbilityCard.gameObject;
			ExecuteEvents.Execute(currentHovered, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);
			if (newSelected == 0)
			{
				m_ScrollRect.ScrollToTop();
			}
			else
			{
				m_ScrollRect.ScrollVerticallyToTop(rectTransform, m_GridLayoutGroup.padding.top);
			}
		}
	}
}
