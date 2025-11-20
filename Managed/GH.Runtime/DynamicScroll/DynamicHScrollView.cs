using UnityEngine;

namespace DynamicScroll;

public class DynamicHScrollView : DynamicScrollView
{
	protected override float ContentAnchoredPosition
	{
		get
		{
			return _contentRect.anchoredPosition.x;
		}
		set
		{
			_contentRect.anchoredPosition = new Vector2(value, _contentRect.anchoredPosition.y);
		}
	}

	protected override float ContentSize => _contentRect.rect.width;

	protected override float ViewportSize => _viewportRect.rect.width;

	protected override float ItemSize => _itemPrototype.rect.width + _extraSeparatorSize;

	protected override void Awake()
	{
		base.Awake();
		_direction = Direction.Horizontal;
	}
}
