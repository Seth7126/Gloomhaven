using UnityEngine;

namespace Script.PlatformLayer;

public class ActivitiesLinker : MonoBehaviour
{
	[SerializeField]
	private GhActivity[] _activities;

	[SerializeField]
	private GhChallenge[] _challenges;

	public static ActivitiesLinker Instance { get; private set; }

	public GhActivity[] Activities => _activities;

	public GhChallenge[] Challenges => _challenges;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Object.Destroy(this);
		}
	}
}
