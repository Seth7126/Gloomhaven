using System.Collections.Generic;
using UnityEngine;

namespace Script.GUI.SMNavigation.Tabs;

public sealed class TabComponentMultiInputListener : TabComponentInputListener
{
	[SerializeField]
	private List<KeyAction> _nextKeys;

	[SerializeField]
	private List<KeyAction> _prevKeys;

	public override void Register()
	{
		base.Register();
		_nextKeys.ForEach(delegate(KeyAction x)
		{
			RegisterAction(x, Next);
		});
		_prevKeys.ForEach(delegate(KeyAction x)
		{
			RegisterAction(x, Previous);
		});
	}

	public override void UnRegister()
	{
		base.UnRegister();
		_nextKeys.ForEach(delegate(KeyAction x)
		{
			UnregisterAction(x, Next);
		});
		_prevKeys.ForEach(delegate(KeyAction x)
		{
			UnregisterAction(x, Previous);
		});
	}
}
