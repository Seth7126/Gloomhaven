using ScenarioRuleLibrary;
using UnityEngine;

public class UIItemLocalTooltip : UILocalTooltip
{
	[Header("Item Card")]
	[SerializeField]
	private RectTransform cardHolder;

	[SerializeField]
	private bool showModifiersLeft;

	private ItemCardUI m_ItemCardUI;

	private void DecorateItem(CItem item, CItem.EItemSlotState? state = null)
	{
		if (m_ItemCardUI == null || m_ItemCardUI.CardName != item.Name)
		{
			if (m_ItemCardUI != null)
			{
				ObjectPool.RecycleCard(m_ItemCardUI.CardID, ObjectPool.ECardType.Item, m_ItemCardUI.gameObject);
			}
			m_ItemCardUI = ObjectPool.SpawnCard(item.ID, ObjectPool.ECardType.Item, cardHolder, resetLocalScale: true, resetToMiddle: true).GetComponent<ItemCardUI>();
			m_ItemCardUI.item = item;
			m_ItemCardUI.EnableShowModifiers(enable: true, showModifiersLeft);
		}
		if (state.HasValue)
		{
			m_ItemCardUI.UpdateState(state.Value, force: true);
		}
	}

	public override void SetTarget(RectTransform newTarget)
	{
		base.transform.SetParent(newTarget, worldPositionStays: false);
		base.SetTarget(newTarget);
	}

	private void OnDisable()
	{
		if (m_ItemCardUI != null)
		{
			ObjectPool.RecycleCard(m_ItemCardUI.CardID, ObjectPool.ECardType.Item, m_ItemCardUI.gameObject);
			m_ItemCardUI = null;
		}
	}

	public override void Hide()
	{
		base.Hide();
		if (m_ItemCardUI != null)
		{
			ObjectPool.RecycleCard(m_ItemCardUI.CardID, ObjectPool.ECardType.Item, m_ItemCardUI.gameObject);
			m_ItemCardUI = null;
		}
	}

	protected override void SetPosition(RectTransform targetPosition)
	{
		if (base.IsShown)
		{
			(base.transform as RectTransform).anchoredPosition = Vector2.zero;
		}
	}

	public void Show(CItem item, RectTransform target)
	{
		DecorateItem(item);
		SetTarget(target);
		Show();
	}

	public void Show(CItem item, RectTransform target, Vector2 offset)
	{
		DecorateItem(item);
		SetTarget(target, offset);
		Show();
	}
}
