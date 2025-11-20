public struct AssetRequest
{
	public int EntityContext;

	public string Descriptor;

	public int ID;

	public string PendingAssetPath;

	public bool IsWaiting => !string.IsNullOrEmpty(PendingAssetPath);

	public void StartWaiting(string asset_path)
	{
		PendingAssetPath = asset_path;
	}

	public void StopWaiting()
	{
		PendingAssetPath = null;
	}
}
