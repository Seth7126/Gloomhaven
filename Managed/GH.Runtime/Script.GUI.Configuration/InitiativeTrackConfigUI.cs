using UnityEngine;

namespace Script.GUI.Configuration;

[CreateAssetMenu(menuName = "UI Config/InitiativeTrack")]
public class InitiativeTrackConfigUI : ScriptableObject
{
	[SerializeField]
	[Range(0f, 1f)]
	private float _minimalEnemyAvatarDesiredWidth = 0.5f;

	[Tooltip("All actors including mercenaries, enemies, allies, etc. If this number is exceeded, the enemy portraits will be minimized.")]
	[SerializeField]
	[Min(0f)]
	private int _actorsNumberBorder = 12;

	[SerializeField]
	private float _startLeftEnemyAvatarBound;

	[SerializeField]
	private float _startRightEnemyAvatarBound;

	public float StartLeftEnemyAvatarBound => _startLeftEnemyAvatarBound;

	public float StartRightEnemyAvatarBound => _startRightEnemyAvatarBound;

	public int ActorsNumberBorder => _actorsNumberBorder;

	public float MinimalEnemyAvatarDesiredWidth => _minimalEnemyAvatarDesiredWidth;
}
