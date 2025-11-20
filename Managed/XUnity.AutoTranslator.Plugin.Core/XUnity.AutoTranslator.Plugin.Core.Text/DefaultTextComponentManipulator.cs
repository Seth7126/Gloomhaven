using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.Common.Constants;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Text;

internal class DefaultTextComponentManipulator : ITextComponentManipulator
{
	private class TypeAndMethod
	{
		private FastReflectionDelegate _setterInvoker;

		public Type Type { get; }

		public MethodBase SetterMethod { get; }

		public FastReflectionDelegate SetterInvoker => _setterInvoker ?? (_setterInvoker = SetterMethod.CreateFastDelegate(directBoxValueAccess: true, forceNonVirtCall: true));

		public TypeAndMethod(Type type, MethodBase method)
		{
			Type = type;
			SetterMethod = method;
		}
	}

	private static readonly string TextPropertyName = "text";

	private readonly Type _type;

	private readonly CachedProperty _property;

	private static Dictionary<Type, TypeAndMethod> _textSetters = new Dictionary<Type, TypeAndMethod>();

	public DefaultTextComponentManipulator(Type type)
	{
		_type = type;
		_property = type.CachedProperty(TextPropertyName);
	}

	public string GetText(object ui)
	{
		return (string)_property?.Get(ui);
	}

	public void SetText(object ui, string text)
	{
		Type type = _type;
		if (UnityTypes.TextWindow != null)
		{
			TypeContainer textMeshPro = UnityTypes.TextMeshPro;
			if (textMeshPro != null && textMeshPro.ClrType.IsAssignableFrom(type))
			{
				Object textWindow = Object.FindObjectOfType(UnityTypes.TextWindow.ClrType);
				if (textWindow != (Object)null)
				{
					BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
					object obj = ((object)textWindow).GetType().GetField("TextMesh", flags)?.GetValue(textWindow);
					if (obj != null && object.Equals(obj, ui))
					{
						if (new StackTrace().GetFrames().Any((StackFrame x) => x.GetMethod().DeclaringType == UnityTypes.TextWindow.ClrType))
						{
							object previousCurText = ((object)textWindow).GetType().GetField("curText", flags).GetValue(textWindow);
							((object)textWindow).GetType().GetField("curText", flags).SetValue(textWindow, text);
							Settings.SetCurText = delegate(object textWindowInner)
							{
								object value3 = ((object)textWindow).GetType().GetField("curText", flags).GetValue(textWindow);
								if (object.Equals(text, value3))
								{
									textWindowInner.GetType().GetMethod("FinishTyping", flags).Invoke(textWindowInner, null);
									textWindowInner.GetType().GetField("curText", flags).SetValue(textWindowInner, previousCurText);
									object value4 = textWindowInner.GetType().GetField("TextMesh", flags).GetValue(textWindowInner);
									object value5 = textWindowInner.GetType().GetField("Keyword", flags).GetValue(textWindowInner);
									value5.GetType().GetMethod("UpdateTextMesh", flags).Invoke(value5, new object[2] { value4, true });
								}
								Settings.SetCurText = null;
							};
						}
						else
						{
							type.CachedProperty(TextPropertyName)?.Set(ui, text);
							object value = ((object)textWindow).GetType().GetField("curText", flags).GetValue(textWindow);
							((object)textWindow).GetType().GetField("curText", flags).SetValue(textWindow, text);
							((object)textWindow).GetType().GetMethod("FinishTyping", flags).Invoke(textWindow, null);
							((object)textWindow).GetType().GetField("curText", flags).SetValue(textWindow, value);
							object value2 = ((object)textWindow).GetType().GetField("Keyword", flags).GetValue(textWindow);
							value2.GetType().GetMethod("UpdateTextMesh", flags).Invoke(value2, new object[2] { obj, true });
						}
						return;
					}
				}
			}
		}
		CachedProperty property = _property;
		if (property != null)
		{
			property.Set(ui, text);
			if (Settings.IgnoreVirtualTextSetterCallingRules)
			{
				string text2 = (string)property.Get(ui);
				Type type2 = type;
				while (text != text2 && type2 != null)
				{
					TypeAndMethod textPropertySetterInParent = GetTextPropertySetterInParent(type2);
					if (textPropertySetterInParent != null)
					{
						type2 = textPropertySetterInParent.Type;
						textPropertySetterInParent.SetterInvoker(ui, text);
						text2 = (string)property.Get(ui);
					}
					else
					{
						type2 = null;
					}
				}
			}
		}
		CachedProperty cachedProperty = type.CachedProperty("maxVisibleCharacters");
		if (cachedProperty != null && cachedProperty.PropertyType == typeof(int))
		{
			int num = (int)cachedProperty.Get(ui);
			if (0 < num && num < 99999)
			{
				cachedProperty.Set(ui, 99999);
			}
		}
		if (UnityTypes.TextExpansion_Methods.SetMessageType != null && UnityTypes.TextExpansion_Methods.SkipTypeWriter != null && UnityTypes.TextExpansion.ClrType.IsAssignableFrom(type))
		{
			UnityTypes.TextExpansion_Methods.SetMessageType.Invoke(ui, 1);
			UnityTypes.TextExpansion_Methods.SkipTypeWriter.Invoke(ui);
		}
	}

	private static TypeAndMethod GetTextPropertySetterInParent(Type type)
	{
		for (Type baseType = type.BaseType; baseType != null; baseType = baseType.BaseType)
		{
			if (_textSetters.TryGetValue(type, out var value))
			{
				return value;
			}
			PropertyInfo property = baseType.GetProperty(TextPropertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null && property.CanWrite)
			{
				TypeAndMethod typeAndMethod = new TypeAndMethod(baseType, property.GetSetMethod());
				_textSetters[baseType] = typeAndMethod;
				return typeAndMethod;
			}
		}
		return null;
	}
}
