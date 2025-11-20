using System.Reflection;
using XUnity.Common.Constants;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Text;

internal class FairyGUITextComponentManipulator : ITextComponentManipulator
{
	private readonly CachedField _html;

	private readonly CachedProperty _htmlText;

	private readonly CachedProperty _text;

	public FairyGUITextComponentManipulator()
	{
		_html = UnityTypes.TextField.ClrType.CachedField("html") ?? UnityTypes.TextField.ClrType.CachedFieldByIndex(3, typeof(bool), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		_text = UnityTypes.TextField.ClrType.CachedProperty("text");
		_htmlText = UnityTypes.TextField.ClrType.CachedProperty("htmlText");
	}

	public string GetText(object ui)
	{
		if ((bool)_html.Get(ui))
		{
			return (string)_htmlText.Get(ui);
		}
		return (string)_text.Get(ui);
	}

	public void SetText(object ui, string text)
	{
		if ((bool)_html.Get(ui))
		{
			_htmlText.Set(ui, text);
		}
		else
		{
			_text.Set(ui, text);
		}
	}
}
