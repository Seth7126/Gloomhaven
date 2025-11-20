using UnityEngine;

namespace DynamicScroll;

public class DynamicVScrollView : DynamicScrollView
{
	protected override float ContentAnchoredPosition
	{
		get
		{
			return 0f - _contentRect.anchoredPosition.y;
		}
		set
		{
			_contentRect.anchoredPosition = new Vector2(_contentRect.anchoredPosition.x, 0f - value);
		}
	}

	protected override float ContentSize => _contentRect.rect.height;

	protected override float ViewportSize => _viewportRect.rect.height;

	protected override float ItemSize => _itemPrototype.rect.height + _extraSeparatorSize;

	protected override void Awake()
	{
		base.Awake();
		_direction = Direction.Vertical;
	}
}
