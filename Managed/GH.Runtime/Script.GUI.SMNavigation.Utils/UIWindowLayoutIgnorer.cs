using UnityEngine;
using UnityEngine.UI;

namespace Script.GUI.SMNavigation.Utils;

[RequireComponent(typeof(LayoutElement))]
public class UIWindowLayoutIgnorer : MonoBehaviour
{
	private LayoutElement _layout;

	private LayoutElement Layout => _layout ?? (_layout = GetComponent<LayoutElement>());

	private void Awake()
	{
		Layout.ignoreLayout = true;
	}

	public void OnShown()
	{
		Layout.ignoreLayout = false;
	}

	public void OnHidden()
	{
		Layout.ignoreLayout = true;
	}
}
