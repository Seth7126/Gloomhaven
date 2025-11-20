using System.Collections.Generic;
using AsmodeeNet.Utils.Extensions;
using GLOOM;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIPartyItemInventoryTooltip : MonoBehaviour
{
	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private RectTransform cardHolder;

	[SerializeField]
	private TextMeshProUGUI itemBoundText;

	[SerializeField]
	private GameObject boundInformation;

	[SerializeField]
	private GameObject informationPanel;

	[SerializeField]
	private TextMeshProUGUI informationText;

	[SerializeField]
	private Image informationSlotIcon;

	[SerializeField]
	private bool showNodifiersLeft;

	[SerializeField]
	private bool trackScreenBound;

	[ConditionalField("trackScreenBound", null, true)]
	[SerializeField]
	private float offsetScreen = 20f;

	private List<IPartyInventoryTooltipHandler> _partyInventoryTooltipHandlers;

	private ItemCardUI m_ItemCardUI;

	private bool _isHighlighted;

	public bool TooltipShown
	{
		get
		{
			if (m_ItemCardUI != null)
			{
				return m_ItemCardUI.AllHintsCardTooltip.TooltipShown;
			}
			return false;
		}
	}

	public bool IsShown
	{
		get
		{
			if (base.gameObject.activeSelf)
			{
				if (!(window == null))
				{
					return window.IsOpen;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsHighlighted => _isHighlighted;

	private void Awake()
	{
		InitPartyInventoryTooltipsHandles();
	}

	private void InitPartyInventoryTooltipsHandles()
	{
		if (_partyInventoryTooltipHandlers == null)
		{
			_partyInventoryTooltipHandlers = new List<IPartyInventoryTooltipHandler>();
			GetComponentsInChildren(includeInactive: true, _partyInventoryTooltipHandlers);
		}
	}

	private void LateUpdate()
	{
		base.transform.position += (base.transform as RectTransform).DeltaWorldPositionToFitTheScreen(UIManager.Instance.UICamera, offsetScreen);
	}

	private void OnDisable()
	{
		if (m_ItemCardUI != null)
		{
			ObjectPool.RecycleCard(m_ItemCardUI.CardID, ObjectPool.ECardType.Item, m_ItemCardUI.gameObject);
			m_ItemCardUI = null;
			_isHighlighted = false;
		}
	}

	public void ToggleTextTooltip()
	{
		if (!(m_ItemCardUI == null))
		{
			if (!m_ItemCardUI.AllHintsCardTooltip.TooltipShown)
			{
				ShowTextTooltip();
			}
			else
			{
				HideTextTooltip();
			}
		}
	}

	public void HideItemTextTooltip()
	{
		if (!(m_ItemCardUI == null) && m_ItemCardUI.AllHintsCardTooltip.TooltipShown)
		{
			HideTextTooltip();
		}
	}

	private void ShowTextTooltip()
	{
		if (IsShown)
		{
			m_ItemCardUI.AllHintsCardTooltip.OnPointerEnter(new PointerEventData(EventSystem.current));
		}
	}

	public bool CanShowTooltip()
	{
		if (m_ItemCardUI == null)
		{
			return false;
		}
		return m_ItemCardUI.AllHintsCardTooltip.CanBeShown;
	}

	private void HideTextTooltip()
	{
		if (IsShown)
		{
			m_ItemCardUI.AllHintsCardTooltip.OnPointerExit(null);
		}
	}

	private void Build(CItem item, RectTransform target, Vector2 offset, CMapCharacter boundTo = null, string information = null, CItem.EItemSlotState? state = null, IShopItemService shopItemService = null)
	{
		if (m_ItemCardUI == null || m_ItemCardUI.CardName != item.Name)
		{
			if (m_ItemCardUI != null)
			{
				ObjectPool.RecycleCard(m_ItemCardUI.CardID, ObjectPool.ECardType.Item, m_ItemCardUI.gameObject);
			}
			m_ItemCardUI = ObjectPool.SpawnCard(item.ID, ObjectPool.ECardType.Item, cardHolder, resetLocalScale: true, resetToMiddle: true).GetComponent<ItemCardUI>();
			m_ItemCardUI.item = item;
			m_ItemCardUI.EnableShowModifiers(enable: true, showNodifiersLeft);
			m_ItemCardUI.BuildTooltipLayout(showNodifiersLeft);
		}
		if (boundTo == null)
		{
			boundInformation.SetActive(value: false);
		}
		else
		{
			informationSlotIcon.sprite = UIInfoTools.Instance.GetItemSlotIcon(item.YMLData.Slot.ToString());
			itemBoundText.text = string.Format(LocalizationManager.GetTranslation("GUI_ITEM_BOUND_TO"), UIInfoTools.Instance.GetCharacterColor(boundTo.CharacterYMLData.Model, boundTo.CharacterYMLData.CustomCharacterConfig).ToHex(), boundTo.CharacterName.IsNOTNullOrEmpty() ? boundTo.CharacterName : LocalizationManager.GetTranslation(boundTo.CharacterYMLData.LocKey), $"AA_{boundTo.CharacterYMLData.Model}");
			boundInformation.SetActive(value: true);
		}
		if (information == null)
		{
			informationPanel.SetActive(value: false);
		}
		else
		{
			informationText.text = information;
			informationPanel.SetActive(value: true);
		}
		base.transform.SetParent(target, worldPositionStays: false);
		InitPartyInventoryTooltipsHandles();
		_partyInventoryTooltipHandlers.ForEach(delegate(IPartyInventoryTooltipHandler x)
		{
			x.OnBuildStarted(item, shopItemService);
		});
		RefreshPosition(offset);
		if (window != null)
		{
			window.Show();
		}
		else
		{
			base.gameObject.SetActive(value: true);
		}
		base.enabled = trackScreenBound;
		if (state.HasValue)
		{
			m_ItemCardUI.UpdateState(state.Value, force: true);
		}
	}

	public void Show(CItem item, RectTransform target, CMapCharacter boundTo = null, string information = null, CItem.EItemSlotState? state = null, IShopItemService shopItemService = null)
	{
		Build(item, target, Vector2.zero, boundTo, information, state, shopItemService);
	}

	public void Show(CItem item, RectTransform target, Vector2 offset)
	{
		Build(item, target, offset);
	}

	public void Hide()
	{
		if (window != null)
		{
			window.Hide();
			base.enabled = false;
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
		_partyInventoryTooltipHandlers?.ForEach(delegate(IPartyInventoryTooltipHandler x)
		{
			x.OnHide();
		});
	}

	public void RefreshPosition(Vector2 offset)
	{
		(base.transform as RectTransform).anchoredPosition = offset;
		base.transform.position += (base.transform as RectTransform).DeltaWorldPositionToFitTheScreen(UIManager.Instance.UICamera, offsetScreen);
	}

	public void Highlight(bool state)
	{
		m_ItemCardUI.HighLight(state);
		_isHighlighted = state;
	}
}
