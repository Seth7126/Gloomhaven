namespace Photon.Bolt;

internal struct PropertyExtrapolationSettings
{
	public bool Enabled;

	public int MaxFrames;

	public float ErrorTolerance;

	public float SnapMagnitude;

	public ExtrapolationVelocityModes VelocityMode;

	public static PropertyExtrapolationSettings Create(int maxFrames, float errorTolerance, float snapMagnitude, ExtrapolationVelocityModes velocityMode)
	{
		PropertyExtrapolationSettings result = default(PropertyExtrapolationSettings);
		result.Enabled = true;
		result.MaxFrames = maxFrames;
		result.ErrorTolerance = errorTolerance;
		result.SnapMagnitude = snapMagnitude;
		result.VelocityMode = velocityMode;
		return result;
	}
}
