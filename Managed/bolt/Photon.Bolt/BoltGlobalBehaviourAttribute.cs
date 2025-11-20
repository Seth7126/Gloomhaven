using System;

namespace Photon.Bolt;

[Documentation]
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class BoltGlobalBehaviourAttribute : Attribute
{
	public BoltNetworkModes Mode { get; private set; }

	public string[] Scenes { get; private set; }

	public BoltGlobalBehaviourAttribute()
		: this(BoltNetworkModes.Shutdown)
	{
	}

	public BoltGlobalBehaviourAttribute(BoltNetworkModes mode)
		: this(mode, new string[0])
	{
	}

	public BoltGlobalBehaviourAttribute(params string[] scenes)
		: this(BoltNetworkModes.Shutdown, scenes)
	{
	}

	public BoltGlobalBehaviourAttribute(BoltNetworkModes mode, params string[] scenes)
	{
		Mode = mode;
		Scenes = scenes;
	}
}
