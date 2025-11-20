using UnityEngine;
using UnityEngine.Events;

namespace Script.GUI.Cards_Hand;

public class FullCardNavigationTranslator : MonoBehaviour
{
	[SerializeField]
	private UnityEvent<RectTransform> _enterEvent;

	[SerializeField]
	private UnityEvent _exitEvent;

	public void Enter(RectTransform rectTransform)
	{
		_enterEvent?.Invoke(rectTransform);
	}

	public void Exit()
	{
		_exitEvent?.Invoke();
	}
}
