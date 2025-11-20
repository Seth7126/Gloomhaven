using System;
using UnityEngine;

namespace Photon.Bolt.Tokens;

[CreateAssetMenu(fileName = "BoltProtocolTokenRegistry", menuName = "Bolt/Create Protocol Token Registry")]
public class BoltProtocolTokenRegistry : ScriptableObject
{
	[Serializable]
	internal struct TokenRegistry
	{
		public string DisplayName;

		public string AssemblyName;
	}

	private static BoltProtocolTokenRegistry _instance;

	[SerializeField]
	internal TokenRegistry[] protocolTokenRegistry = new TokenRegistry[0];

	internal static BoltProtocolTokenRegistry Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = (BoltProtocolTokenRegistry)Resources.Load("BoltProtocolTokenRegistry", typeof(BoltProtocolTokenRegistry));
			}
			return _instance;
		}
	}
}
