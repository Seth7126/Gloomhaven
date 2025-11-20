using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class TooltipsVisibilityHelper : MonoBehaviour
{
	private readonly HashSet<object> _hideRequests = new HashSet<object>();

	public Action HideTooltipsEvent;

	public Action ShowTooltipsEvent;

	public static TooltipsVisibilityHelper Instance { get; private set; }

	[UsedImplicitly]
	private void Awake()
	{
		Instance = this;
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		Instance = null;
		_hideRequests.Clear();
	}

	public bool TooltipsIsHidden()
	{
		return _hideRequests.Count > 0;
	}

	public bool TooltipsIsHiddenBy(object requestSource)
	{
		return _hideRequests.Contains(requestSource);
	}

	public void HideTooltips(object requestSource)
	{
		if (!_hideRequests.Contains(requestSource))
		{
			_hideRequests.Add(requestSource);
			if (_hideRequests.Count == 1)
			{
				HideTooltipsEvent?.Invoke();
			}
		}
	}

	public void ShowTooltips(object requestSource)
	{
		if (_hideRequests.Contains(requestSource))
		{
			_hideRequests.Remove(requestSource);
			if (_hideRequests.Count == 0)
			{
				ShowTooltipsEvent?.Invoke();
			}
		}
	}

	public void RemoveTooltipRequest(object requestSource)
	{
		if (_hideRequests.Contains(requestSource))
		{
			_hideRequests.Remove(requestSource);
		}
	}

	public void ToggleTooltips(object source)
	{
		if (TooltipsIsHiddenBy(source))
		{
			ShowTooltips(source);
		}
		else
		{
			HideTooltips(source);
		}
	}
}
