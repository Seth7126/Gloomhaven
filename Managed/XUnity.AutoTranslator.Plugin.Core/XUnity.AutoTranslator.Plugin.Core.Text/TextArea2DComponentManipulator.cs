using System;
using System.Reflection;
using UnityEngine;
using XUnity.Common.Constants;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Text;

internal class TextArea2DComponentManipulator : ITextComponentManipulator
{
	private readonly Action<object, object> set_status;

	private readonly Action<object, object> set_textData;

	private readonly Action<object, object> set_nameText;

	private readonly Action<object, bool> set_isInputSendMessage;

	private readonly CachedProperty _text;

	private readonly CachedProperty _TextData;

	public TextArea2DComponentManipulator()
	{
		FieldInfo field = UnityTypes.AdvPage.ClrType.GetField("textData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		set_textData = CustomFastReflectionHelper.CreateFastFieldSetter<object, object>(field);
		FieldInfo field2 = UnityTypes.AdvPage.ClrType.GetField("status", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		set_status = CustomFastReflectionHelper.CreateFastFieldSetter<object, object>(field2);
		FieldInfo field3 = UnityTypes.AdvPage.ClrType.GetField("isInputSendMessage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		set_isInputSendMessage = CustomFastReflectionHelper.CreateFastFieldSetter<object, bool>(field3);
		FieldInfo field4 = UnityTypes.AdvPage.ClrType.GetField("nameText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		set_nameText = CustomFastReflectionHelper.CreateFastFieldSetter<object, object>(field4);
		_text = UnityTypes.TextArea2D.ClrType.CachedProperty("text");
		_TextData = UnityTypes.TextArea2D.ClrType.CachedProperty("TextData");
	}

	public string GetText(object ui)
	{
		object obj = _TextData.Get(ui);
		if (obj != null)
		{
			return obj.GetExtensionData<string>();
		}
		return (string)_text.Get(ui);
	}

	public void SetText(object ui, string text)
	{
		if (UnityTypes.AdvUiMessageWindow != null && UnityTypes.AdvPage != null)
		{
			Object instance = Object.FindObjectOfType(UnityTypes.AdvUiMessageWindow.UnityType);
			object objA = UnityTypes.AdvUiMessageWindow_Fields.text.Get(instance);
			object objA2 = UnityTypes.AdvUiMessageWindow_Fields.nameText.Get(instance);
			if (object.Equals(objA, ui))
			{
				Object arg = Object.FindObjectOfType(UnityTypes.AdvPage.UnityType);
				object obj = Activator.CreateInstance(UnityTypes.TextData.ClrType, text);
				_TextData.Set(ui, obj);
				set_textData(arg, obj);
				set_status(arg, 0);
				set_isInputSendMessage(arg, arg2: false);
				return;
			}
			if (object.Equals(objA2, ui))
			{
				Object arg2 = Object.FindObjectOfType(UnityTypes.AdvPage.UnityType);
				object arg3 = Activator.CreateInstance(UnityTypes.TextData.ClrType, text);
				_TextData.Set(ui, arg3);
				set_nameText(arg2, text);
				return;
			}
		}
		object arg4 = Activator.CreateInstance(UnityTypes.TextData.ClrType, text);
		_text.Set(ui, text);
		_TextData.Set(ui, arg4);
	}
}
