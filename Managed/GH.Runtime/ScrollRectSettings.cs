using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "UI Config/Scroll Settings")]
public class ScrollRectSettings : ScriptableObject
{
	public ScrollRect.MovementType MovementType = ScrollRect.MovementType.Elastic;

	[ConditionalField("MovementType", "Elastic", true)]
	public float Elasticity = 0.1f;

	public bool Inertia = true;

	[ConditionalField("Inertia", "true", true)]
	public float DecelerationRate = 0.135f;

	public float ScrollSensitivity = 1f;
}
