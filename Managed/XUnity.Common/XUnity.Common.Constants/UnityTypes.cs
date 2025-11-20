using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using XUnity.Common.Utilities;

namespace XUnity.Common.Constants;

public static class UnityTypes
{
	public static class TMP_Settings_Properties
	{
		public static CachedProperty Version = TMP_Settings?.ClrType.CachedProperty("version");

		public static CachedProperty FallbackFontAssets = TMP_Settings?.ClrType.CachedProperty("fallbackFontAssets");
	}

	public static class TMP_FontAsset_Properties
	{
		public static CachedProperty Version = TMP_FontAsset?.ClrType.CachedProperty("version");
	}

	public static class AdvScenarioData_Properties
	{
		public static CachedProperty ScenarioLabels = AdvScenarioData?.ClrType.CachedProperty("ScenarioLabels");
	}

	public static class UguiNovelText_Properties
	{
		public static CachedProperty TextGenerator = UguiNovelText?.ClrType.CachedProperty("TextGenerator");
	}

	public static class UguiNovelText_Methods
	{
		public static CachedMethod SetAllDirty = UguiNovelText?.ClrType.CachedMethod("SetAllDirty");
	}

	public static class UguiNovelTextGenerator_Methods
	{
		public static CachedMethod Refresh = UguiNovelTextGenerator?.ClrType.CachedMethod("Refresh");
	}

	public static class AdvUguiMessageWindow_Properties
	{
		public static CachedProperty Text = AdvUguiMessageWindow?.ClrType.CachedProperty("Text");

		public static CachedProperty Engine = AdvUguiMessageWindow?.ClrType.CachedProperty("Engine");
	}

	public static class AdvUiMessageWindow_Fields
	{
		public static CachedField text = AdvUiMessageWindow?.ClrType.CachedField("text");

		public static CachedField nameText = AdvUiMessageWindow?.ClrType.CachedField("nameText");
	}

	public static class AdvUguiMessageWindow_Fields
	{
		public static FieldInfo text = AdvUguiMessageWindow?.ClrType.GetField("text", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

		public static FieldInfo nameText = AdvUguiMessageWindow?.ClrType.GetField("nameText", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

		public static FieldInfo engine = AdvUguiMessageWindow?.ClrType.GetField("engine", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
	}

	public static class AdvEngine_Properties
	{
		public static CachedProperty Page = AdvEngine?.ClrType.CachedProperty("Page");
	}

	public static class AdvPage_Methods
	{
		public static CachedMethod RemakeTextData = AdvPage?.ClrType.CachedMethod("RemakeTextData");

		public static CachedMethod RemakeText = AdvPage?.ClrType.CachedMethod("RemakeText");

		public static CachedMethod ChangeMessageWindowText = AdvPage?.ClrType.CachedMethod("ChangeMessageWindowText", typeof(string), typeof(string), typeof(string), typeof(string));
	}

	public static class UILabel_Properties
	{
		public static CachedProperty MultiLine = UILabel?.ClrType.CachedProperty("multiLine");

		public static CachedProperty OverflowMethod = UILabel?.ClrType.CachedProperty("overflowMethod");

		public static CachedProperty SpacingX = UILabel?.ClrType.CachedProperty("spacingX");

		public static CachedProperty UseFloatSpacing = UILabel?.ClrType.CachedProperty("useFloatSpacing");
	}

	public static class Text_Properties
	{
		public static CachedProperty Font = Text?.ClrType.CachedProperty("font");

		public static CachedProperty FontSize = Text?.ClrType.CachedProperty("fontSize");

		public static CachedProperty HorizontalOverflow = Text?.ClrType.CachedProperty("horizontalOverflow");

		public static CachedProperty VerticalOverflow = Text?.ClrType.CachedProperty("verticalOverflow");

		public static CachedProperty LineSpacing = Text?.ClrType.CachedProperty("lineSpacing");

		public static CachedProperty ResizeTextForBestFit = Text?.ClrType.CachedProperty("resizeTextForBestFit");

		public static CachedProperty ResizeTextMinSize = Text?.ClrType.CachedProperty("resizeTextMinSize");

		public static CachedProperty ResizeTextMaxSize = Text?.ClrType.CachedProperty("resizeTextMaxSize");
	}

	public static class InputField_Properties
	{
		public static CachedProperty Placeholder = InputField?.ClrType.CachedProperty("placeholder");
	}

	public static class TMP_InputField_Properties
	{
		public static CachedProperty Placeholder = TMP_InputField?.ClrType.CachedProperty("placeholder");
	}

	public static class Font_Properties
	{
		public static CachedProperty FontSize = Font?.ClrType.CachedProperty("fontSize");
	}

	public static class AssetBundle_Methods
	{
		public static CachedMethod LoadAll = AssetBundle?.ClrType.CachedMethod("LoadAll", typeof(Type));

		public static CachedMethod LoadAllAssets = AssetBundle?.ClrType.CachedMethod("LoadAllAssets", typeof(Type));

		public static CachedMethod LoadFromFile = AssetBundle?.ClrType.CachedMethod("LoadFromFile", typeof(string));

		public static CachedMethod CreateFromFile = AssetBundle?.ClrType.CachedMethod("CreateFromFile", typeof(string));
	}

	public static class TextExpansion_Methods
	{
		public static CachedMethod SetMessageType = TextExpansion?.ClrType.CachedMethod("SetMessageType");

		public static CachedMethod SkipTypeWriter = TextExpansion?.ClrType.CachedMethod("SkipTypeWriter");
	}

	public static class GameObject_Methods
	{
	}

	public static class TextMesh_Methods
	{
	}

	public static class Text_Methods
	{
	}

	public static class InputField_Methods
	{
	}

	public static class TMP_Text_Methods
	{
	}

	public static class TMP_InputField_Methods
	{
	}

	public static class TextMeshPro_Methods
	{
	}

	public static class TextMeshProUGUI_Methods
	{
	}

	public static class UILabel_Methods
	{
	}

	public static class UIRect_Methods
	{
	}

	public static class SceneManager_Methods
	{
		public static readonly Action<UnityAction<Scene, LoadSceneMode>> add_sceneLoaded = (Action<UnityAction<Scene, LoadSceneMode>>)ExpressionHelper.CreateTypedFastInvokeUnchecked(typeof(SceneManager).GetMethod("add_sceneLoaded", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(UnityAction<Scene, LoadSceneMode>) }, null));
	}

	public static class Texture2D_Methods
	{
		public static readonly Func<Texture2D, byte[], bool> LoadImage = (Func<Texture2D, byte[], bool>)ExpressionHelper.CreateTypedFastInvokeUnchecked(typeof(Texture2D).GetMethod("LoadImage", BindingFlags.Instance | BindingFlags.Public, null, new Type[1] { typeof(byte[]) }, null));

		public static readonly Func<Texture2D, byte[]> EncodeToPNG = (Func<Texture2D, byte[]>)ExpressionHelper.CreateTypedFastInvokeUnchecked(typeof(Texture2D).GetMethod("EncodeToPNG", BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null));
	}

	public static class ImageConversion_Methods
	{
		public static readonly Func<Texture2D, byte[], bool, bool> LoadImage = (Func<Texture2D, byte[], bool, bool>)ExpressionHelper.CreateTypedFastInvokeUnchecked(ImageConversion?.ClrType.GetMethod("LoadImage", BindingFlags.Static | BindingFlags.Public, null, new Type[3]
		{
			typeof(Texture2D),
			typeof(byte[]),
			typeof(bool)
		}, null));

		public static readonly Func<Texture2D, byte[]> EncodeToPNG = (Func<Texture2D, byte[]>)ExpressionHelper.CreateTypedFastInvokeUnchecked(ImageConversion?.ClrType.GetMethod("EncodeToPNG", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(Texture2D) }, null));
	}

	public static readonly TypeContainer UILabel = FindType("UILabel");

	public static readonly TypeContainer UIWidget = FindType("UIWidget");

	public static readonly TypeContainer UIAtlas = FindType("UIAtlas");

	public static readonly TypeContainer UISprite = FindType("UISprite");

	public static readonly TypeContainer UITexture = FindType("UITexture");

	public static readonly TypeContainer UI2DSprite = FindType("UI2DSprite");

	public static readonly TypeContainer UIFont = FindType("UIFont");

	public static readonly TypeContainer UIPanel = FindType("UIPanel");

	public static readonly TypeContainer UIRect = FindType("UIRect");

	public static readonly TypeContainer UIInput = FindType("UIInput");

	public static readonly TypeContainer TextField = FindType("FairyGUI.TextField");

	public static readonly TypeContainer TMP_InputField = FindType("TMPro.TMP_InputField");

	public static readonly TypeContainer TMP_Text = FindType("TMPro.TMP_Text");

	public static readonly TypeContainer TextMeshProUGUI = FindType("TMPro.TextMeshProUGUI");

	public static readonly TypeContainer TextMeshPro = FindType("TMPro.TextMeshPro");

	public static readonly TypeContainer TMP_FontAsset = FindType("TMPro.TMP_FontAsset");

	public static readonly TypeContainer TMP_Settings = FindType("TMPro.TMP_Settings");

	public static readonly TypeContainer GameObject = FindType("UnityEngine.GameObject");

	public static readonly TypeContainer Transform = FindType("UnityEngine.Transform");

	public static readonly TypeContainer TextMesh = FindType("UnityEngine.TextMesh");

	public static readonly TypeContainer Text = FindType("UnityEngine.UI.Text");

	public static readonly TypeContainer Image = FindType("UnityEngine.UI.Image");

	public static readonly TypeContainer RawImage = FindType("UnityEngine.UI.RawImage");

	public static readonly TypeContainer MaskableGraphic = FindType("UnityEngine.UI.MaskableGraphic");

	public static readonly TypeContainer Graphic = FindType("UnityEngine.UI.Graphic");

	public static readonly TypeContainer GUIContent = FindType("UnityEngine.GUIContent");

	public static readonly TypeContainer WWW = FindType("UnityEngine.WWW");

	public static readonly TypeContainer InputField = FindType("UnityEngine.UI.InputField");

	public static readonly TypeContainer GUI = FindType("UnityEngine.GUI");

	public static readonly TypeContainer GUI_ToolbarButtonSize = FindType("UnityEngine.GUI+ToolbarButtonSize");

	public static readonly TypeContainer GUIStyle = FindType("UnityEngine.GUIStyle");

	public static readonly TypeContainer ImageConversion = FindType("UnityEngine.ImageConversion");

	public static readonly TypeContainer Texture2D = FindType("UnityEngine.Texture2D");

	public static readonly TypeContainer Texture = FindType("UnityEngine.Texture");

	public static readonly TypeContainer SpriteRenderer = FindType("UnityEngine.SpriteRenderer");

	public static readonly TypeContainer Sprite = FindType("UnityEngine.Sprite");

	public static readonly TypeContainer Object = FindType("UnityEngine.Object");

	public static readonly TypeContainer TextEditor = FindType("UnityEngine.TextEditor");

	public static readonly TypeContainer CustomYieldInstruction = FindType("UnityEngine.CustomYieldInstruction");

	public static readonly TypeContainer SceneManager = FindType("UnityEngine.SceneManagement.SceneManager");

	public static readonly TypeContainer Scene = FindType("UnityEngine.SceneManagement.Scene");

	public static readonly TypeContainer UnityEventBase = FindType("UnityEngine.Events.UnityEventBase");

	public static readonly TypeContainer BaseInvokableCall = FindType("UnityEngine.Events.BaseInvokableCall");

	public static readonly TypeContainer Font = FindType("UnityEngine.Font");

	public static readonly TypeContainer WaitForSecondsRealtime = FindType("UnityEngine.WaitForSecondsRealtime");

	public static readonly TypeContainer Input = FindType("UnityEngine.Input");

	public static readonly TypeContainer AssetBundleCreateRequest = FindType("UnityEngine.AssetBundleCreateRequest");

	public static readonly TypeContainer AssetBundle = FindType("UnityEngine.AssetBundle");

	public static readonly TypeContainer AssetBundleRequest = FindType("UnityEngine.AssetBundleRequest");

	public static readonly TypeContainer Resources = FindType("UnityEngine.Resources");

	public static readonly TypeContainer AsyncOperation = FindType("UnityEngine.AsyncOperation");

	public static readonly TypeContainer TextAsset = FindType("UnityEngine.TextAsset");

	public static readonly Type HorizontalWrapMode = FindClrType("UnityEngine.HorizontalWrapMode");

	public static readonly Type TextOverflowModes = FindClrType("TMPro.TextOverflowModes");

	public static readonly Type TextAlignmentOptions = FindClrType("TMPro.TextAlignmentOptions");

	public static readonly Type VerticalWrapMode = FindClrType("UnityEngine.VerticalWrapMode");

	public static readonly TypeContainer TextExpansion = FindType("UnityEngine.UI.TextExpansion");

	public static readonly TypeContainer Typewriter = FindType("Typewriter");

	public static readonly TypeContainer UguiNovelText = FindType("Utage.UguiNovelText");

	public static readonly TypeContainer UguiNovelTextGenerator = FindType("Utage.UguiNovelTextGenerator");

	public static readonly TypeContainer AdvEngine = FindType("Utage.AdvEngine");

	public static readonly TypeContainer AdvPage = FindType("Utage.AdvPage");

	public static readonly TypeContainer TextData = FindType("Utage.TextData");

	public static readonly TypeContainer AdvUguiMessageWindow = FindType("Utage.AdvUguiMessageWindow") ?? FindType("AdvUguiMessageWindow");

	public static readonly TypeContainer AdvUiMessageWindow = FindType("AdvUiMessageWindow");

	public static readonly TypeContainer AdvDataManager = FindType("Utage.AdvDataManager");

	public static readonly TypeContainer AdvScenarioData = FindType("Utage.AdvScenarioData");

	public static readonly TypeContainer AdvScenarioLabelData = FindType("Utage.AdvScenarioLabelData");

	public static readonly TypeContainer DicingTextures = FindType("Utage.DicingTextures");

	public static readonly TypeContainer DicingImage = FindType("Utage.DicingImage");

	public static readonly TypeContainer TextArea2D = FindType("Utage.TextArea2D");

	public static readonly TypeContainer CubismRenderer = FindType("Live2D.Cubism.Rendering.CubismRenderer");

	public static readonly TypeContainer TextWindow = FindType("Assets.System.Text.TextWindow");

	private static Type FindClrType(string name)
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			try
			{
				Type type = assembly.GetType(name, throwOnError: false);
				if (type != null)
				{
					return type;
				}
			}
			catch
			{
			}
		}
		return null;
	}

	private static TypeContainer FindType(string name)
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			try
			{
				Type type = assembly.GetType(name, throwOnError: false);
				if (type != null)
				{
					return new TypeContainer(type);
				}
			}
			catch
			{
			}
		}
		return null;
	}
}
