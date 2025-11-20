using System.IO;
using System.Linq;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.Common.Constants;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Fonts;

internal static class FontHelper
{
	public static Object GetTextMeshProFont(string assetBundle)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		Object val = null;
		string text = Path.Combine(Paths.GameRoot, assetBundle);
		if (File.Exists(text))
		{
			XuaLogger.AutoTranslator.Info("Attempting to load TextMesh Pro font from asset bundle.");
			AssetBundle val2 = null;
			if (UnityTypes.AssetBundle_Methods.LoadFromFile != null)
			{
				val2 = (AssetBundle)UnityTypes.AssetBundle_Methods.LoadFromFile.Invoke(null, new object[1] { text });
			}
			else
			{
				if (UnityTypes.AssetBundle_Methods.CreateFromFile == null)
				{
					XuaLogger.AutoTranslator.Error("Could not find an appropriate asset bundle load method while loading font: " + text);
					return null;
				}
				val2 = (AssetBundle)UnityTypes.AssetBundle_Methods.CreateFromFile.Invoke(null, new object[1] { text });
			}
			if ((Object)(object)val2 == (Object)null)
			{
				XuaLogger.AutoTranslator.Warn("Could not load asset bundle while loading font: " + text);
				return null;
			}
			if (UnityTypes.TMP_FontAsset != null)
			{
				if (UnityTypes.AssetBundle_Methods.LoadAllAssets != null)
				{
					val = ((Object[])UnityTypes.AssetBundle_Methods.LoadAllAssets.Invoke(val2, new object[1] { UnityTypes.TMP_FontAsset.UnityType }))?.FirstOrDefault();
				}
				else if (UnityTypes.AssetBundle_Methods.LoadAll != null)
				{
					val = ((Object[])UnityTypes.AssetBundle_Methods.LoadAll.Invoke(val2, new object[1] { UnityTypes.TMP_FontAsset.UnityType }))?.FirstOrDefault();
				}
			}
		}
		else
		{
			XuaLogger.AutoTranslator.Info("Attempting to load TextMesh Pro font from internal Resources API.");
			val = Resources.Load(assetBundle);
		}
		if (val != (Object)null)
		{
			CachedProperty version = UnityTypes.TMP_FontAsset_Properties.Version;
			string text2 = ((string)version?.Get(val)) ?? "Unknown";
			XuaLogger.AutoTranslator.Info("Loaded TextMesh Pro font uses version: " + text2);
			if (version != null && Settings.TextMeshProVersion != null && text2 != Settings.TextMeshProVersion)
			{
				XuaLogger.AutoTranslator.Warn("TextMesh Pro version mismatch. Font asset version: " + text2 + ", TextMesh Pro version: " + Settings.TextMeshProVersion);
			}
			Object.DontDestroyOnLoad(val);
		}
		else
		{
			XuaLogger.AutoTranslator.Error("Could not find the TextMeshPro font asset: " + assetBundle);
		}
		return val;
	}

	public static Font GetTextFont(int size)
	{
		Font obj = Font.CreateDynamicFontFromOSFont(Settings.OverrideFont, size);
		Object.DontDestroyOnLoad((Object)(object)obj);
		return obj;
	}

	public static string[] GetOSInstalledFontNames()
	{
		return Font.GetOSInstalledFontNames();
	}
}
