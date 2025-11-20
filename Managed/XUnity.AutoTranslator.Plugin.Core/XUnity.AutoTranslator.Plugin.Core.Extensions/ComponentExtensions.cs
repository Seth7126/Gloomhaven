using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Text;
using XUnity.AutoTranslator.Plugin.Core.Textures;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Extensions;
using XUnity.Common.Harmony;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions;

internal static class ComponentExtensions
{
	private interface IPropertyMover
	{
		void MoveProperty(object source, object destination);
	}

	private class PropertyMover<T, TPropertyType> : IPropertyMover
	{
		private readonly Func<T, TPropertyType> _get;

		private readonly Action<T, TPropertyType> _set;

		public PropertyMover(PropertyInfo propertyInfo)
		{
			MethodInfo getMethod = propertyInfo.GetGetMethod();
			MethodInfo setMethod = propertyInfo.GetSetMethod();
			_get = (Func<T, TPropertyType>)ExpressionHelper.CreateTypedFastInvoke(getMethod);
			_set = (Action<T, TPropertyType>)ExpressionHelper.CreateTypedFastInvoke(setMethod);
		}

		public void MoveProperty(object source, object destination)
		{
			TPropertyType arg = _get((T)source);
			_set((T)destination, arg);
		}
	}

	private static readonly Color Transparent;

	private static readonly string SetAllDirtyMethodName;

	private static readonly string TexturePropertyName;

	private static readonly string MainTexturePropertyName;

	private static readonly string CapitalMainTexturePropertyName;

	private static readonly string MarkAsChangedMethodName;

	private static readonly string SupportRichTextPropertyName;

	private static readonly string RichTextPropertyName;

	private static GameObject[] _objects;

	private static readonly string XuaIgnore;

	private static readonly string XuaIgnoreTree;

	private static List<IPropertyMover> TexturePropertyMovers;

	private static readonly Dictionary<Type, ITextComponentManipulator> Manipulators;

	private static bool _guiContentCheckFailed;

	static ComponentExtensions()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Transparent = new Color(0f, 0f, 0f, 0f);
		SetAllDirtyMethodName = "SetAllDirty";
		TexturePropertyName = "texture";
		MainTexturePropertyName = "mainTexture";
		CapitalMainTexturePropertyName = "MainTexture";
		MarkAsChangedMethodName = "MarkAsChanged";
		SupportRichTextPropertyName = "supportRichText";
		RichTextPropertyName = "richText";
		_objects = (GameObject[])(object)new GameObject[128];
		XuaIgnore = "XUAIGNORE";
		XuaIgnoreTree = "XUAIGNORETREE";
		Manipulators = new Dictionary<Type, ITextComponentManipulator>();
		TexturePropertyMovers = new List<IPropertyMover>();
		LoadProperty<Object, string>("name");
		LoadProperty<Texture, int>("anisoLevel");
		LoadProperty<Texture, FilterMode>("filterMode");
		LoadProperty<Texture, float>("mipMapBias");
		LoadProperty<Texture, TextureWrapMode>("wrapMode");
	}

	private static void LoadProperty<TObject, TPropertyType>(string propertyName)
	{
		PropertyInfo property = typeof(TObject).GetProperty(propertyName);
		if (property != null && property.CanWrite && property.CanRead)
		{
			TexturePropertyMovers.Add(new PropertyMover<TObject, TPropertyType>(property));
		}
	}

	public static ITextComponentManipulator GetTextManipulator(this object ui)
	{
		if (ui == null)
		{
			return null;
		}
		Type unityType = ui.GetUnityType();
		if (!Manipulators.TryGetValue(unityType, out var value))
		{
			value = ((UnityTypes.TextField != null && UnityTypes.TextField.IsAssignableFrom(unityType)) ? new FairyGUITextComponentManipulator() : ((UnityTypes.TextArea2D != null && UnityTypes.TextArea2D.IsAssignableFrom(unityType)) ? new TextArea2DComponentManipulator() : ((UnityTypes.UguiNovelText == null || !UnityTypes.UguiNovelText.IsAssignableFrom(unityType)) ? ((ITextComponentManipulator)new DefaultTextComponentManipulator(ui.GetType())) : ((ITextComponentManipulator)new UguiNovelTextComponentManipulator(ui.GetType())))));
			Manipulators[unityType] = value;
		}
		return value;
	}

	public static bool ShouldIgnoreTextComponent(this object ui)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected O, but got Unknown
		Component val = (Component)((ui is Component) ? ui : null);
		if (val != null && Object.op_Implicit((Object)(object)val))
		{
			Transform val2 = val.transform;
			if (((Object)val2).name.Contains(XuaIgnore))
			{
				return true;
			}
			while (Object.op_Implicit((Object)(object)val2.parent))
			{
				val2 = val2.parent;
				if (((Object)val2).name.Contains(XuaIgnoreTree))
				{
					return true;
				}
			}
			Component val3 = null;
			if (UnityTypes.InputField != null)
			{
				val3 = val.gameObject.GetFirstComponentInSelfOrAncestor(UnityTypes.InputField?.UnityType);
				if ((Object)(object)val3 != (Object)null && UnityTypes.InputField_Properties.Placeholder != null)
				{
					Component x = (Component)UnityTypes.InputField_Properties.Placeholder.Get(val3);
					return !UnityObjectReferenceComparer.Default.Equals(x, val);
				}
			}
			if (UnityTypes.TMP_InputField != null)
			{
				val3 = val.gameObject.GetFirstComponentInSelfOrAncestor(UnityTypes.TMP_InputField?.UnityType);
				if ((Object)(object)val3 != (Object)null && UnityTypes.TMP_InputField_Properties.Placeholder != null)
				{
					Component x2 = (Component)UnityTypes.TMP_InputField_Properties.Placeholder.Get(val3);
					return !UnityObjectReferenceComparer.Default.Equals(x2, val);
				}
			}
			val3 = val.gameObject.GetFirstComponentInSelfOrAncestor(UnityTypes.UIInput?.UnityType);
			return (Object)(object)val3 != (Object)null;
		}
		return false;
	}

	public static bool IsComponentActive(this object ui)
	{
		Component val = (Component)((ui is Component) ? ui : null);
		if (val != null && Object.op_Implicit((Object)(object)val))
		{
			GameObject gameObject = val.gameObject;
			if (Object.op_Implicit((Object)(object)gameObject))
			{
				Behaviour val2 = (Behaviour)(object)((val is Behaviour) ? val : null);
				if (val2 != null)
				{
					if (gameObject.activeInHierarchy)
					{
						return val2.enabled;
					}
					return false;
				}
				return gameObject.activeInHierarchy;
			}
		}
		return true;
	}

	public static bool IsKnownTextType(this object ui)
	{
		if (ui == null)
		{
			return false;
		}
		Type unityType = ui.GetUnityType();
		if ((!Settings.EnableIMGUI || _guiContentCheckFailed || !IsGUIContentSafe(ui)) && (!Settings.EnableUGUI || UnityTypes.Text == null || !UnityTypes.Text.IsAssignableFrom(unityType)) && (!Settings.EnableNGUI || UnityTypes.UILabel == null || !UnityTypes.UILabel.IsAssignableFrom(unityType)) && (!Settings.EnableTextMesh || UnityTypes.TextMesh == null || !UnityTypes.TextMesh.IsAssignableFrom(unityType)) && (!Settings.EnableFairyGUI || UnityTypes.TextField == null || !UnityTypes.TextField.IsAssignableFrom(unityType)))
		{
			if (Settings.EnableTextMeshPro)
			{
				return IsKnownTextMeshProType(unityType);
			}
			return false;
		}
		return true;
	}

	public static bool IsKnownTextMeshProType(Type type)
	{
		if (UnityTypes.TMP_Text != null)
		{
			return UnityTypes.TMP_Text.IsAssignableFrom(type);
		}
		TypeContainer textMeshProUGUI = UnityTypes.TextMeshProUGUI;
		if (textMeshProUGUI == null || !textMeshProUGUI.IsAssignableFrom(type))
		{
			return UnityTypes.TextMeshPro?.IsAssignableFrom(type) ?? false;
		}
		return true;
	}

	public static bool SupportsRichText(this object ui)
	{
		if (ui == null)
		{
			return false;
		}
		Type type = ui.GetType();
		Type unityType = ui.GetUnityType();
		if ((UnityTypes.Text == null || !UnityTypes.Text.IsAssignableFrom(unityType) || !object.Equals(type.CachedProperty(SupportRichTextPropertyName)?.Get(ui), true)) && (UnityTypes.TextMesh == null || !UnityTypes.TextMesh.IsAssignableFrom(unityType) || !object.Equals(type.CachedProperty(RichTextPropertyName)?.Get(ui), true)) && !DoesTextMeshProSupportRichText(ui, type, unityType) && (UnityTypes.UguiNovelText == null || !UnityTypes.UguiNovelText.IsAssignableFrom(unityType)))
		{
			if (UnityTypes.TextField != null)
			{
				return UnityTypes.TextField.IsAssignableFrom(unityType);
			}
			return false;
		}
		return true;
	}

	public static bool DoesTextMeshProSupportRichText(object ui, Type clrType, Type unityType)
	{
		if (UnityTypes.TMP_Text != null)
		{
			if (UnityTypes.TMP_Text.IsAssignableFrom(unityType))
			{
				return object.Equals(clrType.CachedProperty(RichTextPropertyName)?.Get(ui), true);
			}
			return false;
		}
		TypeContainer textMeshPro = UnityTypes.TextMeshPro;
		if (textMeshPro == null || !textMeshPro.IsAssignableFrom(unityType) || !object.Equals(clrType.CachedProperty(RichTextPropertyName)?.Get(ui), true))
		{
			TypeContainer textMeshProUGUI = UnityTypes.TextMeshProUGUI;
			if (textMeshProUGUI != null && textMeshProUGUI.IsAssignableFrom(unityType))
			{
				return object.Equals(clrType.CachedProperty(RichTextPropertyName)?.Get(ui), true);
			}
			return false;
		}
		return true;
	}

	public static bool SupportsStabilization(this object ui)
	{
		if (ui == null)
		{
			return false;
		}
		if (!_guiContentCheckFailed)
		{
			return !IsGUIContentSafe(ui);
		}
		return true;
	}

	public static bool IsSpammingComponent(this object ui)
	{
		if (ui != null)
		{
			if (!_guiContentCheckFailed)
			{
				return IsGUIContentSafe(ui);
			}
			return false;
		}
		return true;
	}

	private static bool IsGUIContentSafe(object ui)
	{
		try
		{
			return IsGUIContentUnsafe(ui);
		}
		catch
		{
			_guiContentCheckFailed = true;
		}
		return false;
	}

	private static bool IsGUIContentUnsafe(object ui)
	{
		return ui is GUIContent;
	}

	private static bool SetTextOnGUIContentSafe(object ui, string text)
	{
		try
		{
			return SetTextOnGUIContentUnsafe(ui, text);
		}
		catch
		{
			_guiContentCheckFailed = true;
		}
		return false;
	}

	private static bool SetTextOnGUIContentUnsafe(object ui, string text)
	{
		GUIContent val = (GUIContent)((ui is GUIContent) ? ui : null);
		if (val != null)
		{
			val.text = text;
			return true;
		}
		return false;
	}

	private static bool TryGetTextFromGUIContentSafe(object ui, out string text)
	{
		try
		{
			return TryGetTextFromGUIContentUnsafe(ui, out text);
		}
		catch
		{
			_guiContentCheckFailed = false;
		}
		text = null;
		return false;
	}

	private static bool TryGetTextFromGUIContentUnsafe(object ui, out string text)
	{
		GUIContent val = (GUIContent)((ui is GUIContent) ? ui : null);
		if (val != null)
		{
			text = val.text;
			return true;
		}
		text = null;
		return false;
	}

	public static bool SupportsLineParser(this object ui)
	{
		if (Settings.GameLogTextPaths.Count > 0)
		{
			Component val = (Component)((ui is Component) ? ui : null);
			if (val != null)
			{
				return Settings.GameLogTextPaths.Contains(val.gameObject.GetPath());
			}
		}
		return false;
	}

	public static string GetText(this object ui, TextTranslationInfo info)
	{
		if (ui == null)
		{
			return null;
		}
		TextGetterCompatModeHelper.IsGettingText = true;
		try
		{
			string text = null;
			if ((_guiContentCheckFailed || !TryGetTextFromGUIContentSafe(ui, out text)) && info != null)
			{
				return info.TextManipulator.GetText(ui);
			}
			return text;
		}
		finally
		{
			TextGetterCompatModeHelper.IsGettingText = false;
		}
	}

	public static void SetText(this object ui, string text, TextTranslationInfo info)
	{
		if (ui != null && (_guiContentCheckFailed || !SetTextOnGUIContentSafe(ui, text)))
		{
			info?.TextManipulator.SetText(ui, text);
		}
	}

	public static TextTranslationInfo GetOrCreateTextTranslationInfo(this object ui)
	{
		if (!ui.IsSpammingComponent())
		{
			TextTranslationInfo orCreateExtensionData = ui.GetOrCreateExtensionData<TextTranslationInfo>();
			orCreateExtensionData.Initialize(ui);
			return orCreateExtensionData;
		}
		return null;
	}

	public static TextTranslationInfo GetTextTranslationInfo(this object ui)
	{
		if (!ui.IsSpammingComponent())
		{
			return ui.GetExtensionData<TextTranslationInfo>();
		}
		return null;
	}

	public static ImageTranslationInfo GetOrCreateImageTranslationInfo(this object obj, Texture2D originalTexture)
	{
		if (obj == null)
		{
			return null;
		}
		ImageTranslationInfo orCreateExtensionData = obj.GetOrCreateExtensionData<ImageTranslationInfo>();
		if (orCreateExtensionData.Original == null)
		{
			orCreateExtensionData.Initialize(originalTexture);
		}
		return orCreateExtensionData;
	}

	public static TextureTranslationInfo GetOrCreateTextureTranslationInfo(this Texture2D texture)
	{
		TextureTranslationInfo orCreateExtensionData = texture.GetOrCreateExtensionData<TextureTranslationInfo>();
		orCreateExtensionData.Initialize(texture);
		return orCreateExtensionData;
	}

	public static object CreateDerivedProxyIfRequiredAndPossible(this Component ui)
	{
		if (ui.IsKnownTextType())
		{
			return ui;
		}
		return null;
	}

	public static Component GetOrCreateNGUIDerivedProxy(this Component ui)
	{
		Type unityType = ui.GetUnityType();
		if (UnityTypes.UILabel != null && UnityTypes.UILabel.IsAssignableFrom(unityType))
		{
			return ui;
		}
		return null;
	}

	public static Component GetFirstComponentInSelfOrAncestor(this GameObject go, Type type)
	{
		if (type == null)
		{
			return null;
		}
		GameObject val = go;
		while ((Object)(object)val != (Object)null)
		{
			Component component = val.GetComponent(type);
			if ((Object)(object)component != (Object)null)
			{
				return component;
			}
			Transform transform = val.transform;
			object obj;
			if (transform == null)
			{
				obj = null;
			}
			else
			{
				Transform parent = transform.parent;
				obj = ((parent != null) ? ((Component)parent).gameObject : null);
			}
			val = (GameObject)obj;
		}
		return null;
	}

	public static IEnumerable<Component> GetAllTextComponentsInChildren(this GameObject go)
	{
		if (Settings.EnableTextMeshPro && UnityTypes.TMP_Text != null)
		{
			Component[] componentsInChildren = go.GetComponentsInChildren(UnityTypes.TMP_Text.UnityType, true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				yield return componentsInChildren[i];
			}
		}
		if (Settings.EnableUGUI && UnityTypes.Text != null)
		{
			Component[] componentsInChildren = go.GetComponentsInChildren(UnityTypes.Text.UnityType, true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				yield return componentsInChildren[i];
			}
		}
		if (Settings.EnableTextMesh && UnityTypes.TextMesh != null)
		{
			Component[] componentsInChildren = go.GetComponentsInChildren(UnityTypes.TextMesh.UnityType, true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				yield return componentsInChildren[i];
			}
		}
		if (Settings.EnableNGUI && UnityTypes.UILabel != null)
		{
			Component[] componentsInChildren = go.GetComponentsInChildren(UnityTypes.UILabel.UnityType, true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				yield return componentsInChildren[i];
			}
		}
	}

	private static GameObject GetAssociatedGameObject(object obj)
	{
		GameObject val = (GameObject)((obj is GameObject) ? obj : null);
		if (val == null)
		{
			Component val2 = (Component)((obj is Component) ? obj : null);
			if (val2 == null)
			{
				throw new ArgumentException("Expected object to be a GameObject or component.", "obj");
			}
			val = val2.gameObject;
		}
		return val;
	}

	public static string[] GetPathSegments(this object obj)
	{
		GameObject val = GetAssociatedGameObject(obj);
		int num = 0;
		int num2 = 0;
		_objects[num++] = val;
		while ((Object)(object)val.transform.parent != (Object)null)
		{
			val = ((Component)val.transform.parent).gameObject;
			_objects[num++] = val;
		}
		string[] array = new string[num];
		while (--num >= 0)
		{
			array[num2++] = ((Object)_objects[num]).name;
			_objects[num] = null;
		}
		return array;
	}

	public static string GetPath(this object obj)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string[] pathSegments = obj.GetPathSegments();
		for (int i = 0; i < pathSegments.Length; i++)
		{
			stringBuilder.Append("/").Append(pathSegments[i]);
		}
		return stringBuilder.ToString();
	}

	public static Texture2D GetTexture(this object ui)
	{
		if (ui == null)
		{
			return null;
		}
		if (ui.TryCastTo<SpriteRenderer>(out var castedObject))
		{
			Sprite sprite = castedObject.sprite;
			if (sprite == null)
			{
				return null;
			}
			return sprite.texture;
		}
		Type type = ui.GetType();
		object obj = type.CachedProperty(MainTexturePropertyName)?.Get(ui) ?? type.CachedProperty(TexturePropertyName)?.Get(ui) ?? type.CachedProperty(CapitalMainTexturePropertyName)?.Get(ui);
		return (Texture2D)((obj is Texture2D) ? obj : null);
	}

	public static Sprite SetTexture(this object ui, Texture2D texture, Sprite sprite, bool isPrefixHooked)
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Invalid comparison between Unknown and O
		if (ui == null)
		{
			return null;
		}
		Texture2D texture2 = ui.GetTexture();
		if (!UnityObjectReferenceComparer.Default.Equals(texture2, texture))
		{
			if (Settings.EnableSpriteRendererHooking && ui.TryCastTo<SpriteRenderer>(out var castedObject))
			{
				if (isPrefixHooked)
				{
					return SafeCreateSprite(castedObject, sprite, texture);
				}
				return SafeSetSprite(castedObject, sprite, texture);
			}
			Type type = ui.GetType();
			type.CachedProperty(MainTexturePropertyName)?.Set(ui, texture);
			type.CachedProperty(TexturePropertyName)?.Set(ui, texture);
			type.CachedProperty(CapitalMainTexturePropertyName)?.Set(ui, texture);
			object obj = type.CachedProperty("material")?.Get(ui);
			if (obj != null)
			{
				CachedProperty cachedProperty = obj.GetType().CachedProperty(MainTexturePropertyName);
				if ((object)(Texture2D)(cachedProperty?.Get(obj)) == texture2)
				{
					cachedProperty?.Set(obj, texture);
				}
			}
		}
		return null;
	}

	private static Sprite SafeSetSprite(SpriteRenderer sr, Sprite sprite, Texture2D texture)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		return sr.sprite = Sprite.Create(texture, ((Object)(object)sprite != (Object)null) ? sprite.rect : sr.sprite.rect, Vector2.zero);
	}

	private static Sprite SafeCreateSprite(SpriteRenderer sr, Sprite sprite, Texture2D texture)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		return Sprite.Create(texture, ((Object)(object)sprite != (Object)null) ? sprite.rect : sr.sprite.rect, Vector2.zero);
	}

	public static void SetAllDirtyEx(this object ui)
	{
		if (ui != null)
		{
			Type unityType = ui.GetUnityType();
			SpriteRenderer castedObject;
			if (UnityTypes.Graphic != null && UnityTypes.Graphic.IsAssignableFrom(unityType))
			{
				UnityTypes.Graphic.ClrType.CachedMethod(SetAllDirtyMethodName).Invoke(ui);
			}
			else if (!ui.TryCastTo<SpriteRenderer>(out castedObject))
			{
				AccessToolsShim.Method(ui.GetType(), MarkAsChangedMethodName)?.Invoke(ui, null);
			}
		}
	}

	public static bool IsKnownImageType(this object ui)
	{
		if (ui == null)
		{
			return false;
		}
		Type unityType = ui.GetUnityType();
		if (!ui.TryCastTo<Material>(out var _) && !ui.TryCastTo<SpriteRenderer>(out var _) && (UnityTypes.Image == null || !UnityTypes.Image.IsAssignableFrom(unityType)) && (UnityTypes.RawImage == null || !UnityTypes.RawImage.IsAssignableFrom(unityType)) && (UnityTypes.CubismRenderer == null || !UnityTypes.CubismRenderer.IsAssignableFrom(unityType)) && (UnityTypes.UIWidget == null || object.Equals(unityType, UnityTypes.UILabel?.UnityType) || !UnityTypes.UIWidget.IsAssignableFrom(unityType)) && (UnityTypes.UIAtlas == null || !UnityTypes.UIAtlas.IsAssignableFrom(unityType)) && (UnityTypes.UITexture == null || !UnityTypes.UITexture.IsAssignableFrom(unityType)))
		{
			if (UnityTypes.UIPanel != null)
			{
				return UnityTypes.UIPanel.IsAssignableFrom(unityType);
			}
			return false;
		}
		return true;
	}

	public static string GetTextureName(this object texture, string fallbackName)
	{
		if (texture.TryCastTo<Texture2D>(out var castedObject))
		{
			string name = ((Object)castedObject).name;
			if (!string.IsNullOrEmpty(name))
			{
				return name;
			}
		}
		return fallbackName;
	}

	public static void LoadImageEx(this Texture2D texture, byte[] data, ImageFormat dataType, Texture2D originalTexture)
	{
		TextureLoader.Load(texture, data, dataType);
		if (!((Object)(object)originalTexture != (Object)null))
		{
			return;
		}
		foreach (IPropertyMover texturePropertyMover in TexturePropertyMovers)
		{
			texturePropertyMover.MoveProperty(originalTexture, texture);
		}
	}

	private static byte[] EncodeToPNGEx(Texture2D texture)
	{
		if (UnityTypes.ImageConversion_Methods.EncodeToPNG != null)
		{
			return UnityTypes.ImageConversion_Methods.EncodeToPNG(texture);
		}
		if (UnityTypes.Texture2D_Methods.EncodeToPNG != null)
		{
			return UnityTypes.Texture2D_Methods.EncodeToPNG(texture);
		}
		throw new NotSupportedException("No way to encode the texture to PNG.");
	}

	public static TextureDataResult GetTextureData(this Texture2D texture)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_0084: Expected O, but got Unknown
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		int width = ((Texture)texture).width;
		int height = ((Texture)texture).height;
		byte[] array = null;
		if (array == null)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, (RenderTextureFormat)7, (RenderTextureReadWrite)0);
			GL.Clear(false, true, Transparent);
			Graphics.Blit((Texture)(object)texture, temporary);
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = temporary;
			Texture2D val = new Texture2D(width, height);
			val.ReadPixels(new Rect(0f, 0f, (float)((Texture)temporary).width, (float)((Texture)temporary).height), 0, 0);
			array = EncodeToPNGEx(val);
			Object.DestroyImmediate((Object)val);
			RenderTexture.active = (((Object)(object)active == (Object)(object)temporary) ? null : active);
			RenderTexture.ReleaseTemporary(temporary);
		}
		float realtimeSinceStartup2 = Time.realtimeSinceStartup;
		return new TextureDataResult(array, nonReadable: false, realtimeSinceStartup2 - realtimeSinceStartup);
	}

	public static bool IsCompatible(this object texture, ImageFormat dataType)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		TextureFormat format = ((Texture2D)texture).format;
		switch (dataType)
		{
		case ImageFormat.TGA:
			if ((int)format != 5 && (int)format != 4)
			{
				return (int)format == 3;
			}
			return true;
		default:
			return false;
		case ImageFormat.PNG:
			return true;
		}
	}
}
