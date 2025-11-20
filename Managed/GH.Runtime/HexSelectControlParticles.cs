using UnityEngine;

public class HexSelectControlParticles : MonoBehaviour
{
	[field: SerializeField]
	public MeshRenderer UnseenGroundPlane { get; set; }

	[field: SerializeField]
	public ParticleSystem[] ParticleBits { get; set; }

	[field: Header("!Same order as - ScenarioManager.EAdjacentPosition")]
	[field: SerializeField]
	public ParticleSystem[] ParticleHover { get; set; }
}
