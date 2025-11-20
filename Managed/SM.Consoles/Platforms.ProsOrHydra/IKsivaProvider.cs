using System;

namespace Platforms.ProsOrHydra;

public interface IKsivaProvider
{
	string Ksiva { get; }

	event Action<string> EventKsivaReceived;
}
