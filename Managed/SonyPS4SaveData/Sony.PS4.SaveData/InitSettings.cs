namespace Sony.PS4.SaveData;

public struct InitSettings
{
	internal ThreadAffinity affinity;

	public ThreadAffinity Affinity
	{
		get
		{
			return affinity;
		}
		set
		{
			affinity = value;
		}
	}

	public void Init()
	{
		affinity = ThreadAffinity.AllCores;
	}
}
