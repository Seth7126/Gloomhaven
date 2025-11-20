using System.IO;

namespace XUnity.ResourceRedirector;

public class AssetBundleLoadingParameters
{
	public string Path { get; set; }

	public uint Crc { get; set; }

	public ulong Offset { get; set; }

	public Stream Stream { get; set; }

	public uint ManagedReadBufferSize { get; }

	public byte[] Binary { get; set; }

	public AssetBundleLoadType LoadType { get; }

	internal AssetBundleLoadingParameters(byte[] data, string path, uint crc, ulong offset, Stream stream, uint managedReadBufferSize, AssetBundleLoadType loadType)
	{
		Binary = data;
		Path = path;
		Crc = crc;
		Offset = offset;
		Stream = stream;
		ManagedReadBufferSize = managedReadBufferSize;
		LoadType = loadType;
	}
}
