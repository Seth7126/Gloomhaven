using JetBrains.Annotations;
using UnityEngine;

namespace UnityStandardAssets.Utility.Ambience;

[CreateAssetMenu(fileName = "AmbienceConfig", menuName = "Ambience config", order = 102)]
public class AmbienceConfig : ScriptableObject
{
	[SerializeField]
	[UsedImplicitly]
	private float _exposure;

	public float Exposure => _exposure;
}
