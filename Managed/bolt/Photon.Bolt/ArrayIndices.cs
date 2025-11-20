using System;

namespace Photon.Bolt;

public struct ArrayIndices
{
	private readonly int[] indices;

	public int Length => (indices != null) ? indices.Length : 0;

	[Documentation(Ignore = true)]
	public int this[int index]
	{
		get
		{
			if (index < 0 || index >= Length)
			{
				throw new IndexOutOfRangeException();
			}
			return indices[index];
		}
	}

	internal ArrayIndices(int[] indices)
	{
		this.indices = indices;
	}
}
