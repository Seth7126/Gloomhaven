namespace Photon.Bolt;

internal struct PropertyMecanimSettings
{
	public MecanimMode Mode;

	public MecanimDirection Direction;

	public int Layer;

	public float Damping;

	public bool Enabled => Mode != MecanimMode.Disabled;
}
