using System;
using UnityEngine.UI;

public class NavigationCalculator : INavigationCalculator
{
	public Func<Selectable> up;

	public Func<Selectable> down;

	public Func<Selectable> right;

	public Func<Selectable> left;

	public Selectable FindLeft()
	{
		return left?.Invoke();
	}

	public Selectable FindRight()
	{
		return right?.Invoke();
	}

	public Selectable FindUp()
	{
		return up?.Invoke();
	}

	public Selectable FindDown()
	{
		return down?.Invoke();
	}

	public NavigationCalculator Copy()
	{
		return new NavigationCalculator
		{
			up = up,
			down = down,
			right = right,
			left = left
		};
	}
}
