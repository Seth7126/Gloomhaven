using System;
using System.Collections;
using System.Collections.Generic;
using AsmodeeNet.Foundation;
using SM.Gamepad;
using SM.Utils;
using Script.GUI.SMNavigation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DynamicScroll;

[RequireComponent(typeof(ScrollRect))]
public abstract class DynamicScrollView : UIBehaviour
{
	[SerializeField]
	private UiNavigationGroup _uiNavigationGroup;

	public int TotalItemCount;

	public RectTransform _itemPrototype;

	protected Direction _direction;

	protected LinkedList<RectTransform> _startContainers = new LinkedList<RectTransform>();

	protected LinkedList<RectTransform> _currentContainers = new LinkedList<RectTransform>();

	protected float _prevAnchoredPosition;

	protected float _extraSeparatorSize;

	protected int _nextInsertItemNo;

	protected int _prevTotalItemCount;

	protected ScrollRect _scrollRect;

	protected RectTransform _viewportRect;

	protected RectTransform _contentRect;

	private int _currentItemIndex;

	protected abstract float ContentAnchoredPosition { get; set; }

	protected abstract float ContentSize { get; }

	protected abstract float ViewportSize { get; }

	protected abstract float ItemSize { get; }

	public event Action OnRefreshEvent;

	protected override void Awake()
	{
		if (_itemPrototype == null)
		{
			LogUtils.LogError("Missed scroll item prototype!");
			return;
		}
		base.Awake();
		_scrollRect = GetComponent<ScrollRect>();
		_viewportRect = _scrollRect.viewport;
		_contentRect = _scrollRect.content;
	}

	protected override void Start()
	{
		base.Start();
		StartCoroutine(OnSeedData());
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (!CoreApplication.IsQuitting)
		{
			Deactivate();
			Singleton<UINavigation>.Instance.NavigationManager.OnTrySelectPreviousEvent -= MoveToCurrent;
			_uiNavigationGroup.AfterNavigationMoveEvent -= ChangeCurrent;
		}
	}

	public void Activate(int count)
	{
		TotalItemCount = count;
		_currentItemIndex = 0;
		_currentContainers = new LinkedList<RectTransform>(_startContainers);
		Singleton<UINavigation>.Instance.NavigationManager.OnTrySelectPreviousEvent -= MoveToCurrent;
		Singleton<UINavigation>.Instance.NavigationManager.OnTrySelectPreviousEvent += MoveToCurrent;
		_uiNavigationGroup.AfterNavigationMoveEvent -= ChangeCurrent;
		_uiNavigationGroup.AfterNavigationMoveEvent += ChangeCurrent;
	}

	private void ChangeCurrent(UINavigationDirection direction)
	{
		RectTransform rectTransform = EventSystem.current.currentSelectedGameObject.transform as RectTransform;
		if (direction == UINavigationDirection.Down)
		{
			_currentItemIndex++;
		}
		if (direction == UINavigationDirection.Up)
		{
			_currentItemIndex--;
		}
		if (rectTransform.anchoredPosition.y < 0f - (ViewportSize - ContentAnchoredPosition - ItemSize))
		{
			ScrollByItemIndexToBottom(_currentItemIndex);
		}
		else if (rectTransform.anchoredPosition.y > ContentAnchoredPosition)
		{
			ScrollByItemIndexToTop(_currentItemIndex);
		}
		LogUtils.LogError("Current index: " + _currentItemIndex);
	}

	private void MoveToCurrent()
	{
		if (_currentItemIndex < TotalItemCount / 2)
		{
			ScrollByItemIndexToTop(_currentItemIndex);
		}
		else
		{
			ScrollByItemIndexToBottom(_currentItemIndex);
		}
	}

	public void Deactivate()
	{
		TotalItemCount = 0;
		if (Singleton<UINavigation>.Instance != null)
		{
			Singleton<UINavigation>.Instance.NavigationManager.OnTrySelectPreviousEvent -= MoveToCurrent;
		}
		_uiNavigationGroup.AfterNavigationMoveEvent -= ChangeCurrent;
	}

	private void Update()
	{
		if (TotalItemCount != _prevTotalItemCount)
		{
			_prevTotalItemCount = TotalItemCount;
			ResizeContent();
			Refresh();
		}
		while (ContentAnchoredPosition - _prevAnchoredPosition < (0f - ItemSize) * 2f)
		{
			if (_nextInsertItemNo + _currentContainers.Count >= TotalItemCount)
			{
				return;
			}
			_prevAnchoredPosition -= ItemSize;
			LinkedListNode<RectTransform> first = _currentContainers.First;
			if (first == null)
			{
				break;
			}
			RectTransform value = first.Value;
			_currentContainers.RemoveFirst();
			_currentContainers.AddLast(value);
			float num = ItemSize * (float)(_currentContainers.Count + _nextInsertItemNo);
			value.anchoredPosition = ((_direction == Direction.Vertical) ? new Vector2(0f, 0f - num) : new Vector2(num, 0f));
			UpdateItem(_currentContainers.Count + _nextInsertItemNo, value.gameObject);
			_nextInsertItemNo++;
		}
		while (ContentAnchoredPosition - _prevAnchoredPosition >= 0f && _nextInsertItemNo > 0)
		{
			_prevAnchoredPosition += ItemSize;
			LinkedListNode<RectTransform> last = _currentContainers.Last;
			if (last != null)
			{
				RectTransform value2 = last.Value;
				_currentContainers.RemoveLast();
				_currentContainers.AddFirst(value2);
				_nextInsertItemNo--;
				float num2 = ItemSize * (float)_nextInsertItemNo;
				value2.anchoredPosition = ((_direction == Direction.Vertical) ? new Vector2(0f, 0f - num2) : new Vector2(num2, 0f));
				UpdateItem(_nextInsertItemNo, value2.gameObject);
				continue;
			}
			break;
		}
	}

	public void ScrollByItemIndexToTop(int itemIndex)
	{
		float num = ContentSize / (float)TotalItemCount * (float)itemIndex;
		ContentAnchoredPosition = 0f - num;
	}

	public void ScrollByItemIndexToBottom(int itemIndex)
	{
		float num = ContentSize / (float)TotalItemCount;
		float num2 = num * (float)(itemIndex - (int)(ViewportSize / ItemSize)) + 0.5f * num;
		ContentAnchoredPosition = 0f - num2;
	}

	public void Refresh()
	{
		int num = 0;
		if (ContentAnchoredPosition != 0f)
		{
			num = (int)((0f - ContentAnchoredPosition) / ItemSize);
		}
		foreach (RectTransform currentContainer in _currentContainers)
		{
			float num2 = ItemSize * (float)num;
			currentContainer.anchoredPosition = ((_direction == Direction.Vertical) ? new Vector2(0f, 0f - num2) : new Vector2(num2, 0f));
			UpdateItem(num, currentContainer.gameObject);
			num++;
		}
		_nextInsertItemNo = num - _currentContainers.Count;
		_prevAnchoredPosition = (float)(int)(ContentAnchoredPosition / ItemSize) * ItemSize;
		this.OnRefreshEvent?.Invoke();
	}

	protected virtual IEnumerator OnSeedData()
	{
		yield return null;
		_itemPrototype.gameObject.SetActive(value: false);
		int num = (int)(ViewportSize / ItemSize) + 3;
		for (int i = 0; i < num; i++)
		{
			RectTransform rectTransform = UnityEngine.Object.Instantiate(_itemPrototype, _contentRect, worldPositionStays: false);
			rectTransform.name = i.ToString();
			rectTransform.anchoredPosition = ((_direction == Direction.Vertical) ? new Vector2(0f, (0f - ItemSize) * (float)i) : new Vector2(ItemSize * (float)i, 0f));
			_startContainers.AddLast(rectTransform);
			rectTransform.gameObject.SetActive(value: true);
			UpdateItem(i, rectTransform.gameObject);
		}
		ResizeContent();
		_currentContainers = new LinkedList<RectTransform>(_startContainers);
	}

	private void ResizeContent()
	{
		Vector2 size = _contentRect.rect.size;
		if (_direction == Direction.Vertical)
		{
			size.y = ItemSize * (float)TotalItemCount;
		}
		else
		{
			size.x = ItemSize * (float)TotalItemCount;
		}
		_contentRect.SetSize(size);
	}

	private void UpdateItem(int index, GameObject itemObj)
	{
		if (index < 0 || index >= TotalItemCount)
		{
			itemObj.SetActive(value: false);
			return;
		}
		itemObj.SetActive(value: true);
		if (itemObj.TryGetComponent<IDynamicScrollViewItem>(out var component))
		{
			component.OnUpdateItem(index, _currentItemIndex == index);
		}
	}
}
