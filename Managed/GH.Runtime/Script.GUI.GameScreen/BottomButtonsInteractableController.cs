using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Script.GUI.GameScreen;

public class BottomButtonsInteractableController : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private List<ButtonOnBlockingPanel> _interactableButtons = new List<ButtonOnBlockingPanel>();

	[SerializeField]
	private BaseButtons _baseButtons;

	private bool _interact = true;

	private void Awake()
	{
		foreach (ButtonOnBlockingPanel interactableButton in _interactableButtons)
		{
			interactableButton.TakeInteractableCallback(OnChangeInteract);
		}
		_baseButtons.ChangeCanvasVisible += OnChangeInteract;
	}

	private void OnDestroy()
	{
		if (_baseButtons != null)
		{
			_baseButtons.ChangeCanvasVisible -= OnChangeInteract;
		}
	}

	private void OnChangeInteract(bool currentInteract)
	{
		if (_interact != currentInteract)
		{
			bool flag = _interactableButtons.Any((ButtonOnBlockingPanel button) => button.IsInteractable());
			bool flag2 = _canvasGroup.alpha > 0.5f && flag;
			_canvasGroup.blocksRaycasts = flag2;
			_interact = flag2;
		}
	}
}
