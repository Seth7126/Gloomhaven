using System;

namespace OdinSerializer.Utilities;

public interface ICache : IDisposable
{
	object Value { get; }
}
