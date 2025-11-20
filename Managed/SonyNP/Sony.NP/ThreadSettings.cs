namespace Sony.NP;

public struct ThreadSettings
{
	public Affinity affinity;

	public void Init()
	{
		affinity = Affinity.AllCores;
	}
}
