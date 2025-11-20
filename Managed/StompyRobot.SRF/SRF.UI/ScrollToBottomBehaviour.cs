using UnityEngine;
using UnityEngine.UI;

namespace SRF.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
[AddComponentMenu("SRF/UI/Scroll To Bottom Behaviour")]
public class ScrollToBottomBehaviour : MonoBehaviour
{
	[SerializeField]
	private ScrollRect _scrollRect;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private bool _scrollToTop;

	public void Start()
	{
		if (_scrollRect == null)
		{
			Debug.LogError("[ScrollToBottomBehaviour] ScrollRect not set");
			return;
		}
		if (_canvasGroup == null)
		{
			Debug.LogError("[ScrollToBottomBehaviour] CanvasGroup not set");
			return;
		}
		_scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
		Refresh();
	}

	private void OnEnable()
	{
		Refresh();
	}

	public void Trigger()
	{
		if (_scrollToTop)
		{
			_scrollRect.normalizedPosition = new Vector2(0f, 1f);
		}
		else
		{
			_scrollRect.normalizedPosition = new Vector2(0f, 0f);
		}
	}

	private void OnScrollRectValueChanged(Vector2 position)
	{
		Refresh();
	}

	private void Refresh()
	{
		if (!(_scrollRect == null))
		{
			Vector2 normalizedPosition = _scrollRect.normalizedPosition;
			if (normalizedPosition.y < 0.001f || (_scrollToTop && normalizedPosition.y >= 0.999f))
			{
				SetVisible(truth: false);
			}
			else
			{
				SetVisible(truth: true);
			}
		}
	}

	private void SetVisible(bool truth)
	{
		if (truth)
		{
			_canvasGroup.alpha = 1f;
			_canvasGroup.interactable = true;
			_canvasGroup.blocksRaycasts = true;
		}
		else
		{
			_canvasGroup.alpha = 0f;
			_canvasGroup.interactable = false;
			_canvasGroup.blocksRaycasts = false;
		}
	}
}
