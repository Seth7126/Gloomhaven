using UnityEngine;

public sealed class CycledModePaginationStrategy : IModePaginationStrategy
{
	public int Paginate(int totalActiveButtons, int offset, int lastModeIndex)
	{
		lastModeIndex += offset;
		if (lastModeIndex >= totalActiveButtons)
		{
			lastModeIndex %= totalActiveButtons;
		}
		if (lastModeIndex < 0)
		{
			lastModeIndex = totalActiveButtons - Mathf.Abs(lastModeIndex) % totalActiveButtons;
		}
		return lastModeIndex;
	}
}
