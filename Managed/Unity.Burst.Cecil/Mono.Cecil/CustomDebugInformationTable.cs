using Mono.Cecil.Metadata;

namespace Mono.Cecil;

internal sealed class CustomDebugInformationTable : MetadataTable<Row<uint, uint, uint>>
{
	public override void Write(TableHeapBuffer buffer)
	{
		for (int i = 0; i < length; i++)
		{
			buffer.WriteCodedRID(rows[i].Col1, CodedIndex.HasCustomDebugInformation);
			buffer.WriteGuid(rows[i].Col2);
			buffer.WriteBlob(rows[i].Col3);
		}
	}
}
