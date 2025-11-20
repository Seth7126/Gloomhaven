using UnityEngine;

public sealed class DefaultModePaginationStrategy : IModePaginationStrategy
{
	public int Paginate(int totalActiveButtons, int offset, int lastModeIndex)
	{
		return Mathf.Clamp(lastModeIndex + offset, 0, totalActiveButtons - 1);
	}
}
