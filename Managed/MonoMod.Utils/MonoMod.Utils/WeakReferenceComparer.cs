using System;
using System.Collections.Generic;

namespace MonoMod.Utils;

public sealed class WeakReferenceComparer : EqualityComparer<WeakReference>
{
	public override bool Equals(WeakReference x, WeakReference y)
	{
		if (x.IsAlive == y.IsAlive)
		{
			if (!x.IsAlive)
			{
				return x == y;
			}
			return x.Target == y.Target;
		}
		return false;
	}

	public override int GetHashCode(WeakReference obj)
	{
		return 0;
	}
}
