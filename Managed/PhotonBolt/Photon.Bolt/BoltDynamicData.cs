using System;
using System.Collections.Generic;
using System.Reflection;
using Photon.Bolt.Collections;
using Photon.Bolt.Internal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

namespace Photon.Bolt;

[Preserve]
public static class BoltDynamicData
{
	public static void Setup()
	{
		BoltNetworkInternal.DebugDrawer = new UnityDebugDrawer();
		BoltNetworkInternal.UsingUnityPro = true;
		BoltNetworkInternal.GetActiveSceneIndex = GetActiveSceneIndex;
		BoltNetworkInternal.GetSceneName = GetSceneName;
		BoltNetworkInternal.GetSceneIndex = GetSceneIndex;
		BoltNetworkInternal.GetGlobalBehaviourTypes = GetGlobalBehaviourTypes;
		BoltNetworkInternal.EnvironmentSetup = BoltNetworkInternal_User.EnvironmentSetup;
		BoltNetworkInternal.EnvironmentReset = BoltNetworkInternal_User.EnvironmentReset;
		UnitySettings.IsBuildMono = true;
		UnitySettings.CurrentPlatform = Application.platform;
	}

	private static int GetActiveSceneIndex()
	{
		return GetSceneIndex(SceneManager.GetActiveScene().name);
	}

	private static int GetSceneIndex(string name)
	{
		try
		{
			return BoltScenes_Internal.GetSceneIndex(name);
		}
		catch
		{
			return -1;
		}
	}

	private static string GetSceneName(int index)
	{
		try
		{
			return BoltScenes_Internal.GetSceneName(index);
		}
		catch
		{
			return null;
		}
	}

	private static List<STuple<BoltGlobalBehaviourAttribute, Type>> GetGlobalBehaviourTypes()
	{
		List<STuple<BoltGlobalBehaviourAttribute, Type>> list = new List<STuple<BoltGlobalBehaviourAttribute, Type>>();
		IEnumerator<string> asmIter = BoltAssemblies.AllAssemblies;
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		while (asmIter.MoveNext())
		{
			try
			{
				Assembly assembly = Array.Find(assemblies, (Assembly assembly2) => assembly2.GetName().Name.Equals(asmIter.Current));
				if (assembly == null)
				{
					continue;
				}
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					try
					{
						if (typeof(MonoBehaviour).IsAssignableFrom(type))
						{
							BoltGlobalBehaviourAttribute customAttribute = type.GetCustomAttribute<BoltGlobalBehaviourAttribute>(inherit: false);
							if (customAttribute != null)
							{
								list.Add(new STuple<BoltGlobalBehaviourAttribute, Type>(customAttribute, type));
							}
						}
					}
					catch (Exception)
					{
					}
				}
			}
			catch (Exception)
			{
			}
		}
		return list;
	}
}
