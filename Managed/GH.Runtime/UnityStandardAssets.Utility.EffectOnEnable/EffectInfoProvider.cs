using JetBrains.Annotations;
using UnityEngine;

namespace UnityStandardAssets.Utility.EffectOnEnable;

public class EffectInfoProvider : MonoBehaviour
{
	[SerializeField]
	[UsedImplicitly]
	private ParticleSystem _firstParticleSystem;

	[SerializeField]
	[UsedImplicitly]
	private ParticleSystem _secondParticleSystem;

	public ParticleSystem FirstParticleSystem => _firstParticleSystem;

	public ParticleSystem SecondParticleSystem => _secondParticleSystem;
}
