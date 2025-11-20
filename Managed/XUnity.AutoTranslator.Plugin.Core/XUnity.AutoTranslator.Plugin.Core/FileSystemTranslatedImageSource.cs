using System.IO;
using XUnity.Common.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core;

internal class FileSystemTranslatedImageSource : ITranslatedImageSource
{
	private readonly string _fileName;

	public FileSystemTranslatedImageSource(string fileName)
	{
		_fileName = fileName;
	}

	public byte[] GetData()
	{
		using FileStream stream = File.OpenRead(_fileName);
		return stream.ReadFully(16384);
	}
}
