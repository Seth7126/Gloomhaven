using UnityEngine;

public class PhysicsController : Singleton<PhysicsController>
{
	[SerializeField]
	private float _simplifyPhysicsRate;

	private float _originalPhysicsRate;

	public float SimplifyPhysicsRate => _simplifyPhysicsRate;

	protected override void Awake()
	{
		base.Awake();
		_originalPhysicsRate = 1f / Time.fixedDeltaTime;
	}

	public void Setup(bool applyOptimizationIfPossible)
	{
		if (PlatformLayer.Setting.SimplifyPhysics)
		{
			if (applyOptimizationIfPossible)
			{
				SetPhysicsFrameRate(_simplifyPhysicsRate);
				Physics.autoSyncTransforms = false;
			}
			else
			{
				SetPhysicsFrameRate(_originalPhysicsRate);
			}
		}
	}

	public void SimplifyPhysics()
	{
		Physics.autoSyncTransforms = false;
		SetPhysicsFrameRate(_simplifyPhysicsRate);
		SetClothesFrequency(_simplifyPhysicsRate);
	}

	private void SetPhysicsFrameRate(float frameRate)
	{
		Time.fixedDeltaTime = 1f / frameRate;
	}

	public void RestorePhysics()
	{
		Physics.autoSyncTransforms = true;
		SetClothesFrequency(120f);
		SetPhysicsFrameRate(_originalPhysicsRate);
	}

	private void SetClothesFrequency(float frequency)
	{
		Cloth[] array = Object.FindObjectsOfType<Cloth>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].clothSolverFrequency = frequency;
		}
	}
}
