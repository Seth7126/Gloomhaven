using System;

namespace Utilities.MemoryManager;

[Serializable]
public enum AsyncLoadingState
{
	Finished,
	Failed,
	Canceled
}
