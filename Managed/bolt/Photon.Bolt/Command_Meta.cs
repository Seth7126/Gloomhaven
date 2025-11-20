namespace Photon.Bolt;

internal abstract class Command_Meta : NetworkObj_Meta
{
	internal bool IsInstant;

	internal bool LimitOnePerFrame;

	internal int SmoothFrames;

	internal bool CompressZeroValues;

	internal bool EnableInputDeltaCompression;

	internal bool EnableResultDeltaCompression;
}
