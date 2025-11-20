using System.IO;
using Mono.Cecil.Cil;
using Mono.CompilerServices.SymbolWriter;

namespace Mono.Cecil.Mdb;

public sealed class MdbReaderProvider : ISymbolReaderProvider
{
	public ISymbolReader GetSymbolReader(ModuleDefinition module, string fileName)
	{
		Mixin.CheckModule(module);
		Mixin.CheckFileName(fileName);
		return new MdbReader(module, MonoSymbolFile.ReadSymbolFile(Mixin.GetMdbFileName(fileName), module.Mvid));
	}

	public ISymbolReader GetSymbolReader(ModuleDefinition module, Stream symbolStream)
	{
		Mixin.CheckModule(module);
		Mixin.CheckStream(symbolStream);
		MonoSymbolFile monoSymbolFile = MonoSymbolFile.ReadSymbolFile(symbolStream);
		if (module.Mvid != monoSymbolFile.Guid)
		{
			if (symbolStream is FileStream fileStream)
			{
				throw new MonoSymbolFileException("Symbol file `{0}' does not match assembly", fileStream.Name);
			}
			throw new MonoSymbolFileException("Symbol file from stream does not match assembly");
		}
		return new MdbReader(module, monoSymbolFile);
	}
}
