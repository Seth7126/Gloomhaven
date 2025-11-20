using System;

namespace Mono.Cecil.Cil;

internal interface IMetadataSymbolWriter : IDisposable, ISymbolWriter
{
	void SetMetadata(MetadataBuilder metadata);

	void WriteModule();
}
