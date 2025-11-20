using UnityEngine;

[CreateAssetMenu(fileName = "Procedural Content Platform Data", menuName = "Data/Platform Settings/Procedural Content Platform Data")]
public class ProceduralContentPlatformData : ScriptableObject
{
	public int _maxFramesToWaitContentCompleteness;

	public bool _isWaitForGeneration;
}
