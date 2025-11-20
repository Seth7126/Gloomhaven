using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions;

internal static class ZipFileExtensions
{
	public static List<ZipEntry> GetEntries(this ZipFile zf)
	{
		List<ZipEntry> list = new List<ZipEntry>();
		foreach (ZipEntry item in zf)
		{
			list.Add(item);
		}
		return list;
	}
}
