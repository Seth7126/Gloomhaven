using System;

namespace MonoMod.Utils;

internal static class _DataHelper_
{
	private sealed class CollectionDummy
	{
		~CollectionDummy()
		{
			GC.ReRegisterForFinalize(this);
			_DataHelper_.Collected?.Invoke();
		}
	}

	public static event Action Collected;

	static _DataHelper_()
	{
		new CollectionDummy();
	}
}
