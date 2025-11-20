using System;
using System.Collections.Generic;
using System.Reflection;
using Photon.Bolt.Collections;
using UdpKit;
using UdpKit.Platform;
using UnityEngine;

namespace Photon.Bolt.Internal;

[Documentation(Ignore = true)]
public static class BoltNetworkInternal
{
	public static bool UsingUnityPro;

	public static Action EnvironmentSetup;

	public static Action EnvironmentReset;

	public static IDebugDrawer DebugDrawer;

	public static Func<int, string> GetSceneName;

	public static Func<string, int> GetSceneIndex;

	public static Func<int> GetActiveSceneIndex;

	public static Func<List<STuple<BoltGlobalBehaviourAttribute, Type>>> GetGlobalBehaviourTypes;

	private static bool Setup()
	{
		Type type = Type.GetType("Photon.Bolt.BoltDynamicData, PhotonBolt");
		if (type != null)
		{
			MethodInfo method = type.GetMethod("Setup");
			if (method != null)
			{
				method.Invoke(null, null);
				return true;
			}
		}
		return false;
	}

	internal static void Initialize(BoltNetworkModes mode, UdpEndPoint endpoint, BoltConfig config, UdpPlatform platform = null, string autoloadScene = null)
	{
		if (!Setup())
		{
			Debug.LogError("Initialization Error. Verify if the class 'Photon.Bolt.BoltDynamicData' exists on your build and it was not stripped.");
		}
		else
		{
			BoltCore.Initialize(mode, endpoint, config, platform, autoloadScene);
		}
	}
}
