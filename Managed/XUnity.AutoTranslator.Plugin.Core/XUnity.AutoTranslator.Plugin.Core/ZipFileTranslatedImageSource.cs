using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using XUnity.Common.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core;

internal class ZipFileTranslatedImageSource : ITranslatedImageSource
{
	private readonly ZipFile _zipFile;

	private readonly ZipEntry _zipEntry;

	public ZipFileTranslatedImageSource(ZipFile zipFile, ZipEntry zipEntry)
	{
		_zipFile = zipFile;
		_zipEntry = zipEntry;
	}

	public byte[] GetData()
	{
		using Stream stream = _zipFile.GetInputStream(_zipEntry);
		return stream.ReadFully(16384);
	}
}
