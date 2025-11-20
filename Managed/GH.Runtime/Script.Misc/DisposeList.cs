using System;
using System.Collections.Generic;

namespace Script.Misc;

public class DisposeList : List<IDisposable>, IDisposable
{
	public void Dispose()
	{
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				enumerator.Current.Dispose();
			}
		}
		Clear();
	}
}
