using System;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;

namespace XUnity.AutoTranslator.Plugin.Core.UI;

internal class TranslatorDropdownOptionViewModel : DropdownOptionViewModel<TranslationEndpointManager>
{
	private GUIContent _selected;

	private GUIContent _normal;

	private GUIContent _disabled;

	public override GUIContent Text
	{
		get
		{
			if (base.Selection.Error != null)
			{
				return _disabled;
			}
			if (base.IsSelected())
			{
				return _selected;
			}
			return _normal;
		}
	}

	public TranslatorDropdownOptionViewModel(bool fallback, Func<bool> isSelected, TranslationEndpointManager selection)
		: base(selection.Endpoint.FriendlyName, isSelected, (Func<bool>)(() => selection.Error == null), selection)
	{
		if (fallback)
		{
			_selected = GUIUtil.CreateContent(selection.Endpoint.FriendlyName, "<b>CURRENT FALLBACK TRANSLATOR</b>\n" + selection.Endpoint.FriendlyName + " is the currently selected fallback translator that will be used to perform translations when the primary translator fails.");
			_disabled = GUIUtil.CreateContent(selection.Endpoint.FriendlyName, "<b>CANNOT SELECT FALLBACK TRANSLATOR</b>\n" + selection.Endpoint.FriendlyName + " cannot be selected because the initialization failed. " + selection.Error?.Message);
			_normal = GUIUtil.CreateContent(selection.Endpoint.FriendlyName, "<b>SELECT FALLBACK TRANSLATOR</b>\n" + selection.Endpoint.FriendlyName + " will be selected as fallback translator.");
		}
		else
		{
			_selected = GUIUtil.CreateContent(selection.Endpoint.FriendlyName, "<b>CURRENT TRANSLATOR</b>\n" + selection.Endpoint.FriendlyName + " is the currently selected translator that will be used to perform translations.");
			_disabled = GUIUtil.CreateContent(selection.Endpoint.FriendlyName, "<b>CANNOT SELECT TRANSLATOR</b>\n" + selection.Endpoint.FriendlyName + " cannot be selected because the initialization failed. " + selection.Error?.Message);
			_normal = GUIUtil.CreateContent(selection.Endpoint.FriendlyName, "<b>SELECT TRANSLATOR</b>\n" + selection.Endpoint.FriendlyName + " will be selected as translator.");
		}
	}
}
