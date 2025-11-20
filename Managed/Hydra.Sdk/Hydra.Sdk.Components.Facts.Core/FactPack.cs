using Hydra.Api.Facts;

namespace Hydra.Sdk.Components.Facts.Core;

public class FactPack
{
	public FactsPackHeader Header { get; }

	public byte[] Entries { get; }

	public byte[] Data { get; }

	public FactPack(FactsPackHeader header, byte[] entries, byte[] data)
	{
		Header = header;
		Entries = entries;
		Data = data;
	}
}
