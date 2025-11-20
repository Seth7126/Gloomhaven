using System;
using System.IO;

namespace Mono.Cecil.Cil;

public class DefaultSymbolReaderProvider : ISymbolReaderProvider
{
	private readonly bool throw_if_no_symbol;

	public DefaultSymbolReaderProvider()
		: this(throwIfNoSymbol: true)
	{
	}

	public DefaultSymbolReaderProvider(bool throwIfNoSymbol)
	{
		throw_if_no_symbol = throwIfNoSymbol;
	}

	public ISymbolReader GetSymbolReader(ModuleDefinition module, string fileName)
	{
		if (module.Image.HasDebugTables())
		{
			return null;
		}
		if (module.HasDebugHeader && module.GetDebugHeader().GetEmbeddedPortablePdbEntry() != null)
		{
			return new EmbeddedPortablePdbReaderProvider().GetSymbolReader(module, fileName);
		}
		if (File.Exists(Mixin.GetPdbFileName(fileName)))
		{
			if (Mixin.IsPortablePdb(Mixin.GetPdbFileName(fileName)))
			{
				return new PortablePdbReaderProvider().GetSymbolReader(module, fileName);
			}
			try
			{
				return SymbolProvider.GetReaderProvider(SymbolKind.NativePdb).GetSymbolReader(module, fileName);
			}
			catch (TypeLoadException)
			{
			}
		}
		if (File.Exists(Mixin.GetMdbFileName(fileName)))
		{
			try
			{
				return SymbolProvider.GetReaderProvider(SymbolKind.Mdb).GetSymbolReader(module, fileName);
			}
			catch (TypeLoadException)
			{
			}
		}
		if (throw_if_no_symbol)
		{
			throw new FileNotFoundException($"No symbol found for file: {fileName}");
		}
		return null;
	}

	public static SymbolReaderKind GetSymbolReaderKind(string fileName)
	{
		try
		{
			using AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(fileName);
			if (assemblyDefinition.Modules.Count <= 0)
			{
				return SymbolReaderKind.None;
			}
			ModuleDefinition module = assemblyDefinition.Modules[0];
			return new DefaultSymbolReaderProvider().GetSymbolReaderKind(module, fileName);
		}
		catch (Exception)
		{
			return SymbolReaderKind.None;
		}
	}

	public SymbolReaderKind GetSymbolReaderKind(ModuleDefinition module, string fileName)
	{
		if (module.Image.HasDebugTables())
		{
			return SymbolReaderKind.None;
		}
		if (File.Exists(Mixin.GetPdbFileName(fileName)))
		{
			if (!Mixin.IsPortablePdb(Mixin.GetPdbFileName(fileName)))
			{
				return SymbolReaderKind.NativePdb;
			}
			return SymbolReaderKind.PortablePdb;
		}
		if (File.Exists(Mixin.GetMdbFileName(fileName)))
		{
			return SymbolReaderKind.Mdb;
		}
		return SymbolReaderKind.None;
	}

	public ISymbolReader GetSymbolReader(ModuleDefinition module, Stream symbolStream)
	{
		throw new NotSupportedException();
	}
}
