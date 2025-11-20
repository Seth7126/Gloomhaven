using System.Collections.Generic;
using AsmodeeNet.Foundation;
using UnityEngine;
using UnityEngine.Events;

namespace AsmodeeNet.UserInterface;

public class FocusableLayer : MonoBehaviour
{
	private const string _documentation = "<b>FocusableLayer</b> aggregates <b>Focusable</b> controls. It is usally used for the root node of a modal dialog box with the <b>modal</b> flag set to true.";

	public bool modal = true;

	public UnityEvent OnBackAction;

	private List<Focusable> _focusables = new List<Focusable>();

	public IList<Focusable> Focusables => _focusables.AsReadOnly();

	private void OnEnable()
	{
		CoreApplication.Instance.UINavigationManager.RegisterFocusableLayer(this);
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			CoreApplication.Instance.UINavigationManager.UnRegisterFocusableLayer(this);
		}
	}

	public void RegisterFocusable(Focusable focusable)
	{
		_focusables.Add(focusable);
	}

	public void UnRegisterFocusable(Focusable focusable)
	{
		_focusables.Remove(focusable);
	}
}
