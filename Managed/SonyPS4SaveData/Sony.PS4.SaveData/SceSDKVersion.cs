namespace Sony.PS4.SaveData;

public struct SceSDKVersion
{
	public uint Major;

	public uint Minor;

	public uint Patch;

	public override string ToString()
	{
		return Major.ToString("X2") + "." + Minor.ToString("X3") + "." + Patch.ToString("X3");
	}
}
