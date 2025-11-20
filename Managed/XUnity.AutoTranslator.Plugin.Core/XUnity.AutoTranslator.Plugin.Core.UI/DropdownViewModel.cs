using System;
using System.Collections.Generic;

namespace XUnity.AutoTranslator.Plugin.Core.UI;

internal class DropdownViewModel<TDropdownOptionViewModel, TSelection> where TDropdownOptionViewModel : DropdownOptionViewModel<TSelection> where TSelection : class
{
	private Action<TSelection> _onSelected;

	public TDropdownOptionViewModel CurrentSelection { get; set; }

	public List<TDropdownOptionViewModel> Options { get; set; }

	public string NoSelection { get; }

	public string NoSelectionTooltip { get; }

	public string Unselect { get; }

	public string UnselectTooltip { get; }

	public DropdownViewModel(string noSelection, string noSelectionTooltip, string unselect, string unselectTooltip, IEnumerable<TDropdownOptionViewModel> options, Action<TSelection> onSelected)
	{
		NoSelection = noSelection;
		NoSelectionTooltip = noSelectionTooltip;
		Unselect = unselect;
		UnselectTooltip = unselectTooltip;
		_onSelected = onSelected;
		Options = new List<TDropdownOptionViewModel>();
		foreach (TDropdownOptionViewModel option in options)
		{
			if (option.IsSelected())
			{
				CurrentSelection = option;
			}
			Options.Add(option);
		}
	}

	public void Select(TDropdownOptionViewModel option)
	{
		if (option == null || !option.IsSelected())
		{
			CurrentSelection = option;
			Action<TSelection> onSelected = _onSelected;
			if (onSelected != null)
			{
				TDropdownOptionViewModel currentSelection = CurrentSelection;
				onSelected((currentSelection != null) ? currentSelection.Selection : null);
			}
		}
	}
}
