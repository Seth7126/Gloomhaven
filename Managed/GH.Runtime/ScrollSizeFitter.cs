using DynamicScroll;
using UnityEngine;
using UnityEngine.UI;

public class ScrollSizeFitter : MonoBehaviour
{
	[SerializeField]
	private ScrollRect _scrollRect;

	[SerializeField]
	private bool _fitVertically;

	[SerializeField]
	private bool _fitHorizontally;

	private Vector2 _initialSize;

	private bool _initialized;

	private void Awake()
	{
		Initialize();
	}

	public void Initialize()
	{
		if (!_initialized)
		{
			_initialized = true;
			_initialSize = _scrollRect.GetComponent<RectTransform>().GetSize();
		}
	}

	public void Fit()
	{
		if (_initialized)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.GetComponent<RectTransform>());
			_scrollRect.ResizeToFitContent(_fitHorizontally, _fitVertically, _initialSize);
		}
	}
}
