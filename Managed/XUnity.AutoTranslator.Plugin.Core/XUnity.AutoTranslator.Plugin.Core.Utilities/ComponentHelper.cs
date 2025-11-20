using UnityEngine;
using XUnity.Common.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities;

internal static class ComponentHelper
{
	public static T[] FindObjectsOfType<T>() where T : Object
	{
		Object[] array = Object.FindObjectsOfType(typeof(T));
		if (array == null)
		{
			return null;
		}
		T[] array2 = new T[array.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			array[i].TryCastTo<T>(out var castedObject);
			array2[i] = castedObject;
		}
		return array2;
	}

	public static Texture2D CreateEmptyTexture2D(TextureFormat originalTextureFormat)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		TextureFormat val = (((int)originalTextureFormat == 3) ? ((TextureFormat)3) : (((int)originalTextureFormat == 10) ? ((TextureFormat)3) : (((int)originalTextureFormat != 12) ? ((TextureFormat)5) : ((TextureFormat)5))));
		return new Texture2D(2, 2, val, false);
	}
}
