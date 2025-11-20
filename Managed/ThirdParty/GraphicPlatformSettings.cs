using UnityEngine;

[CreateAssetMenu(fileName = "Graphic Platform Setting Data", menuName = "Data/Platform Settings/Graphic Platform Setting Data")]
public class GraphicPlatformSettings : ScriptableObject
{
	public bool _castPlayerCharactersShadows;

	public bool _castEnemyShadows;

	public bool _castPropObjectsShadows;

	public bool _castImportantObjectsShadows;
}
