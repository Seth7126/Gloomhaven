using System.IO;
using UnityEngine;
using XUnity.Common.Extensions;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;
using XUnity.ResourceRedirector.Properties;

namespace XUnity.ResourceRedirector;

public static class AssetBundleHelper
{
	internal static string PathForLoadedInMemoryBundle;

	public static AssetBundle CreateEmptyAssetBundle()
	{
		byte[] empty = Resources.empty;
		CabHelper.RandomizeCab(empty);
		return AssetBundle.LoadFromMemory(empty);
	}

	public static AssetBundleCreateRequest CreateEmptyAssetBundleRequest()
	{
		byte[] empty = Resources.empty;
		CabHelper.RandomizeCab(empty);
		return AssetBundle.LoadFromMemoryAsync(empty);
	}

	public static AssetBundle LoadFromMemory(string path, byte[] binary, uint crc)
	{
		try
		{
			PathForLoadedInMemoryBundle = path;
			return AssetBundle.LoadFromMemory(binary, crc);
		}
		finally
		{
			PathForLoadedInMemoryBundle = null;
		}
	}

	public static AssetBundleCreateRequest LoadFromMemoryAsync(string path, byte[] binary, uint crc)
	{
		try
		{
			PathForLoadedInMemoryBundle = path;
			return AssetBundle.LoadFromMemoryAsync(binary, crc);
		}
		finally
		{
			PathForLoadedInMemoryBundle = null;
		}
	}

	public static AssetBundle LoadFromFileWithRandomizedCabIfRequired(string path, uint crc, ulong offset)
	{
		return LoadFromFileWithRandomizedCabIfRequired(path, crc, offset, confirmFileExists: true);
	}

	internal static AssetBundle LoadFromFileWithRandomizedCabIfRequired(string path, uint crc, ulong offset, bool confirmFileExists)
	{
		AssetBundle val = AssetBundle.LoadFromFile(path, crc, offset);
		if ((Object)(object)val == (Object)null && (!confirmFileExists || File.Exists(path)))
		{
			byte[] array;
			using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				long num = fileStream.Length - (long)offset;
				fileStream.Seek((long)offset, SeekOrigin.Begin);
				array = fileStream.ReadFully((int)num);
			}
			CabHelper.RandomizeCabWithAnyLength(array);
			XuaLogger.ResourceRedirector.Warn("Randomized CAB for '" + path + "' in order to load it because another asset bundle already uses its CAB-string. You can ignore the previous error message, but this is likely caused by two mods incorrectly using the same CAB-string.");
			return AssetBundle.LoadFromMemory(array);
		}
		return val;
	}
}
