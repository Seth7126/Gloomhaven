using System;
using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core.UI;

internal class DropdownOptionViewModel<TSelection>
{
	public virtual GUIContent Text { get; set; }

	public Func<bool> IsEnabled { get; set; }

	public Func<bool> IsSelected { get; set; }

	public TSelection Selection { get; set; }

	public DropdownOptionViewModel(string text, Func<bool> isSelected, Func<bool> isEnabled, TSelection selection)
	{
		Text = GUIUtil.CreateContent(text);
		IsSelected = isSelected;
		IsEnabled = isEnabled;
		Selection = selection;
	}
}
