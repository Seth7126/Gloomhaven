namespace Photon.Bolt.Internal;

public static class BoltScenes_Internal
{
	public static int GetSceneIndex(string name)
	{
		return BoltScenes.nameLookup[name];
	}

	public static string GetSceneName(int index)
	{
		return BoltScenes.indexLookup[index];
	}
}
