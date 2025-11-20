using System.Collections.Generic;
using InControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BindingsMenu : MonoBehaviour
{
	public GameObject BindingButtonPrefab;

	private readonly List<BindingButton> bindingButtons = new List<BindingButton>();

	private VerticalLayoutGroup bindingsLayoutGroup;

	private CanvasGroup canvasGroup;

	private PlayerActionSet actionSet;

	private void Awake()
	{
		bindingsLayoutGroup = GetComponentInChildren<VerticalLayoutGroup>(includeInactive: true);
		canvasGroup = GetComponent<CanvasGroup>();
		base.gameObject.SetActive(value: false);
	}

	public void Show(PlayerActionSet actionSet)
	{
		this.actionSet = actionSet;
		if (base.gameObject.activeSelf)
		{
			Debug.LogWarningFormat("{0} is already showing.", base.gameObject.name);
			return;
		}
		CreateBindingButtons();
		base.gameObject.SetActive(value: true);
		if (bindingButtons.Count > 0)
		{
			GameObject gameObject = bindingButtons[0].gameObject;
			EventSystem.current.firstSelectedGameObject = gameObject;
			EventSystem.current.SetSelectedGameObject(gameObject);
		}
	}

	public void Hide()
	{
		if (!base.gameObject.activeSelf)
		{
			Debug.LogWarningFormat("{0} is already hidden.", base.gameObject.name);
		}
		else
		{
			DeleteBindingButtons();
			base.gameObject.SetActive(value: false);
		}
	}

	private void CreateBindingButtons()
	{
		foreach (PlayerAction action in actionSet.Actions)
		{
			GameObject obj = Object.Instantiate(BindingButtonPrefab, Vector3.zero, Quaternion.identity);
			obj.name = "Button - " + action.Name;
			obj.transform.SetParent(bindingsLayoutGroup.transform);
			BindingButton component = obj.GetComponent<BindingButton>();
			component.Setup(action, canvasGroup);
			bindingButtons.Add(component);
		}
	}

	private void DeleteBindingButtons()
	{
		foreach (BindingButton bindingButton in bindingButtons)
		{
			Object.Destroy(bindingButton.gameObject);
		}
		bindingButtons.Clear();
	}

	public void ResetBindings()
	{
		actionSet.Reset();
	}
}
