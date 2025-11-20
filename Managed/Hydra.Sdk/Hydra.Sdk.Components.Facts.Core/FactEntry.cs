using System;
using Hydra.Api.Facts;
using Hydra.Sdk.Helpers;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Components.Facts.Core;

public class FactEntry : IHydraEntry
{
	public string Category { get; private set; }

	public FactsContext Context { get; private set; }

	public string Description { get; private set; }

	public object[] Args { get; private set; }

	public DateTime Time { get; private set; }

	public int Size { get; private set; }

	public FactEntry(string category, FactsContext context, string description, params object[] args)
	{
		Category = category;
		Context = context;
		Description = description;
		Args = args ?? new string[1] { string.Empty };
		Time = DateTime.UtcNow;
		Size = EntrySizeCalculator.GetSize(Category) + ((Context != null) ? Context.CalculateSize() : 0) + EntrySizeCalculator.GetSize(Description) + EntrySizeCalculator.GetSize(Args) + 12;
	}

	public int GetSize()
	{
		return Size;
	}
}
