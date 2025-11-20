using UnityEngine;
using UnityEngine.UI;

namespace Script.GUI.Cards_Hand;

public class FullCardScrollWrapper : MonoBehaviour
{
	[SerializeField]
	private ScrollRect _rect;

	public void ScrollToRect(RectTransform rectTransform)
	{
		_rect.ScrollToFit(rectTransform);
	}
}
